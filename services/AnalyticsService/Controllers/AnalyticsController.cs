using AnalyticsService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsQueryService _analytics;

    public AnalyticsController(AnalyticsQueryService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> GetSummary(string shortCode)
    {
        var summary = await _analytics.GetSummaryAsync(shortCode);
        return Ok(summary);
    }
}