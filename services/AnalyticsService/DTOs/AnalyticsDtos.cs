namespace AnalyticsService.DTOs;

public class ClickEventMessage
{
    public string ShortCode { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referer { get; set; }
}

public class AnalyticsSummaryDto
{
    public string ShortCode { get; set; } = string.Empty;
    public int TotalClicks { get; set; }
    public List<DailyClickDto> ClicksByDay { get; set; } = new();
    public List<RefererDto> TopReferers { get; set; } = new();
}

public class DailyClickDto
{
    public string Date { get; set; } = string.Empty;
    public int Clicks { get; set; }
}

public class RefererDto
{
    public string Referer { get; set; } = string.Empty;
    public int Count { get; set; }
}