
using Infrastructure.Services;
using Domain.Dto.Request;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Domain.Helper.Constants;

namespace SEP490_BackendAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {


        private readonly IEmailService _mailService;
        private readonly ILogger<AccountController> _logger;
        public readonly IUserService _sv;

        public AccountController(IEmailService mailService, ILogger<AccountController> logger)
        {
            _mailService = mailService;
            _logger = logger;
          //  _sv = sr;
        }


        [HttpPost]
        public async Task<IActionResult> SendEmail([FromForm] string Email)
        {
            try
            {
                if (Email == null )
                {
                    _logger.LogError("Invalid mail request. The request or Body is null.");
                    return BadRequest("Invalid mail request. The Body is required.");
                }

                string Body = Domain.Extensions.MailBodyGenerate.BodyCreateAccount(Email, "123456");

                await _mailService.SendEmailAsync(Email, EmailConstant.EMAILSUBJECTCREATEACCOUNT,Body);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
            }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest req)
        {
            var Result = await _sv.CreateAccount(req);
            return Ok(Result);
        }
    }
}
