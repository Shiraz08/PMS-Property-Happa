using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PMS_PropertyHapa.MigrationsFiles.Data
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
        public DbSet<Owner> Owner { get; set; }
        public DbSet<Assets> Assets { get; set; }
        public DbSet<AssetsUnits> AssetsUnits { get; set; }
        public DbSet<TblAssignRole> TblAssignRoles { get; set; }
        public DbSet<TblRolePage> TblRolePages { get; set; }
        public DbSet<TenantOrganizationInfo> TenantOrganizationInfo { get; set; }
        public DbSet<PropertyType> PropertyType { get; set; }
        public DbSet<PropertySubType> PropertySubType { get; set; }
        public DbSet<OwnerOrganization> OwnerOrganization { get; set; }
        public DbSet<Pets> Pets { get; set; }

        public DbSet<Vehicle> Vehicle { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>()
                .HasMany(c => c.Pets) // Ensure your Tenant entity has a collection property named Pets
                .WithOne(e => e.Tenant) // Ensure your Pet entity has a navigation property named Tenant
                .HasForeignKey(e => e.TenantId); // This line assumes your Pet entity has a foreign key property named TenantId

            modelBuilder.Entity<Tenant>()
                .HasMany(c => c.Vehicle)
                .WithOne(e => e.Tenant) 
                .HasForeignKey(e => e.TenantId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
