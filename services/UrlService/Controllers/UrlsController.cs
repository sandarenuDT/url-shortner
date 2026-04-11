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

    // POST api/urls → Create a short URL
    [HttpPost]
    public async Task<IActionResult> Create(CreateUrlDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OriginalUrl))
            return BadRequest(new { message = "URL is required." });

        if (!Uri.TryCreate(dto.OriginalUrl, UriKind.Absolute, out _))
            return BadRequest(new { message = "Invalid URL format." });

        var shortUrl = await _urlService.CreateAsync(
            dto.OriginalUrl, GetUserId(), dto.ExpiresAt);

        return CreatedAtAction(nameof(GetById), new { id = shortUrl.Id },
            new UrlResponseDto
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

    // GET api/urls → Get all URLs for logged-in user
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

    // GET api/urls/{id} → Get single URL by id
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

    // DELETE api/urls/{id} → Delete a URL
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _urlService.DeleteAsync(id, GetUserId());
        if (!deleted) return NotFound();
        return NoContent();
    }

    // // GET /r/{code} → Redirect to original URL (no auth needed)
    // [AllowAnonymous]
    // [HttpGet("/r/{code}")]
    // public async Task<IActionResult> RedirectToUrl(string code)
    // {
    //     var originalUrl = await _urlService.ResolveAsync(code);
    //     if (originalUrl == null)
    //         return NotFound(new { message = "Short URL not found or expired." });

    //     return new RedirectResult(originalUrl, permanent: false);
    // }
    [AllowAnonymous]
    [HttpGet("/r/{code}")]
    public async Task<IActionResult> RedirectToUrl(string code)
    {
        // Capture request info for analytics
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();
        var referer = Request.Headers["Referer"].ToString();

        var originalUrl = await _urlService.ResolveAsync(code, ipAddress, userAgent, referer);
        if (originalUrl == null)
            return NotFound(new { message = "Short URL not found or expired." });

        return new RedirectResult(originalUrl, permanent: false);
    }
}