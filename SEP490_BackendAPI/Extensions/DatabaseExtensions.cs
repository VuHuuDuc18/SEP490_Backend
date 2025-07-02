using Microsoft.EntityFrameworkCore;
using Infrastructure.DBContext;
using Infrastructure.Identity.Contexts;

namespace SEP490_BackendAPI.Extensions
{
    public static class DatabaseExtensions
    {
        public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                // Retry logic for database connection
                await RetryPolicy(async () =>
                {
                    logger.LogInformation("🚀 Attempting to connect to database...");
                    var identityContext = services.GetRequiredService<IdentityContext>();
                    await identityContext.Database.CanConnectAsync();
                    
                    logger.LogInformation("✅ Database connection successful!");
                }, logger,5);

                // Apply migrations
                await RetryPolicy(async () =>
                {
                    logger.LogInformation("🚀 Applying database migrations...");
                    
                    var identityContext = services.GetRequiredService<IdentityContext>();
                    await identityContext.Database.MigrateAsync();
                    
                    logger.LogInformation("✅ Database migrations completed!");
                }, logger,5);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Database migration failed");
                throw;
            }

            return app;
        }

        private static async Task RetryPolicy(Func<Task> operation, ILogger logger, int maxRetries = 10)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                        throw;

                    logger.LogWarning($"⚠️  Attempt {i + 1} failed: {ex.Message}. Retrying in 5 seconds...");
                    await Task.Delay(5000);
                }
            }
        }
    }
} 