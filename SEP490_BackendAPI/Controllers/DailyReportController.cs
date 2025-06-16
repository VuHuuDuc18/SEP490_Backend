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
    public class DailyReportController : ControllerBase
    {
        private readonly IDailyReportService _dailyReportService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic báo cáo hàng ngày.
        /// </summary>
        public DailyReportController(IDailyReportService dailyReportService)
        {
            _dailyReportService = dailyReportService ?? throw new ArgumentNullException(nameof(dailyReportService));
        }

        /// <summary>
        /// Tạo một báo cáo hàng ngày mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDailyReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.CreateAsync(requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một báo cáo hàng ngày.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDailyReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.UpdateAsync(id, requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Xóa mềm một báo cáo hàng ngày bằng cách đặt IsActive thành false.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.DeleteAsync(id, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một báo cáo hàng ngày theo ID, bao gồm danh sách FoodReport và MedicineReport.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (dailyReport, errorMessage) = await _dailyReportService.GetByIdAsync(id, cancellationToken);
            if (dailyReport == null)
                return NotFound(errorMessage ?? "Không tìm thấy báo cáo hàng ngày.");
            return Ok(dailyReport);
        }

        /// <summary>
        /// Lấy danh sách tất cả báo cáo hàng ngày đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách FoodReport và MedicineReport.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(Guid? livestockCircleId = null, CancellationToken cancellationToken = default)
        {
            var (dailyReports, errorMessage) = await _dailyReportService.GetAllAsync(livestockCircleId, cancellationToken);
            if (dailyReports == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách báo cáo hàng ngày.");
            return Ok(dailyReports);
        }
    }
}