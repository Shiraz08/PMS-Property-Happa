using Google.Apis.Storage.v1;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.Services;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using System.Net;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/ReportsAuth")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;
        private readonly GoogleCloudStorageService _storageService;

        public ReportsController(IUserRepository userRepo, UserManager<ApplicationUser> userManager, GoogleCloudStorageService storageService)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
            _storageService = storageService;
        }


        [HttpPost("LeaseReport")]
        public async Task<ActionResult<LeaseReportDto>> GetLeaseReport(ReportFilter reportFilter)
        {
            try
            {
                var leaseReport = await _userRepo.GetLeaseReportAsync(reportFilter);

                if (leaseReport != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = leaseReport;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No lease found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost("InvoiceReport")]
        public async Task<ActionResult<InvoiceReportDto>> GetInvoiceReports(ReportFilter reportFilter)
        {
            try
            {
                var invoiceReport = await _userRepo.GetInvoiceReportAsync(reportFilter);

                if (invoiceReport != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = invoiceReport;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No invoice found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("TaskReport")]
        public async Task<ActionResult<TaskRequestReportDto>> GetTaskRequestReports(ReportFilter reportFilter)
        {
            try
            {
                var taskReport = await _userRepo.GetTaskRequestReportAsync(reportFilter);

                if (taskReport != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = taskReport;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No task request found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
