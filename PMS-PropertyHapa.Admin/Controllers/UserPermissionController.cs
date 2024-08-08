using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Admin.Filters;
using PMS_PropertyHapa.Admin.Services;
using PMS_PropertyHapa.Models.UserPermissions;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class UserPermissionController : Controller
    {
        private readonly IUserPermissionService _userPermissionService;


        public UserPermissionController(IUserPermissionService userPermissionService)
        {
            _userPermissionService = userPermissionService;


        }
        [TypeFilter(typeof(PermissionFilter), Arguments = new object[] { UserPermissions.AdminMenuAccessvisable })]
        public IActionResult Index()
        {
            return View();

        }


        public IActionResult GetUsersPermissionsByRoles(string roleId)
        {
            var permissionCategories = _userPermissionService.GetPermissionCategories();
            var userPermission = _userPermissionService.GetUsersPermissionsByRoles(roleId);

            var result = new
            {
                UserPermissions = userPermission,
                PermissionCategories = permissionCategories,
            };
            return Ok(result);
        }

        #region Ajax 

        [HttpPost]
        [TypeFilter(typeof(PermissionFilter), Arguments = new object[] { UserPermissions.AdminMenuAccessvisable })]
        public async Task<IActionResult> ChangeUserPermissionCheckboxStatus(UserPermissionModel model)
        {
            await _userPermissionService.ChangeUserPermissionCheckboxStatus(model);
            return Ok(true);
        }

        #endregion
    }
}
