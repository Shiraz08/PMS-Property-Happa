using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Staff.Models;
using APIResponse = PMS_PropertyHapa.Staff.Models.APIResponse;

namespace PMS_PropertyHapa.Staff.Services.IServices
{
    public interface IBaseService
    {
        APIResponse responseModel { get; set; }
        Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true);
    }
}
