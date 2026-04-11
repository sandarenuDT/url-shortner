using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace UrlService.Services;

public class EventPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IConfiguration config, ILogger<EventPublisher> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"]!,
            UserName = config["RabbitMQ:Username"]!,
            Password = config["RabbitMQ:Password"]!
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare queue (creates if not exists)
        _channel.QueueDeclare(
            queue: "click-events",
            durable: true,
            exclusive: false,
            autoDelete: false);
    }

    public void PublishClickEvent(string shortCode, string? ipAddress, 
                                   string? userAgent, string? referer)
    {
        var message = new
        {
            ShortCode = shortCode,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Referer = referer
        };

        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true; // message survives RabbitMQ restart

        _channel.BasicPublish(
            exchange: "",
            routingKey: "click-events",
            basicProperties: properties,
            body: body);

        _logger.LogInformation("📤 Click event published for {ShortCode}", shortCode);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}