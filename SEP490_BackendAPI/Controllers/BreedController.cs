using Domain.Dto.Request;
using Domain.Dto.Request.Breed;
using Domain.Services.Implements;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreedController : ControllerBase
    {
        private readonly IBreedService _breedService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic giống loài.
        /// </summary>
        public BreedController(IBreedService breedService)
        {
            _breedService = breedService ?? throw new ArgumentNullException(nameof(breedService));
        }

        /// <summary>
        /// Tạo một giống loài mới, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBreedRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(folder))
                return BadRequest("Tên folder là bắt buộc.");

            var (success, errorMessage) = await _breedService.CreateAsync(request, folder, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một giống loài, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBreedRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(folder))
                return BadRequest("Tên folder là bắt buộc.");

            var (success, errorMessage) = await _breedService.UpdateAsync(id, request, folder, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một giống loài theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (breed, errorMessage) = await _breedService.GetByIdAsync(id, cancellationToken);
            if (breed == null)
                return NotFound(errorMessage ?? "Không tìm thấy giống loài.");
            return Ok(breed);
        }

        /// <summary>
        /// Lấy danh sách tất cả giống loài đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(string breedName = null, Guid? breedCategoryId = null, CancellationToken cancellationToken = default)
        {
            var (breeds, errorMessage) = await _breedService.GetAllAsync(breedName, breedCategoryId, cancellationToken);
            if (breeds == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách giống loài.");
            return Ok(breeds);
        }

        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("breeds/paginated")]
        public async Task<IActionResult> GetPaginatedBreeds([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _breedService.GetPaginatedListAsync(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }
    }
}