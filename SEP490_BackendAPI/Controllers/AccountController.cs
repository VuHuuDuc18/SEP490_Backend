
using Infrastructure.Services;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Domain.Helper.Constants;
using Domain.Dto.Request.Account;
using Domain.Dto.Request;

namespace SEP490_BackendAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {


        private readonly IEmailService _mailService;
        private readonly ILogger<AccountController> _logger;
        public readonly IUserService _sv;


        public AccountController(IEmailService mailService, ILogger<AccountController> logger, IUserService sr)

        {
            _mailService = mailService;
            _logger = logger;
            _sv = sr;
        }


        //[HttpPost]
        //public async Task<IActionResult> SendEmail([FromForm] string Email)
        //{
        //    try
        //    {
        //        if (Email == null )
        //        {
        //            _logger.LogError("Invalid mail request. The request or Body is null.");
        //            return BadRequest("Invalid mail request. The Body is required.");
        //        }

        //        string Body = Domain.Extensions.MailBodyGenerate.BodyCreateAccount(Email, "123456");

        //        await _mailService.SendEmailAsync(Email, EmailConstant.EMAILSUBJECTCREATEACCOUNT,Body);
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while sending email.");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        //    }
        //    }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest req)
        {
            var Result = await _sv.CreateAccount(req);
            if (Result)
            {
                throw new Exception("Email đã được đăng ký");
            }
            else
            {
                return Ok(Result);
            }
        }
        [HttpGet("resetPassword/{id}")]
        public async Task<IActionResult> ResetPassword([FromRoute]Guid id)
        {
            
            return Ok( await _sv.ResetPassword(id));
        }
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            return Ok(await _sv.ChangePassword(req));
        }
        [HttpPost("list")]
        public async Task<IActionResult> Listing(ListingRequest req)
        {
            var result = await _sv.GetListAccount(req);

            return Ok(result);
        }
    }
}
