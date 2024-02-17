using Ecommerce.UI.Models.Claims;
using Microsoft.AspNetCore.Identity;
using PMS_PropertyHapa.Shared.PasswordHash;
using System.Security.Claims;

namespace PMS_PropertyHapa.Models.Roles   
{
    public static class StaticRoles
    {
        public static async Task GenericRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Roles
            await roleManager.CreateAsync(new IdentityRole("Tenant"));
        }
        public static List<Claim> AllClaims = new List<Claim>()
        {
            new Claim("canadduser","canadduser"),
            new Claim("canupdateuser","canupdateuser"),
            new Claim("canviewuser","canviewuser"),
            new Claim("superadminpermission","superadminpermission")
        };
        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Default User
            var defaultUser = new ApplicationUser
            {
                UserName = "superadmin",
                Email = "superadmin@gmail.com",
                FirstName = "Super",
                LastName = "Admin",
                PasswordHash = PasswordHashFormat.HashPassword("Pass@123"),
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                Status = true
                //Group = "SuperAdmin"
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                ClaimsViewModel claimsViewModel = new ClaimsViewModel();
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Pass@123");
                    await userManager.AddToRoleAsync(defaultUser, "SuperAdmin");
                    var id = roleManager.Roles.Where(x => x.Name == "SuperAdmin").Select(x => x.Id).FirstOrDefault();
                    ClaimsListViewModel cla = new ClaimsListViewModel();
                    List<ClaimsListViewModel> inputData = new List<ClaimsListViewModel>();
                    foreach (var parameter in AllClaims)
                    {
                        inputData.Add(new ClaimsListViewModel() { IsSelected = true, ClaimsType = parameter.Value });
                    }
                    claimsViewModel.RoleId = id;
                    claimsViewModel.ClaimsList = inputData;
                    var Role = await roleManager.FindByIdAsync(claimsViewModel.RoleId);
                    if (Role.Id != "" && Role.Id != null)
                    {
                        IList<Claim> cl = await roleManager.GetClaimsAsync(Role);
                        for (var i = 0; i < cl.Count; i++)
                        {
                            var result = await roleManager.RemoveClaimAsync(Role, cl[i]);
                        }
                        foreach (var item in claimsViewModel.ClaimsList.Where(c => c.IsSelected == true))
                        {
                            Claim c = new Claim(item.ClaimsType, item.ClaimsType);
                            var r = await roleManager.AddClaimAsync(Role, c);
                        }
                    }
                }
            }
        }
    }
}
