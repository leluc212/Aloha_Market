using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aloha.ServiceDefaults.Hosting
{
    public static class DatabaseMigrationExtensions
    {
        /// <summary>
        /// Khoi tao schema DB luc startup: neu context co migration thi Migrate(),
        /// khong thi EnsureCreated(). Bao ve bang try/catch de loi DB khong lam sap container
        /// (gateway/health van song).
        /// </summary>
        public static WebApplication MigrateDatabase<TContext>(this WebApplication app)
            where TContext : DbContext
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit");
            try
            {
                using var scope = app.Services.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<TContext>();

                if (ctx.Database.GetMigrations().Any())
                {
                    ctx.Database.Migrate();
                    logger.LogInformation("Applied migrations for {Context}", typeof(TContext).Name);
                }
                else
                {
                    ctx.Database.EnsureCreated();
                    logger.LogInformation("EnsureCreated schema for {Context}", typeof(TContext).Name);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database init failed for {Context} - service van chay tiep", typeof(TContext).Name);
            }

            return app;
        }
    }
}
