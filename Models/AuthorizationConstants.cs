namespace manyasligida.Models
{
    public static class AuthorizationConstants
    {
        // Roles
        public const string AdminRole = "Admin";
        public const string UserRole = "User";
        
        // Policies
        public const string AdminPolicy = "AdminPolicy";
        public const string AuthenticatedUserPolicy = "AuthenticatedUserPolicy";
        
        // Claims
        public const string UserIdClaim = "UserId";
        public const string EmailClaim = "Email";
        public const string FullNameClaim = "FullName";
        public const string IsAdminClaim = "IsAdmin";
    }
}