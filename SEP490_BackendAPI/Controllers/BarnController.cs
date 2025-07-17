using Domain.Dto.Request;
using Domain.Dto.Request.Barn;
using Domain.Dto.Request.Category;
using Domain.IServices;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BarnController : ControllerBase
    {
        private readonly IBarnService _barnService;
        public BarnController(IBarnService barnService)
        {
            _barnService = barnService;
        }
        [HttpPost("admin/get-barn-list")]
        public async Task<IActionResult> GetPaginatedAdminBarnList([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _barnService.GetPaginatedAdminBarnListAsync(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("admin/get-barn-detail/{barnId}")]
        public async Task<IActionResult> GetAdminBarnDetail([FromRoute] Guid barnId, CancellationToken cancellationToken = default)
        {
            var response = await _barnService.GetAdminBarnDetailAsync(barnId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        [HttpPost("admin/create-barn")]
        public async Task<IActionResult> CreateBarn([FromBody] CreateBarnRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _barnService.CreateBarn(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("admin/update-barn")]
        public async Task<IActionResult> UpdateBarn([FromBody] UpdateBarnRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _barnService.UpdateBarn(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        [HttpDelete("admin/disable-barn/{BarnId}")]
        public async Task<IActionResult> DisableBarn([FromRoute] Guid BarnId, CancellationToken cancellationToken = default)
        {
            var response = await _barnService.DisableBarn(BarnId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("getBarnById/{BarnId}")]
        public async Task<IActionResult> GetBarnById([FromRoute] Guid BarnId, CancellationToken cancellationToken = default)
        {
            var response = await _barnService.GetBarnById(BarnId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("woker/get-barn-by-worker")]
        public async Task<IActionResult> GetByWorker([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _barnService.GetBarnByWorker(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("get-barn-list")]
        public async Task<IActionResult> GetPaginatedBarns([FromBody] ListingRequest request)
        {
            var response = await _barnService.GetPaginatedBarnList(request);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("technical-staff/assignedbarn")]
        public async Task<IActionResult> GetAssignedBarn([FromBody] ListingRequest req)
        {
            //Guid technicalStaffId;
            try
            {
                Guid.TryParse(User.FindFirst("uid")?.Value, out Guid technicalStaffId);
                var result = await _barnService.GetAssignedBarn(technicalStaffId, req);
                if (result.Data == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpPost("customer/getReleaseBarnList")]
        public async Task<IActionResult> GetReleaseBarnList([FromBody]ListingRequest request, CancellationToken cancellationToken = default)
        {
            return Ok(await _barnService.GetPaginatedReleaseBarnListAsync(request, cancellationToken));
        }

        [HttpGet("customer/getReleaseBarnDetail/{BarnId}")]
        public async Task<IActionResult> getReleaseBarnDetail([FromRoute] Guid BarnId, CancellationToken cancellationToken = default)
        {
            return Ok(await _barnService.GetReleaseBarnDetail(BarnId, cancellationToken));
        }

    }
}

