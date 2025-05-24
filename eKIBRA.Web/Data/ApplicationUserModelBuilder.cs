using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Data;

internal static class ApplicationUserModelBuilder
{
    internal static void SetModel(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>()
            .Property(e => e.IsDeleted);
    }
}