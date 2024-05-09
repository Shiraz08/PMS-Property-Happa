using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.ViewModels;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.API.Controllers.V2
{
    public class UserRegisterationController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRegisterationController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _userManager = userManager;
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(UserRegisterationDto model)
        {
            var response = await _userRepo.RegisterUserData(model);
            if (!response)
            {
                var errorResponse = new ApiResponseUser
                {
                    HasErrors = true,
                    IsValid = false,
                    TextInfo = "Error occurred while registering user",
                    Result = null,
                    Messages = new[]
                    {
                        new Messages
                        {
                            TypeDescription = MessageType.Error,
                            Message = "Error occurred while registering user",
                            Title = "Bad Request"
                        }
                    }
                };
                return BadRequest(errorResponse);
            }
            return Ok();
        }

        [HttpPost("verify-email/{email}")]
        public async Task<IActionResult> VerifyEmail(string email)
        {
            var user = await _userRepo.FindByEmailAsync(email);
            if (user != null)
            {
                return Ok();
            }
            return NotFound("Email not found.");
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
                var errorResponse = new ApiResponseUser
                {
                    HasErrors = true,
                    IsValid = false,
                    TextInfo = "Invalid OTP.",
                    Result = null,
                    Messages = new[]
                    {
                        new Messages
                        {
                            TypeDescription = MessageType.Error,
                            Message = "Invalid OTP.",
                            Title = "Bad Request"
                        }
                    }
                };
                return BadRequest(errorResponse);
            }
            return Ok();
        }

        [HttpPost("verify-phone/{phoneNumber}")]
        public async Task<IActionResult> VerifyPhone(string phoneNumber)
        {
            var user = await _userRepo.FindByPhoneNumberAsync(phoneNumber);
            if (!user)
            {
                var errorResponse = new ApiResponseUser
                {
                    HasErrors = true,
                    IsValid = false,
                    TextInfo = "Phone Number not found.",
                    Result = null,
                    Messages = new[]
                    {
                        new Messages
                        {
                            TypeDescription = MessageType.Error,
                            Message = "Phone Number not found.",
                            Title = "Not Found"
                        }
                    }
                };
                return NotFound(errorResponse);
            }
            return Ok();
        }

        [HttpPost("verify-phone-otp")]
        public async Task<IActionResult> VerifyPhoneOtp(OTPDto oTPDto)
        {
            var result = await _userRepo.VerifyPhoneOtpAsync(oTPDto);
            if (!result)
            {
                var errorResponse = new ApiResponseUser
                {
                    HasErrors = true,
                    IsValid = false,
                    TextInfo = "Invalid OTP.",
                    Result = null,
                    Messages = new[]
                    {
                        new Messages
                        {
                            TypeDescription = MessageType.Error,
                            Message = "Invalid OTP.",
                            Title = "Bad Request"
                        }
                    }
                };
                return BadRequest(errorResponse);
            }
            return Ok();
        }
    }
}