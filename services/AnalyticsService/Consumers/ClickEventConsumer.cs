using System.Text;
using AnalyticsService.Data;
using AnalyticsService.DTOs;
using AnalyticsService.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AnalyticsService.Consumers;

public class ClickEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ClickEventConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public ClickEventConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<ClickEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for RabbitMQ to be ready
        await Task.Delay(5000, stoppingToken);

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Host"]!,
                UserName = _config["RabbitMQ:Username"]!,
                Password = _config["RabbitMQ:Password"]!
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "click-events",
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert.DeserializeObject<ClickEventMessage>(body);

                if (message != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    db.ClickEvents.Add(new ClickEvent
                    {
                        ShortCode = message.ShortCode,
                        IpAddress = message.IpAddress,
                        UserAgent = message.UserAgent,
                        Referer = message.Referer
                    });

                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("✅ Click recorded for {ShortCode}", message.ShortCode);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(
                queue: "click-events",
                autoAck: false,
                consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ consumer error");
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
