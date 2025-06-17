using Domain.Dto.Request;
using Domain.Dto.Request.Food;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic thức ăn.
        /// </summary>
        public FoodController(IFoodService foodService)
        {
            _foodService = foodService ?? throw new ArgumentNullException(nameof(foodService));
        }

        /// <summary>
        /// Tạo một loại thức ăn mới, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFoodRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(folder))
                return BadRequest("Tên folder là bắt buộc.");

            var (success, errorMessage) = await _foodService.CreateAsync(request, folder, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một loại thức ăn, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFoodRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(folder))
                return BadRequest("Tên folder là bắt buộc.");

            var (success, errorMessage) = await _foodService.UpdateAsync(id, request, folder, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một loại thức ăn theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (food, errorMessage) = await _foodService.GetByIdAsync(id, cancellationToken);
            if (food == null)
                return NotFound(errorMessage ?? "Không tìm thấy thức ăn.");
            return Ok(food);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(string foodName = null, Guid? foodCategoryId = null, CancellationToken cancellationToken = default)
        {
            var (foods, errorMessage) = await _foodService.GetAllAsync(foodName, foodCategoryId, cancellationToken);
            if (foods == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách thức ăn.");
            return Ok(foods);
        }
        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("foods/paginated")]
        public async Task<IActionResult> GetPaginatedFoods([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _foodService.GetPaginatedListAsync(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }
    }
}