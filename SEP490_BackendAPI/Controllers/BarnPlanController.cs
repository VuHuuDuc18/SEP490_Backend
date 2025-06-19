using Domain.Dto.Request.BarnPlan;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarnPlanController : ControllerBase
    {
        private readonly IBarnPlanService _barnPlanService;

        public BarnPlanController(IBarnPlanService barnPlanService)
        {
            _barnPlanService = barnPlanService;
        }
        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> getById([FromQuery]Guid id)
        {
            var result = await _barnPlanService.GetById(id);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> createBarnPlan([FromBody]CreateBarnPlanRequest req)
        {
            var result = await _barnPlanService.CreateBarnPlan(req);
            return Ok(result);
        }
    }
}
