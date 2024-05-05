using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.Net;
using Twilio.Http;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class TaskController : Controller
    {
        private readonly IAuthService _authService;

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

            taskRequestDto.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveTaskAsync(taskRequestDto);
            return Json(new { success = true, message = "Task added successfully" });
        }

        [HttpDelete]
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
