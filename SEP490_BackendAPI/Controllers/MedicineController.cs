using Domain.Dto.Request;
using Domain.Dto.Request.Medicine;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicineController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic thuốc.
        /// </summary>
        public MedicineController(IMedicineService medicineService)
        {
            _medicineService = medicineService ?? throw new ArgumentNullException(nameof(medicineService));
        }

        /// <summary>
        /// Tạo một loại thuốc mới, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateMedicine([FromBody] CreateMedicineRequest request, CancellationToken cancellationToken = default)
        {

            var (success, errorMessage) = await _medicineService.CreateMedicine(request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một loại thuốc, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPut("update/{MedicineId}")]
        public async Task<IActionResult> UpdateMedicine(Guid MedicineId, [FromBody] UpdateMedicineRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _medicineService.UpdateMedicine(MedicineId, request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpDelete("disable/{MedicineId}")]
        public async Task<IActionResult> DisableMedicine(Guid MedicineId, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _medicineService.DisableMedicine(MedicineId,cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một loại thuốc theo ID.
        /// </summary>
        [HttpGet("getMedicineById/{MedicineId}")]
        public async Task<IActionResult> GetMedicineById(Guid id, CancellationToken cancellationToken = default)
        {
            var (medicine, errorMessage) = await _medicineService.GetMedicineById(id, cancellationToken);
            if (medicine == null)
                return NotFound(errorMessage ?? "Không tìm thấy thuốc.");
            return Ok(medicine);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet("getMedicineByCategory")]
        public async Task<IActionResult> GetMedicineByCategory(string medicineName = null, Guid? medicineCategoryId = null, CancellationToken cancellationToken = default)
        {
            var (medicines, errorMessage) = await _medicineService.GetMedicineByCategory(medicineName, medicineCategoryId, cancellationToken);
            if (medicines == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách thuốc.");
            return Ok(medicines);
        }

        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("getPaginatedMedicineList")]
        public async Task<IActionResult> GetPaginatedMedicines([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _medicineService.GetPaginatedMedicineList(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }
    }
}