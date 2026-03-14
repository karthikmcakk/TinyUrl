namespace TinyUrl.Api.Models;

public class Url
{
    public int Id { get; set; }

    public string OriginalUrl { get; set; } = string.Empty;

    public string ShortCode { get; set; } = string.Empty;

    public bool IsPrivate { get; set; }

    public int Clicks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}