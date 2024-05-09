using MagicVilla_VillaAPI.Repository.IRepostiory;
using PMS_PropertyHapa.Models;

namespace PMS_PropertyHapa.API.Controllers.V2
{
    public class TaskController
    {

        private readonly IUserRepository _userRepo;
        protected APIResponse _response;
        public TaskController(IUserRepository userRepo, APIResponse response)
        {
            _userRepo = userRepo;
            _response = response;
        }


        //[HttpGet("Tasks")]
        //    public async Task<ActionResult<TaskRequestDto>> GetAllTasks()
        //    {
        //        try
        //        {
        //            var assets = await _userRepo.GetTaskRequestsAsync();

        //            if (assets != null)
        //            {
        //                _response.StatusCode = HttpStatusCode.OK;
        //                _response.IsSuccess = true;
        //                _response.Result = assets;
        //                return Ok(_response);
        //            }
        //            else
        //            {
        //                _response.StatusCode = HttpStatusCode.NotFound;
        //                _response.IsSuccess = false;
        //                _response.ErrorMessages.Add("No asset found with this id.");
        //                return NotFound(_response);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, $"An error occurred: {ex.Message}");
        //        }
        //    }
    }
}
