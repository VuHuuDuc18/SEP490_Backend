using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Food;
using Domain.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/food-categories")]
    [ApiController]
    public class FoodCategoryController : ControllerBase
    {
        private readonly IFoodCategoryService _foodCategoryService;

        public FoodCategoryController(IFoodCategoryService foodCategoryService)
        {
            _foodCategoryService = foodCategoryService ;
        }

        [HttpPost("food-room-staff/get-food-category-list")]
        public async Task<IActionResult> GetPaginatedFoodCategories([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _foodCategoryService.GetPaginatedFoodCategoryList(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("food-room-staff/create-food-category")]
        public async Task<IActionResult> CreateFoodCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _foodCategoryService.CreateFoodCategory(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }


        [HttpPut("food-room-staff/update-food-category")]
        public async Task<IActionResult> UpdateFoodCategory([FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _foodCategoryService.UpdateFoodCategory(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("food-room-staff/disable-food-category/{FoodCategoryId}")]
        public async Task<IActionResult> DisableFoodCategory([FromRoute] Guid FoodCategoryId, CancellationToken cancellationToken = default)
        {
            var response = await _foodCategoryService.DisableFoodCategory(FoodCategoryId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("get-food-category-by-id/{FoodCategoryId}")]
        public async Task<IActionResult> GetFoodCategoryById([FromRoute] Guid FoodCategoryId, CancellationToken cancellationToken = default)
        {
            var response = await _foodCategoryService.GetFoodCategoryById(FoodCategoryId, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }

        [HttpGet("get-food-category-by-name/{name?}")]
        public async Task<IActionResult> GetFoodCategoryByName([FromRoute] string name = null, CancellationToken cancellationToken = default)
        {
            var response = await _foodCategoryService.GetFoodCategoryByName(name, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }

        [HttpGet("get-all-food-category")]
        public async Task<IActionResult> GetAllFoodCategory(CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _foodCategoryService.GetAllCategory(cancellationToken);
                return Ok(new Response<List<FoodCategoryResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy tất cả danh mục thức ăn thành công",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<List<FoodCategoryResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách danh mục thức ăn",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}