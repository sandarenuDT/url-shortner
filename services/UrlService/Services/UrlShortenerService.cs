using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlService.Data;
using UrlService.Models;

namespace UrlService.Services;

public class UrlShortenerService
{
    private readonly AppDbContext _db;
    private readonly IDatabase _redis;
    private readonly EventPublisher _publisher;
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public UrlShortenerService(AppDbContext db, IConnectionMultiplexer redis,
        EventPublisher publisher)
    {
        _db = db;
        _redis = redis.GetDatabase();
        _publisher = publisher;
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        var random = new Random();
        string code;
        do
        {
            code = new string(Enumerable.Range(0, 6)
                .Select(_ => Chars[random.Next(Chars.Length)]).ToArray());
        } while (await _db.ShortUrls.AnyAsync(u => u.ShortCode == code));
        return code;
    }

    public async Task<ShortUrl> CreateAsync(string originalUrl, int userId, 
        DateTime? expiresAt)
    {
        var shortUrl = new ShortUrl
        {
            OriginalUrl = originalUrl,
            ShortCode = await GenerateUniqueCodeAsync(),
            UserId = userId,
            ExpiresAt = expiresAt
        };

        _db.ShortUrls.Add(shortUrl);
        await _db.SaveChangesAsync();

        await _redis.StringSetAsync(shortUrl.ShortCode, originalUrl, 
            TimeSpan.FromHours(24));

        return shortUrl;
    }

    public async Task<string?> ResolveAsync(string shortCode, string? ipAddress,
        string? userAgent, string? referer)
    {
        // 1. Check Redis first (fast)
        var cached = await _redis.StringGetAsync(shortCode);
        if (cached.HasValue)
        {
            // Publish click event to RabbitMQ
            _publisher.PublishClickEvent(shortCode, ipAddress, userAgent, referer);
            return cached.ToString();
        }

        // 2. Fall back to database
        var shortUrl = await _db.ShortUrls
            .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

        if (shortUrl == null) return null;
        if (shortUrl.ExpiresAt.HasValue && shortUrl.ExpiresAt < DateTime.UtcNow) 
            return null;

        shortUrl.ClickCount++;
        await _db.SaveChangesAsync();

        await _redis.StringSetAsync(shortCode, shortUrl.OriginalUrl, 
            TimeSpan.FromHours(24));

        // Publish click event to RabbitMQ
        _publisher.PublishClickEvent(shortCode, ipAddress, userAgent, referer);

        return shortUrl.OriginalUrl;
    }

    public async Task<List<ShortUrl>> GetByUserAsync(int userId)
    {
        return await _db.ShortUrls
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var url = await _db.ShortUrls
            .FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);

        if (url == null) return false;

        _db.ShortUrls.Remove(url);
        await _db.SaveChangesAsync();
        await _redis.KeyDeleteAsync(url.ShortCode);
        return true;
    }
}