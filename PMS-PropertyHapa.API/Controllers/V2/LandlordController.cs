using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.ViewModels;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;

namespace PMS_PropertyHapa.API.Controllers.V2
{
    public class LandlordController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public LandlordController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
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


        #region Landlord Crud 

        [HttpGet("Tasks")]
        public async Task<ActionResult<TaskRequestDto>> GetTasks()
        {
            try
            {
                var tasks = await _userRepo.GetTaskRequestsAsync();

                if (tasks != null && tasks.Any())
                {
                    return Ok(tasks);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "No tasks found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "No tasks found",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
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
                    return Ok(taskDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"No task found with id: {id}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No task found with id: {id}",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("Task")]
        public async Task<ActionResult<bool>> SaveTaskRequest(TaskRequestDto taskRequestDto)
        {
            try
            {
                var isSuccess = await _userRepo.SaveTaskAsync(taskRequestDto);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "Failed to save task request.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to save task request",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
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

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "Failed to delete task request.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to delete task request",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("Landlord")]
        public async Task<ActionResult<OwnerDto>> GetAllLandlord()
        {
            try
            {
                var landlords = await _userRepo.GetAllLandlordAsync();

                if (landlords != null && landlords.Any())
                {
                    return Ok(landlords);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "No landlords found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "No landlords found",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetSingleLandlord/{ownerId}")]
        public async Task<IActionResult> GetSingleLandlord(int ownerId)
        {
            try
            {
                var landlordDto = await _userRepo.GetSingleLandlordByIdAsync(ownerId);

                if (landlordDto != null)
                {
                    return Ok(landlordDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"No landlord found with id: {ownerId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No landlord found with id: {ownerId}",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("Landlord")]
        public async Task<ActionResult<bool>> CreateOwner(OwnerDto owner)
        {
            try
            {
                var isSuccess = await _userRepo.CreateOwnerAsync(owner);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "Failed to create owner.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to create owner",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPut("Landlord/{OwnerId}")]
        public async Task<ActionResult<bool>> UpdateOwner(int OwnerId, OwnerDto owner)
        {
            try
            {
                owner.OwnerId = OwnerId;
                var isSuccess = await _userRepo.UpdateOwnerAsync(owner);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "Failed to update owner.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to update owner",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("Landlord/{ownerId}")]
        public async Task<ActionResult<bool>> DeleteOwner(int ownerId)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteOwnerAsync(ownerId.ToString());

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "Failed to delete owner.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to delete owner",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
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
