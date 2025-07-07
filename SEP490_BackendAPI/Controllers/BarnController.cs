using Domain.Dto.Request;
using Domain.Dto.Request.Barn;
using Domain.Services.Interfaces;
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

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic chuồng trại.
        /// </summary>
        public BarnController(IBarnService barnService)
        {
            _barnService = barnService ?? throw new ArgumentNullException(nameof(barnService));
        }

        /// <summary>
        /// Tạo một chuồng trại mới.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateBarn([FromBody] CreateBarnRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _barnService.CreateBarn(requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một chuồng trại.
        /// </summary>
        [HttpPut("update/{BarnId}")]
        public async Task<IActionResult> Update(Guid BarnId, [FromBody] UpdateBarnRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _barnService.UpdateBarn(BarnId, requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Xóa mềm một chuồng trại bằng cách đặt IsActive thành false.
        /// </summary>
        [HttpDelete("disable/{BarnId}")]
        public async Task<IActionResult> DisableBarn(Guid BarnId, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _barnService.DisableBarn(BarnId, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một chuồng trại theo ID.
        /// </summary>
        [HttpGet("getBarnById/{BarnId}")]
        public async Task<IActionResult> GetBarnById(Guid BarnId, CancellationToken cancellationToken = default)
        {
            var (barn, errorMessage) = await _barnService.GetBarnById(BarnId, cancellationToken);
            if (barn == null)
                return NotFound(errorMessage ?? "Không tìm thấy chuồng trại.");
            return Ok(barn);
        }

        /// <summary>
        /// Lấy danh sách chuồng trại theo ID của công nhân.
        /// </summary>
        [HttpGet("getBarnByWorker/{workerId}")]
        public async Task<IActionResult> GetByWorker(Guid workerId, CancellationToken cancellationToken = default)
        {
            var (barns, errorMessage) = await _barnService.GetBarnByWorker(workerId, cancellationToken);
            if (barns == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách chuồng trại theo người gia công.");
            return Ok(barns);
        }

        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("getPaginatedBarnList")]
        public async Task<IActionResult> GetPaginatedBarns([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _barnService.GetPaginatedBarnList(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách chuồng trại phân trang cho admin, bao gồm trạng thái có LivestockCircle đang hoạt động.
        /// </summary>
        [HttpPost("getPaginatedBarnListAdmin")]
        public async Task<IActionResult> GetPaginatedAdminBarnList([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var (result, errorMessage) = await _barnService.GetPaginatedAdminBarnListAsync(request, cancellationToken);
                if (result == null)
                    return BadRequest(new { Message = errorMessage });

                return Ok(new
                {
                    Data = result.Items,
                    PageIndex = result.PageIndex,
                    Count = result.Count,
                    TotalCount = result.TotalCount,
                    TotalPages = result.TotalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi lấy danh sách chuồng trại: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy chi tiết chuồng trại cho admin, bao gồm thông tin LivestockCircle đang hoạt động (nếu có).
        /// </summary>
        [HttpGet("getBarnDetailAdmin/{barnId}")]
        public async Task<IActionResult> GetAdminBarnDetail(Guid barnId, CancellationToken cancellationToken = default)
        {
            try
            {
                var (barn, errorMessage) = await _barnService.GetAdminBarnDetailAsync(barnId, cancellationToken);
                if (barn == null)
                    return NotFound(new { Message = errorMessage });

                return Ok(barn);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi lấy chi tiết chuồng trại: {ex.Message}" });
            }
        }

        [HttpPost("customer/getReleaseBarnList")]
        public async Task<IActionResult> GetReleaseBarnList([FromBody]ListingRequest request, CancellationToken cancellationToken = default)
        {
            return Ok(await _barnService.GetPaginatedReleaseBarnListAsync(request, cancellationToken));
        }

    }
}

