namespace PMS_PropertyHapa.Admin.Services
{
    public interface IPermissionService
    {
        Task<bool> HasAccess(string userId, int enumId);
    }
}
