using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using manyasligida.Data;
using manyasligida.Services;
using manyasligida.Middleware;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// Basic logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container
builder.Services.AddControllersWithViews();

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Entity Framework with Azure-optimized configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Log connection string info (without sensitive data)
    var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();
    if (logger != null)
    {
        logger.LogInformation("Connection string found: {HasConnectionString}", !string.IsNullOrEmpty(connectionString));
        if (!string.IsNullOrEmpty(connectionString))
        {
            var serverInfo = connectionString.Contains("Server=") ? 
                connectionString.Substring(connectionString.IndexOf("Server=") + 7, 
                    Math.Min(50, connectionString.IndexOf(";", connectionString.IndexOf("Server=") + 7) - connectionString.IndexOf("Server=") - 7)) : "Not found";
            logger.LogInformation("Database server: {Server}", serverInfo);
        }
    }
    
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

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name = ".ManyasliGida.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Services
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();
builder.Services.AddScoped<ICookieConsentService, CookieConsentService>();

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

    // Add Response Compression
    app.UseResponseCompression();
    startupLogger.LogInformation("Response compression configured");

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

    // Add cache clearing middleware for logout
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/Account/Logout"))
        {
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
        }
        await next();
    });

    app.UseSession();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Add health check endpoints
    app.MapGet("/health", () => "OK");
    app.MapGet("/health/startup", () => Results.Ok(new { status = "startup_complete", timestamp = DateTime.UtcNow }));
    
    app.MapGet("/health/detailed", async (ApplicationDbContext dbContext, ILogger<Program> logger) =>
    {
        try
        {
            logger.LogInformation("Testing database connection...");
            await dbContext.Database.CanConnectAsync();
            logger.LogInformation("Database connection successful");
            return Results.Ok(new { status = "healthy", database = "connected" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database connection failed");
            return Results.Problem($"Database connection failed: {ex.Message}");
        }
    });

    startupLogger.LogInformation("=== APPLICATION STARTUP COMPLETE ===");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "CRITICAL ERROR during application startup");
    throw; // Re-throw to ensure the application fails fast
}

app.Run();
