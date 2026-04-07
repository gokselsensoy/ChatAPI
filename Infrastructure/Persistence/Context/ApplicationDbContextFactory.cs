using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Context
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var webApiPath = FindWebApiDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(webApiPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite());

            return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeNullPublisher());
        }

        private static string FindWebApiDirectory()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "WebApi", "appsettings.json");
                if (File.Exists(candidate))
                    return Path.Combine(dir.FullName, "WebApi");
                dir = dir.Parent;
            }

            throw new InvalidOperationException(
                "WebApi/appsettings.json could not be located. Run dotnet ef from the solution directory.");
        }

        private sealed class DesignTimeNullPublisher : IPublisher
        {
            public Task Publish(object notification, CancellationToken cancellationToken = default) =>
                Task.CompletedTask;

            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification =>
                Task.CompletedTask;
        }
    }
}
