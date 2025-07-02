using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.Account;
using Application.Interfaces;
using Domain.Dto.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Domain.Dto.Request.User;

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
       
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            
            return Ok(await _accountService.ResetPassword(model));
        }
        
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody]DeleteAccountRequest request)
        {
            return Ok(await _accountService.DeleteAccount(request.Email));
        }
        [HttpPost("disable-account")]
        public async Task<IActionResult> DisableAccount([FromBody]string email)
        {
            return Ok(await _accountService.DisableAccountAsync(email));
        }
        [HttpPost("enable-account")]
        public async Task<IActionResult> EnableAccount([FromBody]string email)
        {
            return Ok(await _accountService.EnableAccountAsync(email));
        }
        [HttpGet("get-all-accounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            return Ok(await _accountService.GetAllAccountsAsync());
        }
        [HttpPut("update-account")]
        public async Task<IActionResult> UpdateAccount([FromBody]UpdateAccountRequest request)
        {
            return Ok(await _accountService.UpdateAccountAsync(request));
        }
        [HttpGet("get-account-by-email")]
        public async Task<IActionResult> GetAccountByEmail([FromQuery]string email)
        {
            return Ok(await _accountService.GetAccountByEmailAsync(email));
        }
    }
}