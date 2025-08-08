using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using manyasligida.Data;
using manyasligida.Services;
using manyasligida.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Configure logging for Azure
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
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

// Add Memory Cache for better performance
builder.Services.AddMemoryCache();

// Add Entity Framework with better configuration for Azure
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60); // Increased timeout for Azure
        }));

// Add Session support
builder.Services.AddDistributedMemoryCache();
// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name = ".ManyasliGida.Session";
    options.Cookie.SameSite = SameSiteMode.Lax; // Back to Lax for better compatibility
    options.Cookie.HttpOnly = true; // Back to true for security
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add CartService
builder.Services.AddScoped<CartService>();

// Add FileUploadService
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Add AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Add EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// Add SiteSettingsService
builder.Services.AddSingleton<ISiteSettingsService, SiteSettingsService>();

// Add PerformanceMonitorService
builder.Services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();

// Add CookieConsentService
builder.Services.AddScoped<ICookieConsentService, CookieConsentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add Global Exception Middleware early in the pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add Performance Monitoring Middleware
app.UseMiddleware<PerformanceMonitoringMiddleware>();

// Add Response Compression
app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 year
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});

// Add Response Caching
app.UseResponseCaching();

app.UseRouting();

// Add cache clearing middleware for logout
app.Use(async (context, next) =>
{
    // Check if this is a logout request
    if (context.Request.Path.StartsWithSegments("/Account/Logout"))
    {
        // Clear all cache headers
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }
    
    await next();
});

// Configure session middleware with security
app.UseSession(new SessionOptions
{
    IdleTimeout = TimeSpan.FromMinutes(30),
    Cookie = new CookieBuilder
    {
        Name = ".ManyasliGida.Session",
        HttpOnly = true, // Back to true for security
        IsEssential = true,
        SecurePolicy = CookieSecurePolicy.SameAsRequest,
        SameSite = SameSiteMode.Lax, // Back to Lax for better compatibility
        MaxAge = TimeSpan.FromMinutes(30)
    }
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add health check endpoint for Azure
app.MapGet("/health", () => "OK");

// Add detailed health check
app.MapGet("/health/detailed", async (ApplicationDbContext dbContext) =>
{
    try
    {
        await dbContext.Database.CanConnectAsync();
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

app.Run();
