using Domain.Dto.Request;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreedCategoryController : ControllerBase
    {
        private readonly IBreedCategoryService _breedCategoryService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic danh mục giống loài.
        /// </summary>
        public BreedCategoryController(IBreedCategoryService breedCategoryService)
        {
            _breedCategoryService = breedCategoryService ?? throw new ArgumentNullException(nameof(breedCategoryService));
        }

        /// <summary>
        /// Tạo một danh mục giống loài mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedCategoryService.CreateAsync(request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một danh mục giống loài.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedCategoryService.UpdateAsync(id, request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một danh mục giống loài theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (breedCategory, errorMessage) = await _breedCategoryService.GetByIdAsync(id, cancellationToken);
            if (breedCategory == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh mục giống loài.");
            return Ok(breedCategory);
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục giống loài đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(string name = null, CancellationToken cancellationToken = default)
        {
            var (breedCategories, errorMessage) = await _breedCategoryService.GetAllAsync(name, cancellationToken);
            if (breedCategories == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách danh mục giống loài.");
            return Ok(breedCategories);
        }
    }
}