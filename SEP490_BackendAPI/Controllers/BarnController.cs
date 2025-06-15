using Domain.Dto.Request;
using Domain.Dto.Response;
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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBarnRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _barnService.CreateAsync(requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một chuồng trại.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBarnRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _barnService.UpdateAsync(id, requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Xóa mềm một chuồng trại bằng cách đặt IsActive thành false.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _barnService.DeleteAsync(id, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một chuồng trại theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (barn, errorMessage) = await _barnService.GetByIdAsync(id, cancellationToken);
            if (barn == null)
                return NotFound(errorMessage ?? "Không tìm thấy chuồng trại.");
            return Ok(barn);
        }

        /// <summary>
        /// Lấy danh sách tất cả chuồng trại đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(string barnName = null, Guid? workerId = null, CancellationToken cancellationToken = default)
        {
            var (barns, errorMessage) = await _barnService.GetAllAsync(barnName, workerId, cancellationToken);
            if (barns == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách chuồng trại.");
            return Ok(barns);
        }

        /// <summary>
        /// Lấy danh sách chuồng trại theo ID của công nhân.
        /// </summary>
        [HttpGet("worker/{workerId}")]
        public async Task<IActionResult> GetByWorker(Guid workerId, CancellationToken cancellationToken = default)
        {
            var (barns, errorMessage) = await _barnService.GetByWorkerAsync(workerId, cancellationToken);
            if (barns == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách chuồng trại theo công nhân.");
            return Ok(barns);
        }
    }
}