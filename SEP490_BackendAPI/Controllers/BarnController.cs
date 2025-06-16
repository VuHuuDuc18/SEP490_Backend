using Domain.Services.Implements;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarnController : ControllerBase
    {
        private readonly ILogger<BarnController> logger;    
        private readonly IBarnService barnService;

        public BarnController(ILogger<BarnController> logger, IBarnService barnService)
        {
            this.logger = logger;
            this.barnService = barnService;
        }

        [HttpPost("getBarnById/{id}")]
        public async Task<IActionResult> getById([FromRoute]Guid id)
        {
            var result = await barnService.GetByIdAsync(id);
            if (result.Barn == null)
            {
                return NotFound(result.ErrorMessage);
            }
            return Ok(result.Barn);
        }
    }
}
