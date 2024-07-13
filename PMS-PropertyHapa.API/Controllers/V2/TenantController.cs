using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.ViewModels;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;

namespace PMS_PropertyHapa.API.Controllers.V2
{
    public class TenantController : Controller
    {

        private readonly IUserRepository _userRepo;

        public TenantController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet("Error")]
        public async Task<IActionResult> Error()
        {
            throw new FileNotFoundException();
        }

        [HttpGet("ImageError")]
        public async Task<IActionResult> ImageError()
        {
            throw new BadImageFormatException("Fake Image Exception");
        }

        #region TenantCrud 
        [Authorize]
        [HttpGet("Tenant")]
        public async Task<IActionResult> GetAllTenants()
        {
            try
            {
                var tenants = await _userRepo.GetAllTenantsAsync();

                if (tenants != null && tenants.Any())
                {
                    return Ok(tenants);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "No tenants found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "No tenants found",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [Authorize]
        [HttpGet("Tenant/{tenantId}")]
        public async Task<IActionResult> GetTenantById(string tenantId)
        {
            try
            {
                var tenantDto = await _userRepo.GetTenantsByIdAsync(tenantId);

                if (tenantDto != null)
                {
                    return Ok(tenantDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"No tenant found with ID: {tenantId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No tenant found with ID: {tenantId}",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("GetSingleTenant/{tenantId}")]
        public async Task<IActionResult> GetSingleTenant(int tenantId)
        {
            try
            {
                var tenantDto = await _userRepo.GetSingleTenantByIdAsync(tenantId);

                if (tenantDto != null)
                {
                    return Ok(tenantDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"No tenant found with ID: {tenantId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No tenant found with ID: {tenantId}",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("Tenant")]
        public async Task<ActionResult<bool>> CreateTenant(TenantModelDto tenant)
        {
            try
            {
                var isSuccess = await _userRepo.CreateTenantAsync(tenant);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "Failed to create tenant.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to create tenant",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPut("Tenant/{tenantId}")]
        public async Task<ActionResult<bool>> UpdateTenant(int tenantId, TenantModelDto tenant)
        {
            try
            {
                tenant.TenantId = tenantId; // Ensure tenantId is set
                var isSuccess = await _userRepo.UpdateTenantAsync(tenant);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Tenant with ID: {tenantId} could not be updated.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Tenant with ID: {tenantId} could not be updated",
                        Title = "Error"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPost("Tenant/{tenantId}")]
        public async Task<ActionResult<APIResponse>> DeleteTenant(int tenantId)
        {
            try
            {
                var response = await _userRepo.DeleteTenantAsync(tenantId);
                return response;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        #endregion




        #region TenantOrg
        [HttpGet("TenantOrg/{tenantId}")]
        public async Task<IActionResult> GetTenantOrgById(int tenantId)
        {
            try
            {
                var tenantDto = await _userRepo.GetTenantOrgByIdAsync(tenantId);

                if (tenantDto != null)
                {
                    return Ok(tenantDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"No tenant found with ID: {tenantId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No tenant found with ID: {tenantId}",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPut("TenantOrg/{tenantId}")]
        public async Task<ActionResult<bool>> UpdateTenantOrg(int tenantId, TenantOrganizationInfoDto tenant)
        {
            try
            {
                tenant.Id = tenantId; // Ensure tenantId is set
                var isSuccess = await _userRepo.UpdateTenantOrgAsync(tenant);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Tenant with ID: {tenantId} could not be updated.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Tenant with ID: {tenantId} could not be updated",
                        Title = "Error"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        #endregion

    }
}
