
using PMS_PropertyHapa.Tenant.Models;

namespace PMS_PropertyHapa.Tenant.Services.IServices
{
    public interface IApiMessageRequestBuilder
    {
        HttpRequestMessage Build(APIRequest apiRequest);
    }
}
