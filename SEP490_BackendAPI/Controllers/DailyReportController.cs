using Domain.Dto.Request;
using Domain.Dto.Request.DailyReport;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailyReportController : ControllerBase
    {
        private readonly IDailyReportService _dailyReportService;

        public DailyReportController(IDailyReportService dailyReportService)
        {
            _dailyReportService = dailyReportService ?? throw new ArgumentNullException(nameof(dailyReportService));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.CreateAsync(requestDto, cancellationToken);
            return success ? Ok() : BadRequest(errorMessage);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.UpdateAsync(id, requestDto, cancellationToken);
            return success ? Ok() : BadRequest(errorMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.DeleteAsync(id, cancellationToken);
            return success ? Ok() : BadRequest(errorMessage);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (dailyReport, errorMessage) = await _dailyReportService.GetByIdAsync(id, cancellationToken);
            return dailyReport != null ? Ok(dailyReport) : NotFound(errorMessage ?? "Không tìm thấy báo cáo hàng ngày.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid? livestockCircleId = null, CancellationToken cancellationToken = default)
        {
            var (dailyReports, errorMessage) = await _dailyReportService.GetAllAsync(livestockCircleId, cancellationToken);
            return dailyReports != null ? Ok(dailyReports) : NotFound(errorMessage ?? "Không tìm thấy danh sách báo cáo hàng ngày.");
        }

        [HttpPost("{id}/food-details")]
        public async Task<IActionResult> GetFoodReportDetails(Guid id, [FromBody] ListingRequest request)
        {
            var (foodReports, errorMessage) = await _dailyReportService.GetFoodReportDetailsAsync(id, request);
            return foodReports != null ? Ok(foodReports) : NotFound(errorMessage ?? "Không tìm thấy chi tiết báo cáo thức ăn.");
        }

        [HttpPost("{id}/medicine-details")]
        public async Task<IActionResult> GetMedicineReportDetails(Guid id, [FromBody] ListingRequest request)
        {
            var (medicineReports, errorMessage) = await _dailyReportService.GetMedicineReportDetailsAsync(id, request);
            return medicineReports != null ? Ok(medicineReports) : NotFound(errorMessage ?? "Không tìm thấy chi tiết báo cáo thuốc.");
        }

        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("daily_reports/paginated")]
        public async Task<IActionResult> GetPaginatedMedicines([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _dailyReportService.GetPaginatedListAsync(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }

    }
}