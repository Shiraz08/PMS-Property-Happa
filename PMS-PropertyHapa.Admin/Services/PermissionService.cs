using Microsoft.AspNetCore.Identity;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.UserPermissions;

namespace PMS_PropertyHapa.Admin.Services
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
        CRMProfileView = 1,
        CRMProfileEdit = 2,
        AnalyticsMenuAccessvisable = 3,
        TimeTrackerMenuAccessvisable = 4,
        OpsMenuAccessvisable = 5,
        AdminMenuAccessvisable = 6,
        ViewSE = 7,
        AddCustomerInfo = 8,
        AddModels = 9,
        AdjustAccessoriesandSpecswithNotes = 10,
        UploadPicturesPictureNotes = 11,
        Addinventoryitemstoorder = 12,
        Addrepaircodestoorder = 13,
        Addcapcodestoorder = 14,
        RepairCodePendingCodeview = 15,
        AddOutsourceCost = 16,
        CreateShippingLabels = 17,
        ViewIR = 18,
        AddCustomerInfoIR = 19,
        AddModelsIR = 20,
        UploadPicturesPictureNotesIR = 21,
        AddinventoryitemstoorderIR = 22,
        AddrepaircodestoorderIR = 23,
        AddcapcodestoorderIR = 24,
        RepairCodePendingCodeviewIR = 25,
        AddOutsourceCostIR = 26,
        CreateShippingLabelsIR = 27,
        ViewOnsite = 28,
        AddCustomerInfoOnsite = 29,
        AddTrays = 30,
        CreateOffsiteOrders = 31,
        CreateOnsiteOrders = 32,
        UploadPicturesPictureNotesOnsite = 33,
        AddrepaircodestoorderOnsite = 34,
        Addcapcodestoorderdailyrates = 35,
        PricingVisibile = 36,
        CreateShippingLabelsOnsite = 37,
        FinanceAccessVisible = 38,
        SeOrderAccessVisible = 39,
        IrOrderAccessVisible = 40,
        CpoOrderAccessVisible = 41,
        OnSiteOrderAccessVisible = 42,
        DashboardAccessVisible = 43,
        InventoryAccessVisible = 44,
        VendorManagementAccessVisible = 45,
        ShippingDefaultsAccessVisible = 46,
        SEDocumentView = 47,
        SEShippingLabelView = 48,
        IRShippingLabelView = 49,
        OnSiteShippingLabelView = 50,
        SEQCDocumentView = 51,
        PriceListMenuAccessvisible = 52,
        DeleteHeaderItems = 53,
        SEManualStatusChange = 54,
        IRManualStatusChange = 55,
        LoanerManagementAccessVisible = 56,
    }
}
