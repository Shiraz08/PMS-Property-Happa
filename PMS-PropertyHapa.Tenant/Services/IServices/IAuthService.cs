using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Tenant.Models;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Tenant.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
        Task<T> RegisterAsync<T>(RegisterationRequestDTO obj);
        Task<T> LogoutAsync<T>(TokenDTO obj);

        Task<T> ChangePasswordAsync<T>(ChangePasswordRequestDto obj);

        Task<APIResponse> ForgotPasswordAsync(ForgetPassword obj);

        Task<T> ResetPasswordAsync<T>(ResetPasswordDto obj);

        Task<bool> UpdateProfileAsync(ProfileModel model);

        Task<ProfileModel> GetProfileAsync(string userId);

        Task<IEnumerable<UserDTO>> GetAllUsersAsync();


        Task<IEnumerable<TenantModelDto>> GetAllTenantsAsync();
        Task<List<TenantModelDto>> GetTenantsByIdAsync(string tenantId);
        Task<bool> CreateTenantAsync(TenantModelDto tenant);
        Task<bool> UpdateTenantAsync(TenantModelDto tenant);
        Task<bool> DeleteTenantAsync(string tenantId);

        Task<TenantModelDto> GetSingleTenantAsync(int tenantId);


        Task<bool> CreateAssetAsync(AssetDTO asset);



        Task<List<PropertyTypeDto>> GetAllPropertyTypesAsync();
        Task<List<PropertyTypeDto>> GetPropertyTypeByIdAsync(string tenantId);
        Task<bool> CreatePropertyTypeAsync(PropertyTypeDto propertyType);
        Task<bool> UpdatePropertyTypeAsync(PropertyTypeDto propertyType);
        Task<bool> DeletePropertyTypeAsync(int propertytypeId);

        Task<PropertyTypeDto> GetSinglePropertyTypeAsync(int propertytypeId);






        Task<List<PropertySubTypeDto>> GetAllPropertySubTypesAsync();
        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAsync(int propertytypeId);
        Task<bool> CreatePropertySubTypeAsync(PropertySubTypeDto propertyType);
        Task<bool> UpdatePropertySubTypeAsync(PropertySubTypeDto propertyType);
        Task<bool> DeletePropertySubTypeAsync(int propertysubtypeId);

        Task<PropertySubTypeDto> GetSinglePropertySubTypeAsync(int propertysubtypeId);

        Task<List<PropertyTypeDto>> GetAllPropertyTypes();

        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAllAsync(string tenantId);


        Task<bool>UpdateTenantOrganizationAsync(TenantOrganizationInfoDto tenant);

        Task<TenantOrganizationInfoDto> GetTenantOrganizationByIdAsync(int tenantId);

    }
}
