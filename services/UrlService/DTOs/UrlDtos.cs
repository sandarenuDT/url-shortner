
namespace UrlService.DTOs;
public class CreateUrlDto
{
    public string OriginalUrl { get; set;} = string.Empty;
    public DateTime? ExpiresAt { get; set;}
}

public class UrlResponseDto
{
    public int Id { get; set;}
    public string ShortCode { get; set; } = string.Empty;
    public string OriginalUrl { get; set;} = string.Empty;
    public string ShortUrl { get; set;} = string.Empty;
    public int ClickCount { get; set;} 
    public DateTime CreatedAt { get; set;}
    public DateTime? ExpiresAt { get; set;}
}