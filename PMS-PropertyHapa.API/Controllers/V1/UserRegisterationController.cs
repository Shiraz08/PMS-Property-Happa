using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Web;
using System.Security.Claims;
using System.Net.Http.Headers;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/UserRegisterationAuth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class UserRegisterationController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;

        public UserRegisterationController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
        }

        
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(UserRegisterationDto model)
        {

            var response = await _userRepo.RegisterUserData(model);
            if (response == false)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error occured while registering user");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }



        [HttpPost("verify-email/{email}")]
        public async Task<IActionResult> VerifyEmail(string email)
        {
            var user = await _userRepo.FindByEmailAsync(email);
            if (user != null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }

            _response.StatusCode = HttpStatusCode.NotFound;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Email not found.");
            return NotFound(_response);
        }

        [HttpPost("SavePhoneOTP")]
        public async Task<IActionResult> SavePhoneOTP(OTPDto otpModel)
        {
            try
            {
                var isSuccess = await _userRepo.SavePhoneOTP(otpModel);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("SaveEmailOTP")]
        public async Task<IActionResult> SaveEmailOTP(OTPDto otpModel)
        {
            try
            {
                var isSuccess = await _userRepo.SaveEamilOTP(otpModel);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpPost("verify-email-otp")]
        public async Task<IActionResult> VerifyEmailOtp(OTPDto oTPEmailDto)
        {
            var result = await _userRepo.VerifyEmailOtpAsync(oTPEmailDto);
            if (!result)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid OTP.");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

        [HttpPost("verify-phone/{phoneNumber}")]
        public async Task<IActionResult> VerifyPhone(string phoneNumber)
        {
            var user = await _userRepo.FindByPhoneNumberAsync(phoneNumber);
            if (user == false)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Phone Number not found.");
                return NotFound(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

        [HttpPost("verify-phone-otp")]
        public async Task<IActionResult> VerifyPhoneOtp(OTPDto oTPDto)
        {
            var result = await _userRepo.VerifyPhoneOtpAsync(oTPDto);
            if (!result)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid OTP.");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

    }
}