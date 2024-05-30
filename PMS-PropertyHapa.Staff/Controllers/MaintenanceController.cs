using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly IAuthService _authService;
        EmailSender _emailSender = new EmailSender();

        public MaintenanceController(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<TaskRequestDto> maintenanceTasks = new List<TaskRequestDto>();
            maintenanceTasks = await _authService.GetMaintenanceTasksAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                maintenanceTasks = maintenanceTasks.Where(s => s.AddedBy == currenUserId);
            }
            return View(maintenanceTasks);
        }


        [HttpPost]
        public async Task<IActionResult> SaveTaskHistory([FromBody] TaskRequestHistoryDto taskRequestHistoryDto)
        {
            if (taskRequestHistoryDto == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            TaskRequestDto taskRequestDto = await _authService.GetTaskRequestByIdAsync(taskRequestHistoryDto.TaskRequestId);
            //var currentDateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

            //switch (taskRequestDto.Type)
            //{
            //    case TaskTypes.Task:
            //        // Assigness Missing
            //        break;
            //    case TaskTypes.TenantRequest:
            //        if (taskRequestDto.IsNotifyTenant && taskRequestDto.TenantId != null)
            //        {
            //            var tenant = await _authService.GetSingleTenantAsync(taskRequestDto.TenantId ?? 0);

            //            var emailContent = $"Hello {tenant.FirstName} {tenant.LastName},\n\n" +
            //                               "We wanted to inform you that the status of your task has been updated. Here are the details:\n\n" +
            //                               $"Task Type: {taskRequestDto.Type}\n" +
            //                               $"Task Description: {taskRequestDto.Description}\n\n" +
            //                               $"New Status: {taskRequestHistoryDto.Status}\n" +
            //                               $"Remarks: {taskRequestHistoryDto.Remarks}\n\n" +
            //                               $"Update Date and Time: {currentDateTime}\n\n" +
            //                               "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                               "Thank you!";
            //            await _emailSender.SendEmailAsync(tenant.EmailAddress, taskRequestDto.Subject, emailContent);
            //        }
            //        break;
            //    case TaskTypes.OwnerRequest:
            //        if (taskRequestDto.IsNotifyOwner && taskRequestDto.OwnerId != null)
            //        {
            //            var owner = await _authService.GetSingleLandlordAsync(taskRequestDto.OwnerId ?? 0);

            //            var emailContent = $"Hello {owner.FirstName} {owner.LastName},\n\n" +
            //                        "We wanted to inform you that a new task has been assigned to you. Here are the details:\n\n" +
            //                        $"Task Type: {taskRequestDto.Type}\n" +
            //                        $"Task Description: {taskRequestDto.Description}\n\n" +
            //                        $"Assignment Date and Time: {currentDateTime}\n\n" +
            //                        "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                        "Thank you!";
            //            await _emailSender.SendEmailAsync(owner.EmailAddress, taskRequestDto.Subject, emailContent);
            //        }
            //        break;
            //    case TaskTypes.WorkOrderRequest:
            //        if (taskRequestDto.IsNotifyAssignee && taskRequestDto.VendorId != null)
            //        {
            //            var vendor = await _authService.GetVendorByIdAsync(taskRequestDto.VendorId ?? 0);

            //            var emailContent = $"Hello {vendor.FirstName} {vendor.LastName},\n\n" +
            //                               "We are pleased to inform you that a new task has been assigned to you. Here are the details:\n\n" +
            //                               $"Task Type: {taskRequestDto.Type}\n" +
            //                               $"Task Description: {taskRequestDto.Description}\n\n" +
            //                               "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                               "Thank you!";
            //            await _emailSender.SendEmailAsync(vendor.Email1, taskRequestDto.Subject, emailContent);
            //        }
            //        break;
            //    default:
            //        break;
            //}


            taskRequestHistoryDto.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveTaskHistoryAsync(taskRequestHistoryDto);
            return Json(new { success = true, message = "Task added successfully" });
        }

        public async Task<IActionResult> GetTaskRequestHistory(int taskRequestId)
        {

            IEnumerable<TaskRequestHistoryDto> taskRequestHistory = new List<TaskRequestHistoryDto>();
            taskRequestHistory = await _authService.GetTaskRequestHistoryAsync(taskRequestId);
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                taskRequestHistory = taskRequestHistory.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(taskRequestHistory);
        }

    }
}
