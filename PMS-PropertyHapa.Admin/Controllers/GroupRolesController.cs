using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.Admin.Data;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.Admin.Controllers
{
  //  [Authorize]
    public class GroupRolesController : Controller
    {
        private readonly PropertyHapaAdminContext _context;
        public readonly RoleManager<IdentityRole> _roleManager;

        public GroupRolesController(PropertyHapaAdminContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public ActionResult GetAllGroups()
        {
            return Json(_roleManager.Roles.Select(x => new
            {
                Name = x.Name,
                Id = x.Id
            }).ToList());
        }

        public async Task<JsonResult> GetRoles()
        {
            return Json(await _context.TblRolePages.Where(x => x.IsDeleted == false).ToListAsync());
        }
        [HttpPost]
        public async Task<JsonResult> Create(string RoleTitle, string RoleKey)
        {
            TblRolePage groupRole = new TblRolePage();
            groupRole.RoleTitle = RoleTitle;
            groupRole.RoleKey = RoleKey;
            groupRole.AddedBy = User.Identity?.Name;
            groupRole.AddedDate = DateTime.Now;
            groupRole.RoleActive = true;
            groupRole.IsDeleted = false;
            _context.Add(groupRole);
            await _context.SaveChangesAsync();
            return Json(true);
        }

        [HttpPost]
        public async Task<JsonResult> Edit(string Id, string RoleTitle, string RoleKey)
        {
            var info = _context.TblRolePages.Find(Convert.ToInt32(Id));
            info.RoleTitle = RoleTitle;
            info.RoleKey = RoleKey;
            _context.Update(info);
            await _context.SaveChangesAsync();
            return Json(true);
        }
        [HttpPost]
        public async Task<JsonResult> Delete(string id)
        {
            var info = _context.TblRolePages.Find(Convert.ToInt32(id));
            info.IsDeleted = true;
            _context.Update(info);
            await _context.SaveChangesAsync();
            return Json(true);
        }
        [HttpPost]
        public async Task<JsonResult> Reactivrrole(string id)
        {
            var info = _context.TblRolePages.Find(Convert.ToInt32(id));
            info.RoleActive = true;
            _context.Update(info);
            await _context.SaveChangesAsync();
            return Json(true);
        }
        [HttpPost]
        public async Task<JsonResult> InActivrrole(string id)
        {
            var info = _context.TblRolePages.Find(Convert.ToInt32(id));
            info.RoleActive = false;
            _context.Update(info);
            await _context.SaveChangesAsync();
            return Json(true);
        }
    }
}
