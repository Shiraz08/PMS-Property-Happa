﻿using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.UserPermissions;

namespace PMS_PropertyHapa.Staff.Services.IServices
{
    public interface IUserPermissionService
    {
        List<PermissionCategory> GetPermissionCategories();
        Task<List<PermissionModel>> GetUsersPermissionsByRoles(string roleId);
        Task ChangeUserPermissionCheckboxStatus(UserPermissionModel model);
    }
}
