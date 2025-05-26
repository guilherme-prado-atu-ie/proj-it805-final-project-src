using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Data;

internal static class ApplicationUserModelBuilder<T> where T : ApplicationUser
{
    internal static void SetModel(ModelBuilder builder)
    {
        builder.Entity<T>()
            .Property(e => e.IsDeleted);
    }
}