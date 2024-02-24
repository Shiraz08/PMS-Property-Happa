

using PMS_PropertyHapa.Staff.Models;

namespace PMS_PropertyHapa.Staff.Services.IServices
{
    public interface IApiMessageRequestBuilder
    {
        HttpRequestMessage Build(APIRequest apiRequest);
    }
}
