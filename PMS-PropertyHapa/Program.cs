using MagicVilla_Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json.Serialization;
using PMS_PropertyHapa.Extensions;
using PMS_PropertyHapa.Services.IServices;
using PMS_PropertyHapa.Services;
using PMS_PropertyHapa;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using Microsoft.AspNetCore.Identity.UI.Services;
using PMS_PropertyHapa.Shared.Email;
using Microsoft.Extensions.Options;
using PMS_PropertyHapa.Models.ViewModels;
using PMS_PropertyHapa.Models.Configrations;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews(u => u.Filters.Add(new AuthExceptionRedirection()));
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddSingleton<IApiMessageRequestBuilder, ApiMessageRequestBuilder>();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.Configure<GoogleCloudStorageOptions>(builder.Configuration.GetSection("GoogleServiceAccount"));
builder.Services.AddSingleton<GoogleCloudStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<PMS_PropertyHapa.API.Services.IStripeService, PMS_PropertyHapa.API.Services.StripeService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  options.Cookie.HttpOnly = true;
                  options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                  options.LoginPath = "/Auth/Login";
                  options.AccessDeniedPath = "/Auth/AccessDenied";
                  options.SlidingExpiration = true;
              });
builder.Services.ConfigureApplicationCookie(options =>
{
    //Location for your Custom Access Denied Page
    options.AccessDeniedPath = "/Auth/AccessDenied";

    //Location for your Custom Login Page
    options.LoginPath = "/Auth/Login";
});
// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.SignIn.RequireConfirmedEmail = true;
    opts.Lockout.AllowedForNewUsers = true;
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    opts.Lockout.MaxFailedAccessAttempts = 3;
});

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PropertyConnection")));



builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();
// Add other services
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
// Configure Ajax settings
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

builder.Configuration.GetSection("ServiceUrls");
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.AddControllersWithViews();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();