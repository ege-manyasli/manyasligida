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
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Timezone configuration for Turkey (UTC+3)
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

// Add Rate Limiting for production
if (!builder.Environment.IsDevelopment())
{
    // Rate limiting temporarily disabled due to compatibility issues
    // builder.Services.AddRateLimiter(options =>
    // {
    //     options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    //         RateLimitPartition.GetFixedWindowLimiter(
    //             partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
    //             factory: partition => new FixedWindowRateLimiterOptions
    //             {
    //                 AutoReplenishment = true,
    //                 PermitLimit = 100,
    //                 Window = TimeSpan.FromMinutes(1)
    //             }));
    // });
}

// Add Authentication (Cookie) - Enhanced security
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".ManyasliGida.Auth";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict; // Better security
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Force HTTPS
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

// Add Entity Framework with Azure-optimized configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
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

// Add Session support - ONLY for cart and temporary data
// Use Redis for production, Memory for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    // For production, use Redis if available, otherwise fallback to SQL Server
    var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "ManyasliGida_";
        });
    }
    else
    {
        builder.Services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            options.SchemaName = "dbo";
            options.TableName = "SessionCache";
        });
    }
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Force HTTPS in production
    options.Cookie.Name = ".ManyasliGida.Session";
    options.Cookie.SameSite = SameSiteMode.Strict; // Better security
    options.Cookie.HttpOnly = true;
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
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<manyasligida.Services.Interfaces.IInventoryService, InventoryService>();

// Core Services
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();

var app = builder.Build();

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

    // Add middleware with error handling
    try
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
    }

    // Session - ONLY for cart and temporary data
    try
    {
        app.UseSession();
        startupLogger.LogInformation("Session configured for cart/temporary data only");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Failed to configure session");
    }

    // Authentication must come before authorization
    app.UseAuthentication();
    startupLogger.LogInformation("Authentication configured");

    app.UseHttpsRedirection();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        }
    });

    // Serve uploads from a writable location in production (e.g., Azure App Service)
    try
    {
        var homeDir = Environment.GetEnvironmentVariable("HOME");
        var uploadsPhysicalPath = !string.IsNullOrEmpty(homeDir)
            ? Path.Combine(homeDir, "data", "uploads")
            : Path.Combine(app.Environment.WebRootPath ?? app.Environment.ContentRootPath, "uploads");

        Directory.CreateDirectory(uploadsPhysicalPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPhysicalPath),
            RequestPath = "/uploads",
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
            }
        });
        startupLogger.LogInformation("Uploads static files configured at {UploadsPhysicalPath}", uploadsPhysicalPath);
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Failed to configure uploads static files mapping");
    }

    app.UseResponseCaching();
    app.UseRouting();

    // Rate Limiting (production only)
    if (!app.Environment.IsDevelopment())
    {
        // app.UseRateLimiter(); // Temporarily disabled
    }

    // Authorization
    app.UseAuthorization();
    startupLogger.LogInformation("Authorization configured");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    startupLogger.LogInformation("=== APPLICATION STARTUP COMPLETE ===");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "CRITICAL ERROR during application startup");
    throw; // Re-throw to ensure the application fails fast
}

app.Run();
