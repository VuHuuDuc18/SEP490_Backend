using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DBContext;
using Infrastructure.Identity.Contexts;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly LCFMSDBContext _mainContext;
        private readonly IdentityContext _identityContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            LCFMSDBContext mainContext, 
            IdentityContext identityContext,
            ILogger<HealthController> logger)
        {
            _mainContext = mainContext;
            _identityContext = identityContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check main database connection
                await _mainContext.Database.CanConnectAsync();
                
                // Check identity database connection  
                await _identityContext.Database.CanConnectAsync();

                return Ok(new { 
                    status = "healthy", 
                    timestamp = DateTime.UtcNow,
                    databases = new {
                        main = "connected",
                        identity = "connected"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new { 
                    status = "unhealthy", 
                    error = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }
    }
} 