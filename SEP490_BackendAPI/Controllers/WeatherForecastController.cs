using Domain.Dto.Request;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace SEP490_BackendAPI.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    
    public class WeatherForecastController : ControllerBase
    {
        

        private readonly ILogger<WeatherForecastController> _logger;
        public readonly IUserService _sv;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IUserService sr)
        {
            _logger = logger;
            _sv = sr;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest req)
        {
            var Result = await _sv.CreateAccount(req);
            return Ok(Result);
        }
    }
}
