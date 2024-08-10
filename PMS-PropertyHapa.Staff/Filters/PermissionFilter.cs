using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Staff.Services;
using System.Security.Claims;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Filters
{
    public class PermissionFilter : IAuthorizationFilter
    {
        private readonly UserPermissions _requiredPermission;
        private readonly IPermissionService _permissionService;

        public PermissionFilter(UserPermissions requiredPermission, IPermissionService permissionService)
        {
            _requiredPermission = requiredPermission;
            _permissionService = permissionService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool hasAccess = _permissionService.HasAccess(userId, (int)_requiredPermission).GetAwaiter().GetResult();
            if (!hasAccess)
            {
                context.Result = new OkObjectResult(
                    new
                    {
                        IsValid = false,
                        Message = "You don't have access to do this"
                    });
            }
        }
    }
}

