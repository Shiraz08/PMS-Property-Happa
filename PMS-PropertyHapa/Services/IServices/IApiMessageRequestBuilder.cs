

using PMS_PropertyHapa.Models;

namespace PMS_PropertyHapa.Services.IServices
{
    public interface IApiMessageRequestBuilder
    {
        HttpRequestMessage Build(APIRequest apiRequest);
    }
}
