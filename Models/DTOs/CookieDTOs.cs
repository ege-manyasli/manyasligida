using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models.DTOs;

// Request DTOs
public record CookieConsentRequest
{
    [Required]
    public string SessionId { get; init; } = string.Empty;
    
    public int? UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public bool IsAccepted { get; init; }
    public Dictionary<string, bool> CategoryPreferences { get; init; } = new();
    
    public bool AcceptAll { get; init; }
    public List<CookieCategoryConsentRequest> CategoryConsents { get; init; } = new();
}

public record CookieCategoryConsentRequest
{
    [Required]
    public int CategoryId { get; init; }
    
    [Required]
    public bool IsAccepted { get; init; }
}

public record CookieSettingsUpdateRequest
{
    [Required]
    public List<CookieCategoryConsentRequest> CategoryConsents { get; init; } = new();
}

// Response DTOs
public record CookieConsentResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ConsentId { get; init; } = string.Empty;
    public DateTime ConsentDate { get; init; }
}

public record CookieCategoryResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public bool IsActive { get; init; }
    public bool IsAccepted { get; init; }
    public int SortOrder { get; init; }
}

public record CookieConsentStatusResponse
{
    public bool HasConsent { get; init; }
    public DateTime? ConsentDate { get; init; }
    public List<CookieCategoryResponse> Categories { get; init; } = new();
    public bool NeedsUpdate { get; init; }
}
