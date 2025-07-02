using Domain.Dto.Request;
using Domain.Dto.Request.Food;
using Domain.Dto.Response.Food;
using Domain.Services.Interfaces;
using ExcelControl.Handler;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;
        private readonly IFoodCategoryService _foodCategoryService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic thức ăn.
        /// </summary>
        public FoodController(IFoodService foodService, IFoodCategoryService foodCategoryService)
        {
            _foodService = foodService ?? throw new ArgumentNullException(nameof(foodService));
            this._foodCategoryService = foodCategoryService;
        }

        /// <summary>
        /// Tạo một loại thức ăn mới, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateFood([FromBody] CreateFoodRequest request, CancellationToken cancellationToken = default)
        {
 
            var (success, errorMessage) = await _foodService.CreateFood(request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một loại thức ăn, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPut("update/{FoodId}")]
        public async Task<IActionResult> UpdateFood(Guid FoodId, [FromBody] UpdateFoodRequest request, CancellationToken cancellationToken = default)
        {

            var (success, errorMessage) = await _foodService.UpdateFood(FoodId, request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpDelete("disable/{FoodId}")]
        public async Task<IActionResult> UpdateFood(Guid FoodId, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _foodService.DisableFood(FoodId, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một loại thức ăn theo ID.
        /// </summary>
        [HttpGet("getFoodById/{FoodId}")]
        public async Task<IActionResult> GetFoodById(Guid FoodId, CancellationToken cancellationToken = default)
        {
            var (food, errorMessage) = await _foodService.GetFoodById(FoodId, cancellationToken);
            if (food == null)
                return NotFound(errorMessage ?? "Không tìm thấy thức ăn.");
            return Ok(food);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet("getFoodByCategory")]
        public async Task<IActionResult> GetFoodByCategory(string foodName = null, Guid? foodCategoryId = null, CancellationToken cancellationToken = default)
        {
            var (foods, errorMessage) = await _foodService.GetFoodByCategory(foodName, foodCategoryId, cancellationToken);
            if (foods == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách thức ăn.");
            return Ok(foods);
        }
        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("getPaginatedFoodList")]
        public async Task<IActionResult> GetPaginatedFoods([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _foodService.GetPaginatedFoodList(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }
        [HttpPost("import")]
        public async Task<IActionResult> ImportPersonExcel(IFormFile file)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal(Domain.Helper.Constants.LienceConstant.NonCommercialPersonal);

                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                List<CellFoodItem> items;
                using (var stream = new MemoryStream())
                {

                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    items = ExportExcelHelper.ImportExcelFile<CellFoodItem>(stream);
                }

                return Ok(await _foodService.ExcelDataHandle(items));
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi dữ liệu file");
            }
        }
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal(Domain.Helper.Constants.LienceConstant.NonCommercialPersonal);
            var CategoryList = new List<FoodCategoryResponse>();

            CategoryList = await _foodCategoryService.GetAllCategory();

            var fileBytes = ExportExcelHelper.GenerateExcelTemplateAndData<CellFoodItem, FoodCategoryResponse>("Bảng thuốc", "Phân loại mẫu thuốc", CategoryList);
            string fileName = $"Food-Import-Data-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }
    }
}