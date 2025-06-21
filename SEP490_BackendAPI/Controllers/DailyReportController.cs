using Domain.Dto.Request;
using Domain.Dto.Request.DailyReport;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/daily_reports")]
    public class DailyReportController : ControllerBase
    {
        private readonly IDailyReportService _dailyReportService;

        public DailyReportController(IDailyReportService dailyReportService)
        {
            _dailyReportService = dailyReportService ?? throw new ArgumentNullException(nameof(dailyReportService));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDailyReport([FromBody] CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.CreateDailyReport(requestDto, cancellationToken);
            return success ? Ok() : BadRequest(errorMessage);
        }

        [HttpPut("update/{dailyReportId}")]
        public async Task<IActionResult> Update(Guid dailyReportId, [FromBody] UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.UpdateDailyReport(dailyReportId, requestDto, cancellationToken);
            return success ? Ok() : BadRequest(errorMessage);
        }

        [HttpDelete("disable/{dailyReportId}")]
        public async Task<IActionResult> Delete(Guid dailyReportId, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _dailyReportService.DisableDailyReport(dailyReportId, cancellationToken);
            return success ? Ok() : BadRequest(errorMessage);
        }

        [HttpGet("getDailyReportById/{dailyReportId}")]
        public async Task<IActionResult> GetDailyReportById(Guid dailyReportId, CancellationToken cancellationToken = default)
        {
            var (dailyReport, errorMessage) = await _dailyReportService.GetDailyReportById(dailyReportId,cancellationToken);
            return dailyReport != null ? Ok(dailyReport) : NotFound(errorMessage ?? "Không tìm thấy báo cáo hàng ngày.");
        }

        [HttpGet("getDailyReportByLiveStockCircle")]
        public async Task<IActionResult> GetDailyReportByLiveStockCircle(Guid? livestockCircleId = null, CancellationToken cancellationToken = default)
        {
            var (dailyReports, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, cancellationToken);
            return dailyReports != null ? Ok(dailyReports) : NotFound(errorMessage ?? "Không tìm thấy danh sách báo cáo hàng ngày.");
        }

        [HttpPost("{dailyReportId}/food-details")]
        public async Task<IActionResult> GetFoodReportDetails(Guid id, [FromBody] ListingRequest request)
        {
            var (foodReports, errorMessage) = await _dailyReportService.GetFoodReportDetails(id, request);
            return foodReports != null ? Ok(foodReports) : NotFound(errorMessage ?? "Không tìm thấy chi tiết báo cáo thức ăn.");
        }

        [HttpPost("{dailyReportId}/medicine-details")]
        public async Task<IActionResult> GetMedicineReportDetails(Guid id, [FromBody] ListingRequest request)
        {
            var (medicineReports, errorMessage) = await _dailyReportService.GetMedicineReportDetails(id, request);
            return medicineReports != null ? Ok(medicineReports) : NotFound(errorMessage ?? "Không tìm thấy chi tiết báo cáo thuốc.");
        }

        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("getPaginatedDailyReportList")]
        public async Task<IActionResult> GetPaginatedMedicines([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _dailyReportService.GetPaginatedDailyReportList(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }

        [HttpGet("today-daily-report/{livestockCircleId}")]
        public async Task<IActionResult> GetTodayDailyReport(Guid livestockCircleId)
        {
            var (dailyReports, errorMessage) = await _dailyReportService.GetTodayDailyReport(livestockCircleId);
            return dailyReports != null ? Ok(dailyReports) : NotFound(errorMessage ?? "Không tìm thấy báo cáo hôm nay.");
        }

    }
}