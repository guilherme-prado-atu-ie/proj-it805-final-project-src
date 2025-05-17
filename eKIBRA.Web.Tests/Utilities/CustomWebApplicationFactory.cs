
using System.Data.Common;
using eKIBRA.Web.Data;
using eKIBRA.Web.Tests.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace eKIBRA.Web.Tests.Utilities
{
    public class CustomWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

                if (dbContextDescriptor is not null)
                {
                    services.Remove(dbContextDescriptor);
                }

                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbConnection));

                if (dbConnectionDescriptor is not null)
                {
                    services.Remove(dbConnectionDescriptor);
                }

                // Create as Singleton and open a SqliteConnection so EF won't automatically close it.
                services.AddSingleton<DbConnection>(container =>
                {

                    /* 
                    var connectionBuilder = new SqliteConnectionStringBuilder()
                    {
                        //DataSource = dataSource,
                        Mode = SqliteOpenMode.Memory,
                        Cache = SqliteCacheMode.Shared,
                        ForeignKeys = false,
                        RecursiveTriggers = true,
                        Password = string.Empty,
                        Pooling = true
                    };

                    // Create a new service provider to create a new SQLite database.
                    var serviceProvider = new ServiceCollection()
                    .AddDbContext<ApplicationDbContextTest>(options =>
                    {
                        options.UseSqlite(connectionBuilder.ToString());
                        options.EnableSensitiveDataLogging();
                    })
                    .BuildServiceProvider();

                    // Create a new options instance using an SQLite database and 
                    // IServiceProvider that the context should resolve all of its 
                    // services from.
                    var builder = new DbContextOptionsBuilder<ApplicationDbContextTest>()
                        .UseSqlite()
                        .UseInternalServiceProvider(serviceProvider);
                    */

                    var connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();

                    return connection;
                });

                services.AddDbContext<ApplicationDbContextTest>((container, options) =>
                {
                    var connection = container.GetRequiredService<DbConnection>();
                    options.UseSqlite(connection);
                });
            });

            builder.UseEnvironment("Development");
        }
    }
}