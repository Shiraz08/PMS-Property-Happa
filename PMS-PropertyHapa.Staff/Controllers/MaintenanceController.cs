using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;
        EmailSender _emailSender = new EmailSender();

        public MaintenanceController(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService = permissionService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewMaintenance);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMaintenanceTasks()
        {
            IEnumerable<TaskRequestDto> maintenanceTasks = new List<TaskRequestDto>();
            maintenanceTasks = await _authService.GetMaintenanceTasksAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                maintenanceTasks = maintenanceTasks.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(maintenanceTasks);
        }




        [HttpPost]
        public async Task<IActionResult> SaveTaskHistory([FromForm] TaskRequestHistoryDto taskRequestHistoryDto)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddMaintenance);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (taskRequestHistoryDto == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            TaskRequestDto taskRequestDto = await _authService.GetTaskRequestByIdAsync(taskRequestHistoryDto.TaskRequestId);
            var currentDateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

            switch (taskRequestDto.Type)
            {
                case TaskTypes.Task:
                    // Assigness Missing
                    break;
                case TaskTypes.TenantRequest:
                    if (taskRequestDto.IsNotifyTenant && taskRequestDto.TenantId != null)
                    {
                        var tenant = await _authService.GetSingleTenantAsync(taskRequestDto.TenantId ?? 0);

                        string htmlContent = $@"<!DOCTYPE html>
                                             <html lang=""en"">
                                             <head>
                                                 <meta charset=""UTF-8"">
                                                 <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                                 <title>Task Status Update</title>
                                             </head>
                                             <body>
                                                 <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                                     <p>Hello {tenant.FirstName} {tenant.LastName},</p>
                                                     <p>We wanted to inform you that the status of your task has been updated. Here are the details:</p>
                                                     <p><strong>Task Type:</strong> {taskRequestDto.Type}</p>
                                                     <p><strong>Task Description:</strong> {taskRequestDto.Description}</p>
                                                     <p><strong>New Status:</strong> {taskRequestHistoryDto.Status}</p>
                                                     <p><strong>Remarks:</strong> {taskRequestHistoryDto.Remarks}</p>
                                                     <p><strong>Update Date and Time:</strong> {currentDateTime}</p>
                                                     <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                                                     <p>Thank you!</p>
                                                 </div>
                                             </body>
                                             </html>";

                        await _emailSender.SendEmailAsync(tenant.EmailAddress, taskRequestDto.Subject, htmlContent);
                    }
                    break;
                case TaskTypes.OwnerRequest:
                    if (taskRequestDto.IsNotifyOwner && taskRequestDto.OwnerId != null)
                    {
                        var owner = await _authService.GetSingleLandlordAsync(taskRequestDto.OwnerId ?? 0);
                        string htmlContent = $@"<!DOCTYPE html>
                                             <html lang=""en"">
                                             <head>
                                                 <meta charset=""UTF-8"">
                                                 <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                                 <title>Task Status Update</title>
                                             </head>
                                             <body>
                                                 <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                                     <p>Hello {owner.FirstName} {owner.LastName},</p>
                                                     <p>We wanted to inform you that the status of your task has been updated. Here are the details:</p>
                                                     <p><strong>Task Type:</strong> {taskRequestDto.Type}</p>
                                                     <p><strong>Task Description:</strong> {taskRequestDto.Description}</p>
                                                     <p><strong>New Status:</strong> {taskRequestHistoryDto.Status}</p>
                                                     <p><strong>Remarks:</strong> {taskRequestHistoryDto.Remarks}</p>
                                                     <p><strong>Update Date and Time:</strong> {currentDateTime}</p>
                                                     <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                                                     <p>Thank you!</p>
                                                 </div>
                                             </body>
                                             </html>";


                        await _emailSender.SendEmailAsync(owner.EmailAddress, taskRequestDto.Subject, htmlContent);
                    }
                    break;
                case TaskTypes.WorkOrderRequest:
                    if (taskRequestDto.IsNotifyAssignee && taskRequestDto.VendorId != null)
                    {
                        var vendor = await _authService.GetVendorByIdAsync(taskRequestDto.VendorId ?? 0);

                        string htmlContent = $@"<!DOCTYPE html>
                                             <html lang=""en"">
                                             <head>
                                                 <meta charset=""UTF-8"">
                                                 <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                                 <title>Task Status Update</title>
                                             </head>
                                             <body>
                                                 <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                                     <p>Hello {vendor.FirstName} {vendor.LastName},</p>
                                                     <p>We are pleased to inform you that the status of your task has been updated. Here are the details:</p>
                                                     <p><strong>Task Type:</strong> {taskRequestDto.Type}</p>
                                                     <p><strong>Task Description:</strong> {taskRequestDto.Description}</p>
                                                     <p><strong>New Status:</strong> {taskRequestHistoryDto.Status}</p>
                                                     <p><strong>Remarks:</strong> {taskRequestHistoryDto.Remarks}</p>
                                                     <p><strong>Update Date and Time:</strong> {currentDateTime}</p>
                                                     <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                                                     <p>Thank you!</p>
                                                 </div>
                                             </body>
                                             </html>";

                        await _emailSender.SendEmailAsync(vendor.Email1, taskRequestDto.Subject, htmlContent);
                    }
                    break;
                default:
                    break;
            }


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
