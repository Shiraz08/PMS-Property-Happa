using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.Net;
using Twilio.Http;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class TaskController : Controller
    {
        private readonly IAuthService _authService;
        EmailSender _emailSender = new EmailSender();

        public TaskController(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<IActionResult> ViewTask()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskRequests()
        {
            IEnumerable<TaskRequestDto> taskRequests = new List<TaskRequestDto>();
            taskRequests = await _authService.GetTaskRequestsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                taskRequests = taskRequests.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(taskRequests);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTask([FromBody] TaskRequestDto taskRequestDto)
        {
            if (taskRequestDto == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            switch (taskRequestDto.Type)
            {
                case TaskTypes.Task:
                    // Assigness Missing
                    break;
                case TaskTypes.TenantRequest:
                    if (taskRequestDto.IsNotifyTenant && taskRequestDto.TenantId != null)
                    {
                        var tenant = await _authService.GetSingleTenantAsync(taskRequestDto.TenantId ?? 0);

                        string htmlContent = taskRequestDto.Status == TaskStatusTypes.NotStarted.ToString() ?
                     $@"<!DOCTYPE html>
                         <html lang=""en"">
                         <head>
                             <meta charset=""UTF-8"">
                             <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                             <title>New Task Assignment</title>
                         </head>
                         <body>
                             <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                 <p>Hello {tenant.FirstName} {tenant.LastName},</p>
                                 <p>We are pleased to inform you that a new task has been assigned to you. Here are the details:</p>
                                 <p><strong>Task Type:</strong> {taskRequestDto.Type}</p>
                                 <p><strong>Task Description:</strong> {taskRequestDto.Description}</p>
                                 <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                                 <p>Thank you!</p>
                             </div>
                         </body>
                         </html>" :
                     $@"<!DOCTYPE html>
                         <html lang=""en"">
                         <head>
                             <meta charset=""UTF-8"">
                             <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                             <title>Task Status Update</title>
                         </head>
                         <body>
                             <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                 <p>Hello {tenant.FirstName} {tenant.LastName},</p>
                                 <p>We are pleased to inform you that your task has been moved to {taskRequestDto.Status} on {DateTime.Now.ToString("yyyy-MM-dd")}.</p>
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

                        string htmlContent = taskRequestDto.Status == TaskStatusTypes.NotStarted.ToString() ?
                                $@"<!DOCTYPE html>
                                    <html lang=""en"">
                                    <head>
                                        <meta charset=""UTF-8"">
                                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                        <title>New Task Assignment</title>
                                    </head>
                                    <body>
                                        <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                            <p>Hello {owner.FirstName} {owner.LastName},</p>
                                            <p>We are pleased to inform you that a new task has been assigned to you. Here are the details:</p>
                                            <p><strong>Task Type:</strong> {taskRequestDto.Type}</p>
                                            <p><strong>Task Description:</strong> {taskRequestDto.Description}</p>
                                            <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                                            <p>Thank you!</p>
                                        </div>
                                    </body>
                                    </html>" :
                                $@"<!DOCTYPE html>
                                    <html lang=""en"">
                                    <head>
                                        <meta charset=""UTF-8"">
                                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                        <title>Task Status Update</title>
                                    </head>
                                    <body>
                                        <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                            <p>Hello {owner.FirstName} {owner.LastName},</p>
                                            <p>We are pleased to inform you that your task has been moved to {taskRequestDto.Status} on {DateTime.Now.ToString("yyyy-MM-dd")}.</p>
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

                        string htmlContent = taskRequestDto.Status == TaskStatusTypes.NotStarted.ToString() ?
                                             $@"<!DOCTYPE html>
                                         <html lang=""en"">
                                         <head>
                                             <meta charset=""UTF-8"">
                                             <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                             <title>New Task Assignment</title>
                                         </head>
                                         <body>
                                             <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                                 <p>Hello {vendor.FirstName} {vendor.LastName},</p>
                                                 <p>We are pleased to inform you that a new task has been assigned to you. Here are the details:</p>
                                                 <p><strong>Task Type:</strong> {taskRequestDto.Type}</p>
                                                 <p><strong>Task Description:</strong> {taskRequestDto.Description}</p>
                                                 <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                                                 <p>Thank you!</p>
                                             </div>
                                         </body>
                                         </html>" :
                                                             $@"<!DOCTYPE html>
                                         <html lang=""en"">
                                         <head>
                                             <meta charset=""UTF-8"">
                                             <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                             <title>Task Status Update</title>
                                         </head>
                                         <body>
                                             <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                                 <p>Hello {vendor.FirstName} {vendor.LastName},</p>
                                                 <p>We are pleased to inform you that your task has been moved to {taskRequestDto.Status} on {DateTime.Now.ToString("yyyy-MM-dd")}.</p>
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

            taskRequestDto.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveTaskAsync(taskRequestDto);
            return Json(new { success = true, message = "Task added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            await _authService.DeleteTaskAsync(id);
            return Json(new { success = true, message = "Task deleted successfully" });
        }

        public async Task<IActionResult> GetTaskById(int id)
        {
            TaskRequestDto task = await _authService.GetTaskRequestByIdAsync(id);
            if (task == null)
            {
                return StatusCode(500, "Task request not found");
            }
            return Ok(task);
        }
    }
}
