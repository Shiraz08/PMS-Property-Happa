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
    [Route("api/v1/GetDataByIdAuth")]
    [ApiController]
    public class GetDataByIdController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;
        private readonly GoogleCloudStorageService _storageService;

        public GetDataByIdController(IUserRepository userRepo, UserManager<ApplicationUser> userManager , GoogleCloudStorageService storageService)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
            _storageService = storageService;
        }


        [HttpGet("GetLandlordDataById/{id}")]
        public async Task<ActionResult> GetLandlordDataById(int id)
        {
            try
            {
                var landlordData = await _userRepo.GetLandlordDataById(id);

                if (landlordData != null)
                {
                    return Ok(landlordData);
                }
                else
                {
                    return NotFound("No user found with this id.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetTenantDataById/{id}")]
        public async Task<ActionResult> GetTenantDataById(int id)
        {
            try
            {
                var TenantData = await _userRepo.GetTenantDataById(id);

                if (TenantData != null)
                {
                    return Ok(TenantData);
                }
                else
                {
                    return NotFound("No user found with this id.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        //Calender Start
        [HttpPost("CalendarEvents")]
        public async Task<ActionResult<CalendarEvent>> GetCalendarEvents(CalendarFilterModel filter)
        {
            try
            {
                var events = await _userRepo.GetCalendarEventsAsync(filter);

                if (events != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = events;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No asset found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("OccupancyOverviewEvents")]
        public async Task<ActionResult<OccupancyOverviewEvents>> GetOccupancyOverviewEvents(CalendarFilterModel filter)
        {
            try
            {
                var events = await _userRepo.GetOccupancyOverviewEventsAsync(filter);

                if (events != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = events;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No asset found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("LeaseData/{id}")]
        public async Task<ActionResult<LeaseDataDto>> GetLeaseDataByIdAsync(int id)
        {
            try
            {
                var events = await _userRepo.GetLeaseDataByIdAsync(id);

                if (events != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = events;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No asset found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Calender End
        [HttpGet("GetTaskRequestHistory/{id}")]
        public async Task<ActionResult<TaskRequestHistoryDto>> GetTaskRequestHistory(int id)
        {

            try
            {
                var taskDto = await _userRepo.GetTaskRequestHistoryAsync(id);

                if (taskDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = taskDto;
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

        //Task & Maintenaince
        [HttpGet("MaintenanceTasks")]
        public async Task<ActionResult<TaskRequestDto>> GetMaintenanceTasks()
        {
            try
            {
                var assets = await _userRepo.GetMaintenanceTasksAsync();

                if (assets != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = assets;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No asset found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpGet("Tasks")]
        public async Task<ActionResult<TaskRequestDto>> GetTasks()
        {
            try
            {
                var assets = await _userRepo.GetTaskRequestsAsync();

                if (assets != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = assets;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No asset found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetTaskById/{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {

            try
            {
                var taskDto = await _userRepo.GetTaskByIdAsync(id);

                if (taskDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = taskDto;
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

        [HttpPost("Task")]
        public async Task<ActionResult<bool>> SaveTaskRequest(TaskRequestDto taskRequestDto)
        {
            try
            {
                var isSuccess = await _userRepo.SaveTaskAsync(taskRequestDto);
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


        [HttpPost("Task/{id}")]
        public async Task<ActionResult<bool>> DeleteTaskRequest(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteTaskAsync(id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost("TaskHistory")]
        public async Task<ActionResult<bool>> SaveTaskHistory(TaskRequestHistoryDto taskRequestHistoryDto)
        {
            try
            {
                var isSuccess = await _userRepo.SaveTaskHistoryAsync(taskRequestHistoryDto);
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


        // Account Type Start

        [HttpGet("AccountTypes")]
        public async Task<ActionResult<AccountType>> GetAccountTypes()
        {
            try
            {
                var accountTypes = await _userRepo.GetAccountTypesAsync();

                if (accountTypes != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = accountTypes;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No account found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetAccountTypeById/{id}")]
        public async Task<IActionResult> GetAccountTypeById(int id)
        {

            try
            {
                var accountType = await _userRepo.GetAccountTypeByIdAsync(id);

                if (accountType != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = accountType;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No account found with this id.");
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

        [HttpPost("AccountType")]
        public async Task<ActionResult<bool>> SaveAccountType(AccountType accountType)
        {
            try
            {
                var isSuccess = await _userRepo.SaveAccountTypeAsync(accountType);
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

        [HttpPost("AccountType/{id}")]
        public async Task<ActionResult<bool>> DeleteAccountTypeRequest(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteAccountTypeAsync(id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Account Type End



        // Account Sub Type Start

        [HttpGet("AccountSubTypes")]
        public async Task<ActionResult<AccountSubTypeDto>> GetAccountSubTypes()
        {
            try
            {
                var accountSubTypes = await _userRepo.GetAccountSubTypesAsync();

                if (accountSubTypes != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = accountSubTypes;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No sub account found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetAccountSubTypeById/{id}")]
        public async Task<IActionResult> GetAccountSubTypeById(int id)
        {

            try
            {
                var accountSubType = await _userRepo.GetAccountSubTypeByIdAsync(id);

                if (accountSubType != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = accountSubType;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No sub account found with this id.");
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

        [HttpPost("AccountSubType")]
        public async Task<ActionResult<bool>> SaveAccountSubType(AccountSubType accountSubType)
        {
            try
            {
                var isSuccess = await _userRepo.SaveAccountSubTypeAsync(accountSubType);
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

        [HttpPost("AccountSubType/{id}")]
        public async Task<ActionResult<bool>> DeleteAccountSubTypeRequest(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteAccountSubTypeAsync(id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Account Sub Type End


        // Account Chart Account Start

        [HttpGet("ChartAccounts")]
        public async Task<ActionResult<ChartAccountDto>> GetChartAccounts()
        {
            try
            {
                var chartAccounts = await _userRepo.GetChartAccountsAsync();

                if (chartAccounts != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = chartAccounts;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No sub account found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetChartAccountById/{id}")]
        public async Task<IActionResult> GetChartAccountById(int id)
        {

            try
            {
                var chartAccount = await _userRepo.GetChartAccountByIdAsync(id);

                if (chartAccount != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = chartAccount;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No sub account found with this id.");
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

        [HttpPost("ChartAccount")]
        public async Task<ActionResult<bool>> SaveChartAccount(ChartAccount chartAccount)
        {
            try
            {
                var isSuccess = await _userRepo.SaveChartAccountAsync(chartAccount);
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

        [HttpPost("ChartAccount/{id}")]
        public async Task<ActionResult<bool>> DeleteChartAccountRequest(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteChartAccountAsync(id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Account Sub Type End


        [HttpPost("uploadImage")]
        
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var url = await _storageService.UploadImageAsync(file);
            return Ok(url);
        }

    }
}
