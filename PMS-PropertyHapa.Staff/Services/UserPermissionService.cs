using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.UserPermissions;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private ApiDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserPermissionService(ApiDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public List<PermissionCategory> GetPermissionCategories()
        {
            return _context.PermissionCategories.ToList();
        }

        public async Task<List<PermissionModel>> GetUsersPermissionsByRoles(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            var userPermissions = _context.UserPermissions.Where(x => x.RoleId == role.Id).Select(x => x.PermissionId).ToList();

            var result = from p in _context.Permissions
                         join pc in _context.PermissionCategories on p.PermissionCategoryId equals pc.Id into pcg
                         from pc in pcg.DefaultIfEmpty()
                         select new PermissionModel
                         {
                             Id = p.Id,
                             EnumId = p.EnumId,
                             Name = p.Name,
                             PermissionCategoryId = p.PermissionCategoryId,
                             PermissionCategory = pc != null ? pc.Name : null,
                             RoleId = role.Id,
                             IsChecked = userPermissions.Contains(p.Id)
                         };

            return result.ToList();
        }

        public async Task ChangeUserPermissionCheckboxStatus(UserPermissionModel model)
        {
            var userPermission = await _context.UserPermissions.FirstOrDefaultAsync(x => x.PermissionId == model.PermissionId && x.RoleId == model.RoleId);

            if (model.IsChecked)
            {
                if (userPermission == null)
                {
                    userPermission = new UserPermission
                    {
                        PermissionId = model.PermissionId,
                        RoleId = model.RoleId
                    };
                    _context.UserPermissions.Add(userPermission);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                if (userPermission != null)
                {
                    _context.UserPermissions.Remove(userPermission);
                    await _context.SaveChangesAsync();
                }
            }
        }

    }
}
