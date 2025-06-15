using Domain.Dto.Request;
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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicineRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(folder))
                return BadRequest("Tên folder là bắt buộc.");

            var (success, errorMessage) = await _medicineService.CreateAsync(request, folder, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một loại thuốc, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicineRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(folder))
                return BadRequest("Tên folder là bắt buộc.");

            var (success, errorMessage) = await _medicineService.UpdateAsync(id, request, folder, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một loại thuốc theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (medicine, errorMessage) = await _medicineService.GetByIdAsync(id, cancellationToken);
            if (medicine == null)
                return NotFound(errorMessage ?? "Không tìm thấy thuốc.");
            return Ok(medicine);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(string medicineName = null, Guid? medicineCategoryId = null, CancellationToken cancellationToken = default)
        {
            var (medicines, errorMessage) = await _medicineService.GetAllAsync(medicineName, medicineCategoryId, cancellationToken);
            if (medicines == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách thuốc.");
            return Ok(medicines);
        }
    }
}