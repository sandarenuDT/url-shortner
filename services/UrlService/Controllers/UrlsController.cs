using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlService.DTOs;
using UrlService.Services;

namespace UrlService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UrlsController : ControllerBase
{
    private readonly UrlShortenerService _urlService;
    private readonly IConfiguration _config;

    public UrlsController(UrlShortenerService urlService, IConfiguration config)
    {
        _urlService = urlService;
        _config = config;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string BuildShortUrl(string code) =>
        $"{_config["BaseUrl"]}/{code}";
        
    [HttpPost]
    public async Task<IActionResult> Create(CreateUrlDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OriginalUrl))
            return BadRequest(new { message = "URL is required." });

        if (!Uri.TryCreate(dto.OriginalUrl, UriKind.Absolute, out _))
            return BadRequest(new { message = "Invalid URL format." });

        var shortUrl = await _urlService.CreateAsync(dto.OriginalUrl, GetUserId(), dto.ExpiresAt);

        return CreatedAtAction(nameof(GetById), new { id = shortUrl.Id }, new UrlResponseDto
        {
            Id = shortUrl.Id,
            OriginalUrl = shortUrl.OriginalUrl,
            ShortCode = shortUrl.ShortCode,
            ShortUrl = BuildShortUrl(shortUrl.ShortCode),
            ClickCount = shortUrl.ClickCount,
            CreatedAt = shortUrl.CreatedAt,
            ExpiresAt = shortUrl.ExpiresAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var urls = await _urlService.GetByUserAsync(GetUserId());

        var result = urls.Select(u => new UrlResponseDto
        {
            Id = u.Id,
            OriginalUrl = u.OriginalUrl,
            ShortCode = u.ShortCode,
            ShortUrl = BuildShortUrl(u.ShortCode),
            ClickCount = u.ClickCount,
            CreatedAt = u.CreatedAt,
            ExpiresAt = u.ExpiresAt
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var urls = await _urlService.GetByUserAsync(GetUserId());
        var url = urls.FirstOrDefault(u => u.Id == id);
        if (url == null) return NotFound();

        return Ok(new UrlResponseDto
        {
            Id = url.Id,
            OriginalUrl = url.OriginalUrl,
            ShortCode = url.ShortCode,
            ShortUrl = BuildShortUrl(url.ShortCode),
            ClickCount = url.ClickCount,
            CreatedAt = url.CreatedAt,
            ExpiresAt = url.ExpiresAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _urlService.DeleteAsync(id, GetUserId());
        if (!deleted) return NotFound();
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("/r/{code}")]
    public new async Task<IActionResult> Redirect(string code)
    {
        var originalUrl = await _urlService.ResolveAsync(code);
        if (originalUrl == null)
            return NotFound(new { message = "Short URL not found or expired." });

        return await Redirect(originalUrl);
    }
}
