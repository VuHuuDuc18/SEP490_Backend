using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.Account;
using Application.Interfaces;
using Domain.Dto.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Domain.Services.Interfaces;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authServices)
        {
            _authService = authServices;
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(AuthenticationRequest request)
        {
            return Ok(await _authService.LoginAsync(request, GenerateIPAddress()));
        }
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccountAsync(CreateNewAccountRequest request)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString() ?? "https://localhost:7074";
            }
            return Ok(await _authService.CreateAccountAsync(request, origin));
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery]string userId, [FromQuery]string code)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString() ?? "https://localhost:7074";
            }
            return Ok(await _authService.ConfirmEmailAsync(userId, code));
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = Request.Headers["Referer"].ToString() ?? "https://localhost:7074";
            }
            await _authService.ForgotPassword(model, origin);
            return Ok();
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            
            return Ok(await _authService.ResetPassword(model));
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            return Ok(await _authService.RefreshTokenAsync(request.Token, GenerateIPAddress()));
        }
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            return Ok(await _authService.RevokeTokenAsync(request.Token, GenerateIPAddress()));
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
            return Ok(await _authService.DeleteAccount(request.Email));
        }
        [HttpPost("disable-account")]
        public async Task<IActionResult> DisableAccount([FromBody]string email)
        {
            return Ok(await _authService.DisableAccountAsync(email));
        }
        [HttpPost("enable-account")]
        public async Task<IActionResult> EnableAccount([FromBody]string email)
        {
            return Ok(await _authService.EnableAccountAsync(email));
        }
        [HttpGet("get-all-accounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            return Ok(await _authService.GetAllAccountsAsync());
        }
        [HttpPut("update-account")]
        public async Task<IActionResult> UpdateAccount([FromBody]UpdateAccountRequest request)
        {
            return Ok(await _authService.UpdateAccountAsync(request));
        }
        [HttpGet("get-account-by-email")]
        public async Task<IActionResult> GetAccountByEmail([FromQuery]string email)
        {
            return Ok(await _authService.GetAccountByEmailAsync(email));
        }
    }
}