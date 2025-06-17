using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.Interfaces;
using Domain.Dto.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(AuthenticationRequest request)
        {
            return Ok(await _accountService.LoginAsync(request, GenerateIPAddress()));
        }
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccountAsync(CreateNewAccountRequest request)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString() ?? "https://localhost:7074";
            }
            return Ok(await _accountService.CreateAccountAsync(request, origin));
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery]string userId, [FromQuery]string code)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString() ?? "https://localhost:7074";
            }
            return Ok(await _accountService.ConfirmEmailAsync(userId, code));
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString() ?? "https://localhost:7074";
            }
            await _accountService.ForgotPassword(model, origin);
            return Ok();
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            
            return Ok(await _accountService.ResetPassword(model));
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            return Ok(await _accountService.RefreshTokenAsync(request.Token, GenerateIPAddress()));
        }
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            return Ok(await _accountService.RevokeTokenAsync(request.Token, GenerateIPAddress()));
        }
        private string GenerateIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody]DeleteAccountRequest request)
        {
            return Ok(await _accountService.DeleteAccount(request.Email));
        }
    }
}