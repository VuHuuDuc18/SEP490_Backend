using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace SEP490_BackendAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IEmailService _mailService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IEmailService mailService, ILogger<WeatherForecastController> logger)
        {
            _mailService = mailService;
            _logger = logger;
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

                await _mailService.SendAsync(Email);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
