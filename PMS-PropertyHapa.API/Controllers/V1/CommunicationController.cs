using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using PMS_PropertyHapa.Shared.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Web;
using System.Security.Claims;
using System.Net.Http.Headers;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/CommunicationAuth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class CommunicationController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;

        public CommunicationController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _response = new();
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


        #region CommunicationCrud 



        [HttpGet("Communication")]
        public async Task<ActionResult<CommunicationDto>> GetAllCommunication()
        {
            try
            {
                var Communication = await _userRepo.GetAllCommunicationAsync();

                if (Communication != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = Communication;
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


        [HttpPost("Communication")]
        public async Task<ActionResult<bool>> CreateCommunication(CommunicationDto communication)
        {
            try
            {
                var isSuccess = await _userRepo.CreateCommunicationAsync(communication);

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

        [HttpPut("Communication/{communicationId}")]
        public async Task<ActionResult<bool>> UpdateCommunication(int communicationId, CommunicationDto communication)
        {
            try
            {
                communication.Communication_Id = communicationId; 
                var isSuccess = await _userRepo.UpdateCommunicationAsync(communication);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("Communication/{communicationId}")]
        public async Task<ActionResult<bool>> DeleteCommunication(int communicationId)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteCommunicationAsync(communicationId);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost("updateAccount")]
        public async Task<ActionResult<bool>> UpdateAccount(TiwiloDto obj)
        {
            try
            {
                var isSuccess = await _userRepo.UpdateAccountAsync(obj);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        #endregion

    }
}