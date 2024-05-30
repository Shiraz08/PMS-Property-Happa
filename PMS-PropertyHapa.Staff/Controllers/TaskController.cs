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
            IEnumerable<TaskRequestDto> taskRequests = new List<TaskRequestDto>();
            taskRequests = await _authService.GetTaskRequestsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                taskRequests = taskRequests.Where(s => s.AddedBy == currenUserId);
            }
            return View(taskRequests);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTask([FromBody] TaskRequestDto taskRequestDto)
        {
            if (taskRequestDto == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            //switch (taskRequestDto.Type)
            //{
            //    case TaskTypes.Task:
            //        // Assigness Missing
            //        break;
            //    case TaskTypes.TenantRequest:
            //        if (taskRequestDto.IsNotifyTenant && taskRequestDto.TenantId != null)
            //        {
            //            var tenant = await _authService.GetSingleTenantAsync(taskRequestDto.TenantId ?? 0);

            //            var emailContent = taskRequestDto.Status == TaskStatusTypes.NotStarted.ToString() ?
            //                                $"Hello {tenant.FirstName} {tenant.LastName},\n\n" +
            //                                "We are pleased to inform you that a new task has been assigned to you. Here are the details:\n\n" +
            //                                $"Task Type: {taskRequestDto.Type}\n" +
            //                                $"Task Description: {taskRequestDto.Description}\n\n" +
            //                                "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                                "Thank you!" :
            //                                $"Hello {tenant.FirstName} {tenant.LastName},\n\n" +
            //                                $"We are pleased to inform you that your task has been moved to {taskRequestDto.Status} on {DateTime.Now.ToString("yyyy-MM-dd")}.\n\n" +
            //                                "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                                "Thank you!";
            //            await _emailSender.SendEmailAsync(tenant.EmailAddress, taskRequestDto.Subject, emailContent);
            //        }
            //        break;
            //    case TaskTypes.OwnerRequest:
            //        if (taskRequestDto.IsNotifyOwner && taskRequestDto.OwnerId != null)
            //        {
            //            var owner = await _authService.GetSingleLandlordAsync(taskRequestDto.OwnerId ?? 0);

            //            var emailContent = taskRequestDto.Status == TaskStatusTypes.NotStarted.ToString() ?
            //                                 $"Hello {owner.FirstName} {owner.LastName},\n\n" +
            //                                 "We are pleased to inform you that a new task has been assigned to you. Here are the details:\n\n" +
            //                                 $"Task Type: {taskRequestDto.Type}\n" +
            //                                 $"Task Description: {taskRequestDto.Description}\n\n" +
            //                                 "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                                 "Thank you!" :
            //                                 $"Hello {owner.FirstName} {owner.LastName},\n\n" +
            //                                 $"We are pleased to inform you that your task has been moved to {taskRequestDto.Status} on {DateTime.Now.ToString("yyyy-MM-dd")}.\n\n" +
            //                                 "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                                 "Thank you!";

            //            await _emailSender.SendEmailAsync(owner.EmailAddress, taskRequestDto.Subject, emailContent);
            //        }
            //        break;
            //    case TaskTypes.WorkOrderRequest:
            //        if (taskRequestDto.IsNotifyAssignee && taskRequestDto.VendorId != null)
            //        {
            //            var vendor = await _authService.GetVendorByIdAsync(taskRequestDto.VendorId ?? 0);

            //            var emailContent = taskRequestDto.Status == TaskStatusTypes.NotStarted.ToString() ?
            //                                $"Hello {vendor.FirstName} {vendor.LastName},\n\n" +
            //                                "We are pleased to inform you that a new task has been assigned to you. Here are the details:\n\n" +
            //                                $"Task Type: {taskRequestDto.Type}\n" +
            //                                $"Task Description: {taskRequestDto.Description}\n\n" +
            //                                "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                                "Thank you!" :
            //                                $"Hello {vendor.FirstName} {vendor.LastName},\n\n" +
            //                                $"We are pleased to inform you that your task has been moved to {taskRequestDto.Status} on {DateTime.Now.ToString("yyyy-MM-dd")}.\n\n" +
            //                                "If you have any questions or need further assistance, please do not hesitate to contact us.\n\n" +
            //                                "Thank you!";

            //            await _emailSender.SendEmailAsync(vendor.Email1, taskRequestDto.Subject, emailContent);
            //        }
            //        break;
            //    default:
            //        break;
            //}

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
