# Azure App Service Troubleshooting Guide

## HTTP Error 500.30 - ASP.NET Core App Failed to Start

### Common Causes and Solutions

#### 1. Database Connection Issues
**Problem**: Application cannot connect to Azure SQL Database
**Symptoms**: 
- Database connection timeouts
- Login failures
- Retry limit exceeded errors

**Solutions**:
- Verify connection string in Azure App Service Configuration
- Check if Azure SQL Database is running and accessible
- Ensure firewall rules allow Azure App Service IP
- Verify database credentials are correct

**Connection String Format**:
```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=YourDatabase;Persist Security Info=False;User ID=YourUsername;Password=YourPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;
```

#### 2. Missing Environment Variables
**Problem**: Application cannot find required configuration
**Solutions**:
- Set `ASPNETCORE_ENVIRONMENT=Production` in Azure App Service Configuration
- Ensure all connection strings are properly configured
- Check if `ASPNETCORE_URLS` is set correctly

#### 3. Missing Dependencies
**Problem**: Application cannot find required assemblies
**Solutions**:
- Ensure all NuGet packages are properly referenced
- Check if any native dependencies are missing
- Verify .NET version compatibility

#### 4. File System Permissions
**Problem**: Application cannot write to file system
**Solutions**:
- Use Azure App Service's temporary storage for file operations
- Ensure upload directories exist and are writable
- Check file system permissions

### Diagnostic Steps

#### 1. Enable Detailed Logging
Add these settings to your `appsettings.Production.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

#### 2. Check Application Logs
- Go to Azure Portal → Your App Service → Logs → Log stream
- Check for specific error messages
- Look for startup failures

#### 3. Test Database Connection
Use the health check endpoints:
- `/health` - Basic health check
- `/health/detailed` - Database connection test
- `/health/startup` - Startup completion check

#### 4. Enable Application Insights
Add Application Insights to get detailed telemetry:
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Quick Fixes

#### 1. Restart the Application
- Go to Azure Portal → Your App Service → Overview → Restart

#### 2. Check Configuration
- Verify all connection strings in Azure App Service Configuration
- Ensure environment variables are set correctly

#### 3. Check Database Status
- Verify Azure SQL Database is running
- Check firewall rules and network security groups

#### 4. Review Recent Changes
- Check if recent deployments introduced breaking changes
- Review application logs for specific error messages

### Prevention

#### 1. Use Health Checks
The application includes health check endpoints:
- `/health` - Basic health check
- `/health/detailed` - Database connection test

#### 2. Implement Proper Error Handling
- Use try-catch blocks around critical operations
- Log all exceptions with sufficient detail
- Implement graceful degradation

#### 3. Test in Staging Environment
- Deploy to staging slot first
- Test all functionality before swapping to production

#### 4. Monitor Application Performance
- Use Azure Application Insights
- Set up alerts for critical errors
- Monitor database performance

### Emergency Recovery

If the application is completely down:

1. **Immediate Actions**:
   - Restart the App Service
   - Check Azure SQL Database status
   - Verify connection strings

2. **Rollback Options**:
   - Deploy previous working version
   - Use deployment slots for safe rollbacks

3. **Contact Support**:
   - If issues persist, contact Azure support
   - Provide detailed logs and error messages

### Common Configuration Issues

#### 1. Connection String Issues
- Missing or incorrect connection string
- Wrong database name or server
- Incorrect credentials

#### 2. Environment Configuration
- Wrong ASPNETCORE_ENVIRONMENT setting
- Missing required environment variables
- Incorrect appsettings file

#### 3. Middleware Issues
- Custom middleware throwing exceptions
- Missing required services
- Dependency injection failures

### Monitoring and Alerts

Set up monitoring for:
- Application availability
- Database connection status
- Response times
- Error rates
- Resource usage

### Support Resources

- [Azure App Service Troubleshooting](https://docs.microsoft.com/en-us/azure/app-service/troubleshoot-diagnostic-logs)
- [ASP.NET Core on Azure](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/)
- [Azure SQL Database Troubleshooting](https://docs.microsoft.com/en-us/azure/azure-sql/database/troubleshoot-connectivity-issues-microsoft-azure-sql-database)
