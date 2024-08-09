using Microsoft.AspNetCore.Identity;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.UserPermissions;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Services
{
    public class PermissionService : IPermissionService
    {
        private ApiDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public PermissionService(ApiDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public async Task<bool> HasAccess(string userId, int enumId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var roleIds = _roleManager.Roles.Where(r => roles.Contains(r.Name)).Select(r => r.Id);
            var hasAccess = (from up in _context.UserPermissions
                             where roleIds.Contains(up.RoleId)
                             join p in _context.Permissions on up.PermissionId equals p.Id
                             where p.EnumId == enumId
                             select new { up.RoleId, p.Id, p.Name, p.EnumId }).Any();

            return hasAccess;
        }

        //public bool HasAccess(string userId, int enumId)
        //{
        //    return (from up in _context.UserPermissions
        //            join netRoles in _roleManager on up.RoleId equals netRoles.Id
        //            from netUser in _userManager.Where(x => x.Id == userId && x.Role.Any(x => x.Id == netRoles.Id))
        //            from p in _context.Permissions.Where(x => x.Id == up.PermissionId)
        //            where netUser.Id == userId
        //            select new UserPermissionModel
        //            {
        //                RoleId = netRoles.Id,
        //                RoleName = netRoles.Name,
        //                PermissionId = p.Id,
        //                PermissionName = p.Name,
        //                EnumId = p.EnumId,
        //            }).Any(x => x.EnumId == enumId);
        //}
    }
    public enum UserPermissions
    {
        ViewLandlord = 1,
        AddLandlord = 2,
        ViewAssets = 3,
        AddAssets = 4,
        ViewTenant = 5,
        AddTenant = 6,
        ViewApplication = 7,
        AddApplication = 8,
        ViewLease = 9,
        AddLease = 10,
        ViewLeaseInvoices = 11,
        ViewMaintenance = 12,
        AddMaintenance = 13,
        ViewVendor = 14,
        AddVendor = 15,
        ViewVendorCategories = 16,
        AddVendorCategories = 17,
        ViewVendorClassification = 18,
        AddVendorClassification = 19,
        ViewAccounting = 20,
        ViewRent = 21,
        ViewAssestExpense = 22,
        ViewDeposit = 23,
        ViewBilling = 24,
        ViewInvoices = 25,
        ViewAccountType = 26,
        AddAccountType = 27,
        ViewAccountSubType = 28,
        AddAccountSubType = 29,
        ViewChartofAccounts = 30,
        AddChartofAccounts = 31,
        ViewBudget = 32,
        AddBudget = 33,
        ViewTask = 34,
        AddTask = 35,
        AddOwnerRequest = 36,
        AddTenantRequest = 37,
        AddWorkOrder = 38,
        ViewCommunication = 39,
        AddCommunication = 40,
        ViewCalendar = 41,
        ViewOccupancyOverview = 42,
        ViewReports = 43,
        ViewLeaseReports = 44,
        ViewFinanceReports = 45,
        ViewTenantReports = 46,
        ViewAssetReports = 47,
        ViewLandlordReports = 48,
        ViewDocuments = 49,
        AddDocuments = 50,
        PaymentIntegration = 51,
        QuickBookIntegration = 52,
        ViewSupportCenter = 53,
        ViewFAQ = 54,
        ContactUs = 55,
        BookAFreeDemo = 56,
        ViewVideoTutorial = 57,
        ViewSettings = 58,
        AddLateFee = 59,
        AddSubscription = 60
    }
}
