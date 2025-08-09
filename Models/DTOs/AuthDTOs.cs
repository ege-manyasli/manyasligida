using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models.DTOs;

// Request DTOs
public record LoginRequest
{
    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Şifre gereklidir")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }
}

public record RegisterRequest
{
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olmalıdır")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olmalıdır")]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Telefon gereklidir")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    public string Phone { get; init; } = string.Empty;

    [Required(ErrorMessage = "Şifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre 6-100 karakter arasında olmalıdır")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor")]
    public string ConfirmPassword { get; init; } = string.Empty;

    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
}

public record ChangePasswordRequest
{
    [Required(ErrorMessage = "Mevcut şifre gereklidir")]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre 6-100 karakter arasında olmalıdır")]
    public string NewPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
    [Compare(nameof(NewPassword), ErrorMessage = "Yeni şifreler eşleşmiyor")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

public record UpdateProfileRequest
{
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olmalıdır")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olmalıdır")]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Telefon gereklidir")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    public string Phone { get; init; } = string.Empty;

    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
}

public record EmailVerificationRequest
{
    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Doğrulama kodu gereklidir")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Doğrulama kodu 6 karakter olmalıdır")]
    public string VerificationCode { get; init; } = string.Empty;
}

// Response DTOs
public record AuthResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public UserResponse? User { get; init; }
    public string? Token { get; init; }
}

public record UserResponse
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public bool IsActive { get; init; }
    public bool IsAdmin { get; init; }
    public bool EmailConfirmed { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = new();

    public static ApiResponse<T> SuccessResult(T data, string message = "İşlem başarılı")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };
}
