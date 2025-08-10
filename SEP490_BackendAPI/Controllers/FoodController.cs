using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Food;
using Domain.Dto.Response.Food;
using Domain.IServices;
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

        public FoodController(IFoodService foodService, IFoodCategoryService foodCategoryService)
        {
            _foodService = foodService;
            _foodCategoryService = foodCategoryService;
        }

        [HttpPost("food-room-staff/get-food-list")]
        public async Task<IActionResult> GetPaginatedFoods([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _foodService.GetPaginatedFoodList(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("food-room-staff/create-food")]
        public async Task<IActionResult> CreateFood([FromBody] CreateFoodRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _foodService.CreateFood(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("food-room-staff/update-foood")]
        public async Task<IActionResult> UpdateFood([FromBody] UpdateFoodRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _foodService.UpdateFood(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("food-room-staff/disable-food/{FoodId}")]
        public async Task<IActionResult> DisableFood([FromRoute] Guid FoodId, CancellationToken cancellationToken = default)
        {
            var response = await _foodService.DisableFood(FoodId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("get-food-by-id/{FoodId}")]
        public async Task<IActionResult> GetFoodById([FromRoute] Guid FoodId, CancellationToken cancellationToken = default)
        {
            var response = await _foodService.GetFoodById(FoodId, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }

        //[HttpGet("get-food-by-category")]
        //public async Task<IActionResult> GetFoodByCategory(string foodName = null, Guid? foodCategoryId = null, CancellationToken cancellationToken = default)
        //{
        //    var response = await _foodService.GetFoodByCategory(foodName, foodCategoryId, cancellationToken);
        //    if (!response.Succeeded)
        //        return NotFound(response);
        //    return Ok(response);
        //}



        /// <summary>
        /// Lấy tất cả loại thức ăn đang hoạt động.
        /// </summary>
        [HttpGet("get-all-food")]
        public async Task<IActionResult> GetAllFood(CancellationToken cancellationToken = default)
        {
            try
            {
                var foods = await _foodService.GetAllFood(cancellationToken);
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

            var fileBytes = ExportExcelHelper.GenerateExcelTemplateAndData<CellFoodItem, FoodCategoryResponse>("Bảng thức ăn", "Phân loại mẫu thức ăn", CategoryList);
            string fileName = $"Food-Import-Data-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }
    }
}