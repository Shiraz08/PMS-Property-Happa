﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models;

namespace PMS_PropertyHapa.Admin.Data
{
    public class PropertyHapaAdminContext : IdentityDbContext<ApplicationUser>
    {
        public PropertyHapaAdminContext(DbContextOptions<PropertyHapaAdminContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Tenant> Tenant { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


        }
    }
}
