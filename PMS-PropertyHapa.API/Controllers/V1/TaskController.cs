using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/TaskAuth")]
    [ApiController]
    public class TaskController : Controller
    {

            private readonly IUserRepository _userRepo;
            protected APIResponse _response;
            public TaskController(IUserRepository userRepo, APIResponse response)
            {
                _userRepo = userRepo;
                _response = response;
            }


        //[HttpGet("Tasks")]
        //public async Task<ActionResult<TaskRequestDto>> GetTasks()
        //{
        //    try
        //    {
        //        var assets = await _userRepo.GetTaskRequestsAsync();

        //        if (assets != null)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = true;
        //            _response.Result = assets;
        //            return Ok(_response);
        //        }
        //        else
        //        {
        //            _response.StatusCode = HttpStatusCode.NotFound;
        //            _response.IsSuccess = false;
        //            _response.ErrorMessages.Add("No asset found with this id.");
        //            return NotFound(_response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}

        //[HttpGet("GetTaskById/{id}")]
        //public async Task<IActionResult> GetTaskById(int id)
        //{

        //    try
        //    {
        //        var taskDto = await _userRepo.GetTaskByIdAsync(id);

        //        if (taskDto != null)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = true;
        //            _response.Result = taskDto;
        //            return Ok(_response);
        //        }
        //        else
        //        {
        //            _response.StatusCode = HttpStatusCode.NotFound;
        //            _response.IsSuccess = false;
        //            _response.ErrorMessages.Add("No user found with this id.");
        //            return NotFound(_response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.StatusCode = HttpStatusCode.NotFound;
        //        _response.IsSuccess = false;
        //        _response.ErrorMessages.Add("Error Occured");
        //        return NotFound(_response);
        //    }
        //}

        //[HttpPost("Task")]
        //public async Task<ActionResult<bool>> SaveTaskRequest(TaskRequestDto taskRequestDto)
        //{
        //    try
        //    {
        //        var isSuccess = await _userRepo.SaveTaskAsync(taskRequestDto);
        //        if (isSuccess == true)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = true;
        //            _response.Result = isSuccess;
        //        }
        //        return Ok(_response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}

        //[HttpPost("Task/{id}")]
        //public async Task<ActionResult<bool>> DeleteTaskRequest(int id)
        //{
        //    try
        //    {
        //        var isSuccess = await _userRepo.DeleteTaskAsync(id);
        //        return Ok(isSuccess);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}
    }
}
