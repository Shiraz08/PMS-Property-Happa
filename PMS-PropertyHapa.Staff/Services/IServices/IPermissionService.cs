namespace PMS_PropertyHapa.Staff.Services.IServices
{
    public interface IPermissionService
    {
        Task<bool> HasAccess(string userId, int enumId);
    }
}
