using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.API.Areas.Identity.Data
{
    public class ApiDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
    : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<TblAssignRole> TblAssignRoles { get; set; }
        public DbSet<TblRolePage> TblRolePages { get; set; }
        public DbSet<TenantOrganizationInfo> TenantOrganizationInfo { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
