using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using PMS_PropertyHapa.Shared.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Web;
using System.Security.Claims;
using System.Net.Http.Headers;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/LeaseAuth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class LeaseController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;

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
                if (isSuccess == true)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = isSuccess;
                }
                return Ok(_response);
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
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = lease;
                    return Ok(_response);
                }
                else
                {
                    return NotFound($"Lease with ID {leaseId} not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpGet("Invoices/{leaseId}")]
        public async Task<ActionResult<List<Invoice>>> GetInvoices(int leaseId)
        {
            try
            {
                var invoices = await _userRepo.GetInvoicesAsync(leaseId);
                if (invoices != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = invoices;
                    return Ok(_response);
                }
                else
                {
                    return NotFound($"Invoices with ID {leaseId} not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    
        [HttpGet("Invoice/{invoiceId}")]
        public async Task<ActionResult<Invoice>> GetInvoiceById(int invoiceId)
        {
            try
            {
                var invoice = await _userRepo.GetInvoiceByIdAsync(invoiceId);
                if (invoice != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = invoice;
                    return Ok(_response);
                }
                else
                {
                    return NotFound($"Invoices with ID {invoiceId} not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("AllInvoicePaid/{leaseId}")]
        public async Task<ActionResult<bool>> AllInvoicePaid(int leaseId)
        {
            try
            {
                var isSuccess = await _userRepo.AllInvoicePaidAsync(leaseId);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpPost("AllInvoiceOwnerPaid/{leaseId}")]
        public async Task<ActionResult<bool>> AllInvoiceOwnerPaid(int leaseId)
        {
            try
            {
                var isSuccess = await _userRepo.AllInvoiceOwnerPaidAsync(leaseId);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpPost("InvoicePaid/{invoiceId}")]
        public async Task<ActionResult<bool>> InvoicePaid(int invoiceId)
        {
            try
            {
                var isSuccess = await _userRepo.InvoicePaidAsync(invoiceId);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpPost("InvoiceOwnerPaid/{invoiceId}")]
        public async Task<ActionResult<bool>> InvoiceOwnerPaid(int invoiceId)
        {
            try
            {
                var isSuccess = await _userRepo.InvoiceOwnerPaidAsync(invoiceId);
                return Ok(isSuccess);
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
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = leases;
                return Ok(_response);
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
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = isSuccess;
                    return Ok(_response);
                }
                else
                {
                    return NotFound($"Lease with ID {lease.LeaseId} could not be updated.");
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