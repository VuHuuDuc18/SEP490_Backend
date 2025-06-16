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
    public class MedicineReportController : ControllerBase
    {
        private readonly IMedicineReportService _medicineReportService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic báo cáo thuốc.
        /// </summary>
        public MedicineReportController(IMedicineReportService medicineReportService)
        {
            _medicineReportService = medicineReportService ?? throw new ArgumentNullException(nameof(medicineReportService));
        }

        /// <summary>
        /// Tạo một báo cáo thuốc mới và trừ lượng còn lại trong LivestockCircleMedicine.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicineReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _medicineReportService.CreateAsync(requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một báo cáo thuốc và điều chỉnh lượng còn lại trong LivestockCircleMedicine.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicineReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _medicineReportService.UpdateAsync(id, requestDto, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Xóa mềm một báo cáo thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _medicineReportService.DeleteAsync(id, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một báo cáo thuốc theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (medicineReport, errorMessage) = await _medicineReportService.GetByIdAsync(id, cancellationToken);
            if (medicineReport == null)
                return NotFound(errorMessage ?? "Không tìm thấy báo cáo thuốc.");
            return Ok(medicineReport);
        }

        /// <summary>
        /// Lấy danh sách tất cả báo cáo thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(Guid? medicineId = null, Guid? reportId = null, CancellationToken cancellationToken = default)
        {
            var (medicineReports, errorMessage) = await _medicineReportService.GetAllAsync(medicineId, reportId, cancellationToken);
            if (medicineReports == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách báo cáo thuốc.");
            return Ok(medicineReports);
        }
    }
}