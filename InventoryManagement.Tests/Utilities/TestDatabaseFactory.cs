using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using InventoryManagement.Infrastructure.Persistence;

namespace InventoryManagement.Tests.Utilities
{
    public class TestDatabaseFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove ALL DbContext related services
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<InventoryDbContext>));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions));
                if (dbContextOptionsDescriptor != null)
                    services.Remove(dbContextOptionsDescriptor);

                // Remove DbContextOptions<T> for any types that are not the main one
                var otherDbContextOptions = services.Where(d =>
                    d.ServiceType.IsGenericType &&
                    d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)).ToList();

                foreach (var descriptor in otherDbContextOptions)
                    services.Remove(descriptor);

                // Remove any other database provider registrations
                var databaseProviders = services.Where(d =>
                    d.ServiceType.Namespace != null &&
                    d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore")).ToList();

                foreach (var descriptor in databaseProviders)
                    services.Remove(descriptor);

                // Add new DbContext using InMemoryDatabase
                services.AddDbContext<InventoryDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Ensure database is created
                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
