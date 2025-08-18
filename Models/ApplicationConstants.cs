namespace manyasligida.Models
{
    public static class ApplicationConstants
    {
        // Session Keys
        public static class SessionKeys
        {
            public const string UserId = "UserId";
            public const string UserName = "UserName";
            public const string UserEmail = "UserEmail";
            public const string IsAdmin = "IsAdmin";
        }

        // Order Status
        public static class OrderStatus
        {
            public const string Pending = "Pending";
            public const string Processing = "Processing";
            public const string Shipped = "Shipped";
            public const string Delivered = "Delivered";
            public const string Cancelled = "Cancelled";
        }

        // Payment Status
        public static class PaymentStatus
        {
            public const string Pending = "Pending";
            public const string Paid = "Paid";
            public const string Failed = "Failed";
            public const string Refunded = "Refunded";
        }

        // Controller Names
        public static class Controllers
        {
            public const string Home = "Home";
            public const string Account = "Account";
            public const string Admin = "Admin";
            public const string Products = "Products";
            public const string Cart = "Cart";
            public const string Order = "Order";
        }

        // Action Names
        public static class Actions
        {
            public const string Index = "Index";
            public const string Login = "Login";
            public const string Register = "Register";
            public const string Logout = "Logout";
            public const string Profile = "Profile";
            public const string Detail = "Detail";
        }

        // Messages
        public static class Messages
        {
            public const string LoginSuccess = "Başarıyla giriş yaptınız.";
            public const string LoginError = "Geçersiz e-posta veya şifre!";
            public const string LogoutSuccess = "Başarıyla çıkış yaptınız.";
            public const string RegisterSuccess = "Hesabınız başarıyla oluşturuldu.";
            public const string ProfileUpdateSuccess = "Profil bilgileriniz güncellendi.";
            public const string ProductAddSuccess = "Ürün başarıyla eklendi.";
            public const string EmptyCart = "Sepetiniz boş.";
            public const string UnauthorizedAccess = "Bu sayfaya erişim yetkiniz bulunmamaktadır.";
        }

        // File Upload
        public static class FileUpload
        {
            public const int MaxImageSizeMB = 5;
            public const long MaxImageSizeBytes = MaxImageSizeMB * 1024 * 1024;
            public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        }

        // Pagination
        public static class Pagination
        {
            public const int DefaultPageSize = 12;
            public const int MaxPageSize = 50;
        }

        // Rate Limiting
        public static class RateLimiting
        {
            public const int MaxRequestsPerMinute = 100;
            public const int MaxLoginAttemptsPerHour = 5;
            public const int MaxRegistrationAttemptsPerHour = 3;
        }

        // Security
        public static class Security
        {
            public const int PasswordMinLength = 6;
            public const int SessionTimeoutMinutes = 30;
            public const int AuthCookieExpiryDays = 7;
            public const int EmailVerificationExpiryMinutes = 15;
            public const int PasswordResetExpiryMinutes = 15;
        }
    }
}