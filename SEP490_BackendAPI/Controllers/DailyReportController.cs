using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.DailyReport;
using Domain.Dto.Response.Food;
using Domain.Dto.Response.Medicine;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Services.Implements;
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

        [HttpPost("worker/create-daily-report")]
        public async Task<IActionResult> CreateDailyReport([FromBody] CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            var result = await _dailyReportService.CreateDailyReport(requestDto, cancellationToken);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result);
        }

        [HttpPut("worker/update-daily-report")]
        public async Task<IActionResult> Update([FromBody] UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            var result = await _dailyReportService.UpdateDailyReport(requestDto, cancellationToken);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result);
        }

        [HttpDelete("worker/disable-daily-report/{dailyReportId}")]
        public async Task<IActionResult> Delete([FromRoute] Guid dailyReportId, CancellationToken cancellationToken = default)
        {
            var result = await _dailyReportService.DisableDailyReport(dailyReportId, cancellationToken);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result);
        }

        [HttpGet("worker/get-all-remaining-food-by-livestockcircle")]
        public async Task<IActionResult> GetAllFood(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var foods = await _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId,cancellationToken);
                return Ok(new Response<List<FoodResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy tất cả thức ăn thành công",
                    Data = foods
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<List<FoodResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách thức ăn",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("worker/get-all-remaining-medicine-by-livestockcircle")]
        public async Task<IActionResult> GetAllMedicine(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var medicines = await _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, cancellationToken);
                return Ok(new Response<List<MedicineResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy tất cả thức ăn thành công",
                    Data = medicines
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<List<MedicineResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách thức ăn",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("get-daily-report-by-id/{dailyReportId}")]
        public async Task<IActionResult> GetDailyReportById([FromRoute] Guid dailyReportId, CancellationToken cancellationToken = default)
        {
            var result = await _dailyReportService.GetDailyReportById(dailyReportId, cancellationToken);
            return result.Succeeded ? Ok(result.Data) : NotFound(result);
        }

        //[HttpGet("getDailyReportByLiveStockCircle/{livestockCircleId}")]
        //public async Task<IActionResult> GetDailyReportByLiveStockCircle([FromRoute] Guid livestockCircleId, CancellationToken cancellationToken = default)
        //{
        //    var result = await _dailyReportService.Get(livestockCircleId, cancellationToken);
        //    return result.Succeeded ? Ok(result.Data) : NotFound(result);
        //}

        [HttpPost("food-details/{dailyReportId}")]
        public async Task<IActionResult> GetFoodReportDetails([FromRoute] Guid dailyReportId, [FromBody] ListingRequest request)
        {
            var result = await _dailyReportService.GetFoodReportDetails(dailyReportId, request);
            return result.Succeeded ? Ok(result.Data) : NotFound(result);
        }

        [HttpPost("medicine-details/{dailyReportId}")]
        public async Task<IActionResult> GetMedicineReportDetails([FromRoute] Guid dailyReportId, [FromBody] ListingRequest request)
        {
            var result = await _dailyReportService.GetMedicineReportDetails(dailyReportId, request);
            return result.Succeeded ? Ok(result.Data) : NotFound(result);
        }

        //[HttpPost("getPaginatedDailyReportList")]
        //public async Task<IActionResult> GetPaginatedDailyReport([FromBody] ListingRequest request)
        //{
        //    var result = await _dailyReportService.GetPaginatedDailyReportList(request);
        //    return result.Succeeded ? Ok(result.Data) : BadRequest(result);
        //}

        [HttpPost("get-list-report-by-livestockCircle/{livestockCircleId}")]
        public async Task<IActionResult> GetPaginatedDailyReportListByLiveStockCircle([FromRoute] Guid livestockCircleId, [FromBody] ListingRequest request)
        {
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, livestockCircleId);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result);
        }

        [HttpGet("get-today-daily-report/{livestockCircleId}")]
        public async Task<IActionResult> GetTodayDailyReport([FromRoute] Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, cancellationToken);
            return result.Succeeded ? Ok(result.Data) : NotFound(result);
        }
        [HttpGet("has-today-daily-report/{livestockCircleId}")]
        public async Task<IActionResult> HasTodayDailyReport([FromRoute] Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, cancellationToken);
            return result.Succeeded ? Ok(result.Data) : NotFound(result);
        }
    }
}