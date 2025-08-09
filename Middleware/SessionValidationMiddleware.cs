using manyasligida.Services;
using Microsoft.AspNetCore.Authentication;

namespace manyasligida.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ISessionManager sessionManager)
        {
            try
            {
                // Skip session validation for static files and certain paths
                if (ShouldSkipSessionValidation(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                // Check both claims-based auth and session consistency
                var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
                var hasSession = !string.IsNullOrEmpty(context.Session.GetString("UserId"));

                if (isAuthenticated && !hasSession)
                {
                    // User has auth cookie but no session - recreate session
                    _logger.LogInformation("Auth cookie exists but no session - attempting to recreate session");
                    var isValid = await sessionManager.ValidateSessionAsync();
                    if (!isValid)
                    {
                        _logger.LogWarning("Failed to recreate session from auth cookie - forcing logout");
                        await context.SignOutAsync();
                    }
                }
                else if (!isAuthenticated && hasSession)
                {
                    // Session exists but no auth cookie - clear session
                    _logger.LogInformation("Session exists but no auth cookie - clearing session");
                    await sessionManager.InvalidateSessionAsync();
                }
                else if (isAuthenticated && hasSession)
                {
                    // Both exist - validate session normally
                    var isValid = await sessionManager.ValidateSessionAsync();
                    if (!isValid)
                    {
                        _logger.LogInformation("Session validation failed - clearing both auth and session");
                        await sessionManager.InvalidateSessionAsync();
                        await context.SignOutAsync();
                    }
                }
                else
                {
                    // Extend session if user is logged in
                    var isLoggedIn = await sessionManager.IsUserLoggedInAsync();
                    if (isLoggedIn)
                    {
                        await sessionManager.ExtendSessionAsync();
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SessionValidationMiddleware");
                await _next(context);
            }
        }

        private bool ShouldSkipSessionValidation(PathString path)
        {
            var skipPaths = new[]
            {
                "/css/",
                "/js/",
                "/lib/",
                "/img/",
                "/uploads/",
                "/favicon.ico",
                "/health",
                "/health/startup",
                "/account/login",
                "/account/logout",
                "/account/register",
                "/account/verifyemail",
                "/account/resendverificationcode"
            };

            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
        }
    }
}
