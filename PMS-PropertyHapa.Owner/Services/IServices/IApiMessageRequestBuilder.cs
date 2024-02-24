

using PMS_PropertyHapa.Owner.Models;

namespace PMS_PropertyHapa.Owner.Services.IServices
{
    public interface IApiMessageRequestBuilder
    {
        HttpRequestMessage Build(APIRequest apiRequest);
    }
}
