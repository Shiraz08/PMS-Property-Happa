
using PMS_PropertyHapa.Models.DTO;

namespace PMS_PropertyHapa.Tenant.Services.IServices
{
    public interface ITokenProvider
    {
        void SetToken(TokenDTO tokenDTO);
        TokenDTO? GetToken();
        void ClearToken();
    }
}
