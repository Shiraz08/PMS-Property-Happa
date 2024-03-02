using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;

namespace PMS_PropertyHapa.Admin.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private RoleManager<IdentityRole> roleManager;
        private UserManager<ApplicationUser> userManager;
        private UserManager<ApplicationUser> _UserManager;
        private SignInManager<ApplicationUser> _SignInManager;
        private ApiDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GroupsController(RoleManager<IdentityRole> roleMgr, UserManager<ApplicationUser> userMrg, UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signinMgr, IWebHostEnvironment webHostEnvironment, ApiDbContext context, IEmailSender emailSender)
        {
            roleManager = roleMgr;
            userManager = userMrg;
            _UserManager = userMgr;
            _SignInManager = signinMgr;
            _context = context;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        public JsonResult Getroles()
        {
            var list = roleManager.Roles.ToList();
            return Json(list);
        }
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<JsonResult> Create(string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                    return Json(true);
                else
                    Json(false);
            }
            return Json(false);
        }

        public async Task<IActionResult> Update(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            List<ApplicationUser> members = new List<ApplicationUser>();
            List<ApplicationUser> nonMembers = new List<ApplicationUser>();
            foreach (ApplicationUser user in userManager.Users)
            {
                var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            return View(new RoleEdit
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(RoleModification model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.AddIds ?? new string[] { })
                {
                    ApplicationUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                foreach (string userId in model.DeleteIds ?? new string[] { })
                {
                    ApplicationUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            else
                return await Update(model.RoleId);
        }

        [HttpPost]
        public async Task<JsonResult> Delete(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return Json(true);
                else
                    return Json(false);
            }
            else
                return Json(false);
        }
        [HttpPost]
        public async Task<JsonResult> EditRole(string id, string EditRolename)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                role.Name = EditRolename;
                role.NormalizedName = EditRolename.ToUpper();
                IdentityResult result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                    return Json(true);
                else
                    return Json(false);
            }
            else
                return Json(false);
        }
        public JsonResult GetRolePages()
        {
            var roles_pages = _context.TblRolePages.Where(x => x.RoleActive == true && x.IsDeleted == false).ToList();
            return Json(roles_pages);
        }
        public JsonResult GetSelectedRolePages(string Id)
        {
            var roles_pages = _context.TblAssignRoles.Where(x => x.GroupId == Id).Select(x => x.RolePageId).FirstOrDefault();
            return Json(roles_pages);
        }
        [HttpPost]
        public JsonResult SaveRolesPages(string groupid, string groupname, List<int> selectedid, List<string> selectedname)
        {
            var oldinfo = _context.TblAssignRoles.Where(x => x.GroupId == groupid).FirstOrDefault();
            if (oldinfo != null)
            {
                _context.Remove(oldinfo);
                _context.SaveChanges();
            }

            TblAssignRole assignRole = null;
            assignRole = new TblAssignRole();
            assignRole.RoleActive = true;
            assignRole.AddedDate = DateTime.Now;
            assignRole.AddedBy = User.Identity?.Name;
            assignRole.GroupId = groupid;
            assignRole.GroupName = groupname;
            assignRole.RolePageId = String.Join(",", selectedid);
            assignRole.RoleTitle = String.Join(",", selectedname);
            _context.Add(assignRole);
            _context.SaveChanges();
            return Json(true);
        }
    }
}
