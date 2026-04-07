namespace AnalyticsService.Models;

public class ClickEvent
{
    public int Id { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referer { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
}