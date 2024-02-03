using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Serialization;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Admin.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch.Internal;
using PMS_PropertyHapa.Shared.Dapper;
using AutoMapper;
using PMS_PropertyHapa.Shared.MappingProfiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultSQLConnection") ?? throw new InvalidOperationException("Connection string 'DefaultSQLConnection' not found.");
builder.Services.AddDbContext<PropertyHapaAdminContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<PropertyHapaAdminContext>();
builder.Services.AddDbContext<PropertyHapaAdminContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// Add other services
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
// Configure Ajax settings
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});
// Session
builder.Services.AddMvc();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(60);
});
//email sender override class
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
   opt.TokenLifespan = TimeSpan.FromHours(2));
builder.Services.AddTransient<IEmailSender, EmailSender>();
//Register dapper in scope    
builder.Services.AddScoped<IDapper, DapperServices>();
// Auto Mapper Configurations
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Lockout.AllowedForNewUsers = true;
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    opts.Lockout.MaxFailedAccessAttempts = 3;
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapRazorPages();
    endpoints.MapControllers();
});
app.Run();


app.UseExceptionHandler("/Home/Error");
app.UseHsts();