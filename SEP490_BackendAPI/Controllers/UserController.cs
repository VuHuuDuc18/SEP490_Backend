﻿using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Domain.Dto.Request.User;
using Domain.IServices;
using Entities.EntityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userServices;
        public UserController(IUserService userServices)
        {
            _userServices = userServices;
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(AuthenticationRequest request)
        {
            return Ok(await _userServices.LoginAsync(request, GenerateIPAddress()));
        }

        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccountAsync(CreateNewAccountRequest request)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString();
            }
            return Ok(await _userServices.CreateCustomerAccountAsync(request, origin));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery]string userId, [FromQuery]string code)
        {
            return Ok(await _userServices.ConfirmEmailAsync(userId, code));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString();
            }
            return Ok(await _userServices.ForgotPassword(model, origin));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            
            return Ok(await _userServices.ResetPassword(model));
        }

        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
        {
            return Ok(await _userServices.ChangePassword(model));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            return Ok(await _userServices.RefreshTokenAsync(request.Token, GenerateIPAddress()));
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            return Ok(await _userServices.RevokeTokenAsync(request.Token, GenerateIPAddress()));
        }

        [HttpPatch("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody]UserUpdateAccountRequest request)
        {
            return Ok(await _userServices.UpdateAccountAsync(request));
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            return Ok(await _userServices.GetUserProfile());
        }

        [HttpPost("resend-verify-email")]
        public async Task<IActionResult> ResendVerifyEmail([FromQuery] string email)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString();
            }
            return Ok(await _userServices.ResendVerifyEmailAsync(email,origin));
        }

        private string GenerateIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}