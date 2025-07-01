using Domain.Dto.Request;
using Domain.Dto.Request.Breed;
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
        [HttpPost("create")]
        public async Task<IActionResult> CreateBreed([FromBody] CreateBreedRequest request,  CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedService.CreateBreed(request,cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một giống loài, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPut("update/{BreedId}")]
        public async Task<IActionResult> UpdateBreed([FromRoute] Guid BreedId, [FromBody] UpdateBreedRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedService.UpdateBreed(BreedId, request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpPut("disable/{BreedId}")]
        public async Task<IActionResult> DisableBreed([FromRoute] Guid BreedId,CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedService.DisableBreed(BreedId,cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một giống loài theo ID.
        /// </summary>
        [HttpGet("getBreedById/{BreedId}")]
        public async Task<IActionResult> GetBreedById([FromRoute] Guid BreedId, CancellationToken cancellationToken = default)
        {
            var (breed, errorMessage) = await _breedService.GetBreedById(BreedId, cancellationToken);
            if (breed == null)
                return NotFound(errorMessage ?? "Không tìm thấy giống loài.");
            return Ok(breed);
        }

        /// <summary>
        /// Lấy danh sách tất cả giống loài đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet("getBreedByCategory")]
        public async Task<IActionResult> GetBreedByCategory(string breedName = null, Guid? breedCategoryId = null, CancellationToken cancellationToken = default)
        {
            var (breeds, errorMessage) = await _breedService.GetBreedByCategory(breedName, breedCategoryId, cancellationToken);
            if (breeds == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách giống loài.");
            return Ok(breeds);
        }

        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("getPaginatedBreedList")]
        public async Task<IActionResult> GetPaginatedBreeds([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _breedService.GetPaginatedBreedList(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }
    }
}