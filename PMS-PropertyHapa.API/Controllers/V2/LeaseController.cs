using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.ViewModels;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;
using System.Net;

namespace PMS_PropertyHapa.API.Controllers.V2
{
    public class LeaseController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected ApiResponseUser _response;

        public LeaseController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
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

        #region Lease Crud 

        [HttpPost("Lease")]
        public async Task<ActionResult<bool>> CreateLease(LeaseDto lease)
        {
            try
            {
                var isSuccess = await _userRepo.CreateLeaseAsync(lease);

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
                        TextInfo = "Failed to create lease.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to create lease",
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

        [HttpGet("Lease/{leaseId}")]
        public async Task<ActionResult<LeaseDto>> GetLeaseById(int leaseId)
        {
            try
            {
                var lease = await _userRepo.GetLeaseByIdAsync(leaseId);

                if (lease != null)
                {
                    return Ok(lease);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Lease with ID {leaseId} not found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Lease with ID {leaseId} not found",
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

        [HttpGet("Leases")]
        public async Task<ActionResult<IEnumerable<LeaseDto>>> GetAllLeases()
        {
            try
            {
                var leases = await _userRepo.GetAllLeasesAsync();

                if (leases != null && leases.Any())
                {
                    return Ok(leases);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "No leases found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "No leases found",
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

        [HttpPut("Lease")]
        public async Task<ActionResult<bool>> UpdateLease(LeaseDto lease)
        {
            try
            {
                var isSuccess = await _userRepo.UpdateLeaseAsync(lease);

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
                        TextInfo = $"Lease with ID {lease.LeaseId} could not be updated.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Lease with ID {lease.LeaseId} could not be updated",
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
