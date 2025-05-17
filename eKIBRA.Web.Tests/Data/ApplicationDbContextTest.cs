using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using eKIBRA.Web.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;

namespace eKIBRA.Web.Tests.Data;

/// <summary>
/// DbContext class to map edge case that Sqlite differ from SqlServer
/// </summary>
public class ApplicationDbContextTest : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContextTest(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(e => e.IsDeleted);
    }

    /* Entities */
    public override DbSet<ApplicationUser> Users { get; set; }
}
