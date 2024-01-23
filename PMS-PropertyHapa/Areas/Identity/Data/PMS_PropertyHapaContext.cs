using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.Areas.Identity.Data;

namespace PMS_PropertyHapa.Data;

public class PMS_PropertyHapaContext : IdentityDbContext<ApplicationUser>
{
    public PMS_PropertyHapaContext(DbContextOptions<PMS_PropertyHapaContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
