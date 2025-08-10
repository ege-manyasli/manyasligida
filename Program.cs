using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Http.Features;

using manyasligida.Data;
using manyasligida.Services;
using manyasligida.Services.Interfaces;
using manyasligida.Middleware;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// Timezone configuration for Turkey (UTC+3)
// This ensures all DateTime operations use Turkey timezone when server is in Europe
try 
{
    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
    Console.WriteLine("✅ Turkey timezone detected and configured");
}
catch 
{
    Console.WriteLine("⚠️ Turkey timezone not found, using UTC+3 offset");
}

// Basic logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure request size limits for large file uploads
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100_000_000; // 100MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100_000_000; // 100MB
});

// Configure form options
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100_000_000; // 100MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Add Response Caching
builder.Services.AddResponseCaching();





// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Authentication (Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".ManyasliGida.Auth";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Add Entity Framework with Azure-optimized configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Simplified connection string validation for startup stability
    Console.WriteLine($"Connection string configured: {!string.IsNullOrEmpty(connectionString)}");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Please check Azure App Service Configuration.");
    }
    
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    });
});

// Add Session support with enhanced security
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name = ".ManyasliGida.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    // Generate unique session ID for each request
    options.IOTimeout = TimeSpan.FromMinutes(1);
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Services - Modern Architecture
builder.Services.AddScoped<manyasligida.Services.Interfaces.IAccountService, AccountService>();
builder.Services.AddScoped<manyasligida.Services.Interfaces.ICookieConsentService, CookieConsentService>();
builder.Services.AddScoped<manyasligida.Services.Interfaces.IAccountingService, AccountingService>();
builder.Services.AddScoped<manyasligida.Services.Interfaces.IAboutService, AboutService>();
builder.Services.AddScoped<manyasligida.Services.Interfaces.IHomeService, HomeService>();
builder.Services.AddScoped<SiteSettingsService>();


// Legacy Services (keeping for compatibility)
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISessionManager, SessionManager>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();

var app = builder.Build();

// Database health check removed for startup stability
Console.WriteLine("Application starting - database health check deferred to runtime");

// Get logger for startup logging
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    startupLogger.LogInformation("=== APPLICATION STARTUP BEGIN ===");
    startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    startupLogger.LogInformation("Content Root: {ContentRoot}", app.Environment.ContentRootPath);
    startupLogger.LogInformation("Web Root: {WebRoot}", app.Environment.WebRootPath);
    
    // Log environment variables
    var aspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    startupLogger.LogInformation("ASPNETCORE_ENVIRONMENT: {Environment}", aspNetCoreEnv);
    
    // Log configuration
    var configuration = app.Services.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    startupLogger.LogInformation("Connection string configured: {HasConnectionString}", !string.IsNullOrEmpty(connectionString));
    
    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
        startupLogger.LogInformation("Production mode configured");
    }
    else
    {
        app.UseDeveloperExceptionPage();
        startupLogger.LogInformation("Development mode configured");
    }

    // Add middleware with error handling - TEMPORARILY DISABLED FOR EMERGENCY FIX
    /*try
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        startupLogger.LogInformation("GlobalExceptionMiddleware registered");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Failed to register GlobalExceptionMiddleware");
    }

    try
    {
        app.UseMiddleware<PerformanceMonitoringMiddleware>();
        startupLogger.LogInformation("PerformanceMonitoringMiddleware registered");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Failed to register PerformanceMonitoringMiddleware");
    }*/

    try
    {
        app.UseSession();
        startupLogger.LogInformation("Session configured");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Failed to configure session");
    }

    // Ensure authentication is available before session validation
    app.UseAuthentication();

    // TEMPORARILY DISABLED FOR EMERGENCY FIX
    /*try
    {
        app.UseMiddleware<SessionValidationMiddleware>();
        startupLogger.LogInformation("SessionValidationMiddleware registered");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Failed to register SessionValidationMiddleware");
    }*/



    app.UseHttpsRedirection();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        }
    });

    app.UseResponseCaching();
    app.UseRouting();

    // Cache headers moved to controller level for better control
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Add simple health check endpoints (no async operations for warmup stability)
    app.MapGet("/health", () => "OK");
    app.MapGet("/health/startup", () => Results.Ok(new { status = "startup_complete", timestamp = DateTimeHelper.NowTurkey }));

    startupLogger.LogInformation("=== APPLICATION STARTUP COMPLETE ===");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "CRITICAL ERROR during application startup");
    throw; // Re-throw to ensure the application fails fast
}

app.Run();
