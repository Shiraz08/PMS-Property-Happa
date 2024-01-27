﻿using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using PMS_PropertyHapa.API.Areas.Identity.Data;
using PMS_PropertyHapa.Shared.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Web;

namespace PMS_PropertyHapa.API.Controllers
{
    [Route("api/v1/UsersAuth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        protected APIResponse _response;

        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _response = new();
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var tokenDto = await _userRepo.Login(model);
            if (tokenDto == null || string.IsNullOrEmpty(tokenDto.AccessToken))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = tokenDto;
            return Ok(_response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            bool ifUserNameUnique = _userRepo.IsUniqueUser(model.UserName);
            if (!ifUserNameUnique)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            var user = await _userRepo.Register(model);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while registering");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDTOResponse = await _userRepo.RefreshAccessToken(tokenDTO);
                if (tokenDTOResponse == null || string.IsNullOrEmpty(tokenDTOResponse.AccessToken))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Token Invalid");
                    return BadRequest(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = tokenDTOResponse;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.Result = "Invalid Input";
                return BadRequest(_response);
            }

        }


        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
        {

            if (ModelState.IsValid)
            {
                await _userRepo.RevokeRefreshToken(tokenDTO);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            _response.IsSuccess = false;
            _response.Result = "Invalid Input";
            return BadRequest(_response);
        }



        #region Registeration 

        [HttpPost("register/tenant")]
        public async Task<IActionResult> RegisterTenant([FromBody] RegisterationRequestDTO model)
        {
            var user = await _userRepo.RegisterTenant(model);

            if (user == null || user.userID == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while registering tenant");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = user;
            return Ok(_response);
        }




        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterationRequestDTO model)
        {
            try
            {


                var user = await _userRepo.RegisterAdmin(model);
                if (user == null || user.userID == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Error while registering admin");
                    return BadRequest(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = user;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost("register/user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterationRequestDTO model)
        {

            var user = await _userRepo.RegisterUser(model);
            if (user == null || user.userID == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while registering user");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = user;
            return Ok(_response);
        }



        #endregion





        #region ChangePassword, ForgetPassword, Reset Password

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto model)
        {
            if (!await _userRepo.ValidateCurrentPassword(model.userId, model.currentPassword))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Current password is incorrect");
                return BadRequest(_response);
            }

            if (model.newPassword != model.newRepeatPassword)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("New password and confirmation password do not match");
                return BadRequest(_response);
            }
            if (!await _userRepo.ChangePassword(model.userId, model.currentPassword, model.newPassword))
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while changing password");
                return StatusCode(500, _response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = "Password changed successfully";
            return Ok(_response);
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userRepo.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Invalid request.");
            }
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("The password and confirmation password do not match.");
            }
            var result = await _userRepo.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = "Password has been reset successfully";
            return Ok(_response);
        }



        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgetPassword model)
        {
            var user = await _userRepo.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }

            var token = await _userRepo.GeneratePasswordResetTokenAsync(user);
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";
            var callbackUrl = $"{baseUrl}/reset-password?token={HttpUtility.UrlEncode(token)}";
            await _userRepo.SendResetPasswordEmailAsync(user, callbackUrl);

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = "Reset password email sent successfully";
            return Ok(_response);

        }
        #endregion
    }
}