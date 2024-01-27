
using PMS_PropertyHapa.Models.DTO;

namespace PMS_PropertyHapa.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
        Task<T> RegisterAsync<T>(RegisterationRequestDTO objToCreate);
        Task<T> LogoutAsync<T>(TokenDTO obj);
    }
}
