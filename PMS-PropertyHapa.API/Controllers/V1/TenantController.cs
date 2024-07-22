using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using NuGet.ContentModel;
using static PMS_PropertyHapa.Models.DTO.TenantModelDto;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/Tenantauth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class TenantController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;

        public TenantController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
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

        [HttpGet("Tenant")]
        public async Task<IActionResult> GetAllTenants()
        {
            try
            {
                var tenants = await _userRepo.GetAllTenantsAsync();
                _response.Result = tenants;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("TenantDll")]
        public async Task<IActionResult> GetAllTenantsDll(Filter filter)
        {
            try
            {
                var tenants = await _userRepo.GetAllTenantsDllAsync(filter);
                _response.Result = tenants;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("Tenant/{tenantId}")]
        public async Task<IActionResult> GetTenantById(string tenantId)
        {
            try
            {
                var tenantDto = await _userRepo.GetTenantsByIdAsync(tenantId);

                if (tenantDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = tenantDto;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occured");
                return NotFound(_response);
            }
        }


        [HttpGet("GetSingleTenant/{tenantId}")]
        public async Task<IActionResult> GetSingleTenant(int tenantId)
        {
            try
            {
                var tenantDto = await _userRepo.GetSingleTenantByIdAsync(tenantId);

                if (tenantDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = tenantDto;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occured");
                return NotFound(_response);
            }
        }


        [HttpPost("Tenant")]
        public async Task<ActionResult<bool>> CreateTenant(TenantModelDto tenant)
        {
            try
            {
                var isSuccess = await _userRepo.CreateTenantAsync(tenant);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("Tenant/{tenantId}")]
        public async Task<ActionResult<bool>> UpdateTenant(int tenantId, TenantModelDto tenant)
        {
            try
            {
                tenant.TenantId = tenantId; // Ensure tenantId is set
                var isSuccess = await _userRepo.UpdateTenantAsync(tenant);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("DeleteTenant/{tenantId}")]
        public async Task<ActionResult<APIResponse>> DeleteTenant(int tenantId)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteTenantAsync(tenantId);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return NotFound(_response);
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
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = tenantDto;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occured");
                return NotFound(_response);
            }
        }


        [HttpPost("UpdateTenantOrg/{tenantId}")]
        public async Task<ActionResult<bool>> UpdateTenantOrg(int tenantId, TenantOrganizationInfoDto tenant)
        {
            try
            {
                tenant.Id = tenantId; // Ensure tenantId is set
                var isSuccess = await _userRepo.UpdateTenantOrgAsync(tenant);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("TenantsReport")]
        public async Task<IActionResult> GetTenantsReport(ReportFilter reportFilter)
        {
            try
            {
                var tenants = await _userRepo.GetTenantsReportAsync(reportFilter);
                if (tenants != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = tenants;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
                return Ok(_response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("InvoicesReport")]
        public async Task<IActionResult> GetInvoicesReport(ReportFilter reportFilter)
        {
            try
            {
                var invoices = await _userRepo.GetInvoicesReportAsync(reportFilter);
                if (invoices != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = invoices;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
                return Ok(_response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("TenantDependents")]
        public async Task<IActionResult> GetTenantDependents(ReportFilter reportFilter)
        {
            try
            {
                var dependents = await _userRepo.GetTenantDependentsAsync(reportFilter);
                if (dependents != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = dependents;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
                return Ok(_response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        #endregion
    }
}