using Domain.Dto.Request;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/breed-categories")]
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
        [HttpPost("create")]
        public async Task<IActionResult> CreateBreedCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedCategoryService.CreateBreedCategory(request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một danh mục giống loài.
        /// </summary>
        [HttpPut("update/{BreedCategoryId}")]
        public async Task<IActionResult> UpdateBreedCategory(Guid BreedCategoryId, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedCategoryService.UpdateBreedCategory(BreedCategoryId, request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpDelete("disable/{BreedCategoryId}")]
        public async Task<IActionResult> DisableBreedCategory(Guid BreedCategoryId, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedCategoryService.DisableBreedCategory(BreedCategoryId,  cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một danh mục giống loài theo ID.
        /// </summary>
        [HttpGet("getBreedCategoryById/{BreedCategoryId}")]
        public async Task<IActionResult> GetBreedCategoryById(Guid BreedCategoryId, CancellationToken cancellationToken = default)
        {
            var (breedCategory, errorMessage) = await _breedCategoryService.GetBreedCategoryById(BreedCategoryId, cancellationToken);
            if (breedCategory == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh mục giống loài.");
            return Ok(breedCategory);
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục giống loài đang hoạt động với bộ lọc name.
        /// </summary>
        [HttpGet("getBreedCategoryByName/{name}")]
        public async Task<IActionResult> GetBreedCategoryByName(string name = null, CancellationToken cancellationToken = default)
        {
            var (breedCategories, errorMessage) = await _breedCategoryService.GetBreedCategoryByName(name, cancellationToken);
            if (breedCategories == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách danh mục giống loài.");
            return Ok(breedCategories);
        }

        [HttpPost("getPaginatedBreedCategoryList")]
        public async Task<IActionResult> GetPaginatedBreedCategoryList([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _breedCategoryService.GetPaginatedBreedCategoryList(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }
    }
}