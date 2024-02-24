using PMS_PropertyHapa.Tenant.Models;

namespace PMS_PropertyHapa.Tenant.Services.IServices
{
    public interface IBaseService
    {
        APIResponse responseModel { get; set; }
        Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true);
    }
}
