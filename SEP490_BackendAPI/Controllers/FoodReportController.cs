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
    public class FoodReportController : ControllerBase
    {
        private readonly IFoodReportService _foodReportService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic báo cáo thức ăn.
        /// </summary>
        public FoodReportController(IFoodReportService foodReportService)
        {
            _foodReportService = foodReportService ?? throw new ArgumentNullException(nameof(foodReportService));
        }

        /// <summary>
        /// Tạo một báo cáo thức ăn mới và trừ lượng còn lại trong LivestockCircleFood.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFoodReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _foodReportService.CreateAsync(requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một báo cáo thức ăn và điều chỉnh lượng còn lại trong LivestockCircleFood.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFoodReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _foodReportService.UpdateAsync(id, requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Xóa mềm một báo cáo thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _foodReportService.DeleteAsync(id, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một báo cáo thức ăn theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (foodReport, errorMessage) = await _foodReportService.GetByIdAsync(id, cancellationToken);
            if (foodReport == null)
                return NotFound(errorMessage ?? "Không tìm thấy báo cáo thức ăn.");
            return Ok(foodReport);
        }

        /// <summary>
        /// Lấy danh sách tất cả báo cáo thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(Guid? foodId = null, Guid? reportId = null, CancellationToken cancellationToken = default)
        {
            var (foodReports, errorMessage) = await _foodReportService.GetAllAsync(foodId, reportId, cancellationToken);
            if (foodReports == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách báo cáo thức ăn.");
            return Ok(foodReports);
        }
    }
}