using Microsoft.AspNetCore.Identity;

namespace eKIBRA.Web.Data;

public class ApplicationUser : IdentityUser
{
    public virtual required bool IsDeleted { get; set; }
}