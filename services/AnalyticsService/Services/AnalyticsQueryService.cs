using AnalyticsService.Data;
using AnalyticsService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Services;

public class AnalyticsQueryService
{
    private readonly AppDbContext _db;

    public AnalyticsQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(string shortCode)
    {
        var events = await _db.ClickEvents
            .Where(c => c.ShortCode == shortCode)
            .ToListAsync();

        var clicksByDay = events
            .GroupBy(c => c.ClickedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyClickDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Clicks = g.Count()
            }).ToList();

        var topReferers = events
            .Where(c => !string.IsNullOrEmpty(c.Referer))
            .GroupBy(c => c.Referer!)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new RefererDto
            {
                Referer = g.Key,
                Count = g.Count()
            }).ToList();

        return new AnalyticsSummaryDto
        {
            ShortCode = shortCode,
            TotalClicks = events.Count,
            ClicksByDay = clicksByDay,
            TopReferers = topReferers
        };
    }
}