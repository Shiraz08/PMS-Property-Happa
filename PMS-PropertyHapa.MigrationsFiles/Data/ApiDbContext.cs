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
        public DbSet<OTP> OTP { get; set; }
        public DbSet<AdditionalUserData> AdditionalUserData { get; set; }
        public DbSet<AssetsUnits> AssetsUnits { get; set; }
        public DbSet<TblAssignRole> TblAssignRoles { get; set; }
        public DbSet<TblRolePage> TblRolePages { get; set; }
        public DbSet<TenantOrganizationInfo> TenantOrganizationInfo { get; set; }
        public DbSet<PropertyType> PropertyType { get; set; }
        public DbSet<PropertySubType> PropertySubType { get; set; }
        public DbSet<OwnerOrganization> OwnerOrganization { get; set; }
        public DbSet<Pets> Pets { get; set; }

        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<CoTenant> CoTenant { get; set; }
        public DbSet<TenantDependent> TenantDependent { get; set; }
        public DbSet<Lease> Lease { get; set; }
        public DbSet<Communication> Communication { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<VideoTutorial> VideoTutorial { get; set; }
        public DbSet<RentCharge> RentCharge { get; set; }
        public DbSet<FeeCharge> FeeCharge { get; set; }
        public DbSet<SecurityDeposit> SecurityDeposit { get; set; }
        public DbSet<TaskRequest> TaskRequest { get; set; }
        public DbSet<TaskRequestHistory> TaskRequestHistory { get; set; }
        public DbSet<LineItem> LineItem { get; set; }
        public DbSet<VendorCategory> VendorCategory { get; set; }
        public DbSet<VendorClassification> VendorClassification { get; set; }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<VendorOrganization> VendorOrganization { get; set; }
        public DbSet<Applications> Applications { get; set; }
        public DbSet<ApplicationPets> ApplicationPets { get; set; }
        public DbSet<ApplicationVehicles> ApplicationVehicles { get; set; }
        public DbSet<ApplicationDependent> ApplicationDependent { get; set; }
        public DbSet<AccountSubType> AccountSubType { get; set; }
        public DbSet<AccountType> AccountType { get; set; }
        public DbSet<ChartAccount> ChartAccount { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Documents> Documents { get; set; }
        public DbSet<LateFee> LateFees { get; set; }
        public DbSet<LateFeeAsset> LateFeeAsset { get; set; }
        public DbSet<RecurringJob> RecurringJobs { get; set; }
        public DbSet<PaymentGuid> PaymentGuids { get; set; }
        public DbSet<PaymentInformation> PaymentInformations { get; set; }
        public DbSet<PaymentMethodInformation> PaymentMethodInformations { get; set; }
        public DbSet<StripeSubscription> StripeSubscriptions { get; set; }
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


            modelBuilder.Entity<Tenant>()
                .HasMany(c => c.TenantDependent)
                .WithOne(e => e.Tenant)
                .HasForeignKey(e => e.TenantId);



            modelBuilder.Entity<Tenant>()
                .HasMany(c => c.CoTenant)
                .WithOne(e => e.Tenant)
                .HasForeignKey(e => e.TenantId);


            modelBuilder.Entity<SecurityDeposit>(entity =>
            {
                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18, 4)"); 
            });

            modelBuilder.Entity<RentCharge>(entity =>
            {
                entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 4)");
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}
