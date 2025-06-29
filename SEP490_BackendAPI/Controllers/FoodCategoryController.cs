using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/food-categories")]
    public class FoodCategoryController : ControllerBase
    {
        private readonly IFoodCategoryService _foodCategoryService;

        public FoodCategoryController(IFoodCategoryService foodCategoryService)
        {
            _foodCategoryService = foodCategoryService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateFoodCategory([FromBody] CreateCategoryRequest request)
        {
            var (success, errorMessage) = await _foodCategoryService.CreateFoodCategory(request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpPut("update/{FoodCategoryId}")]
        public async Task<IActionResult> UpdateFoodCategory(Guid FoodCategoryId, [FromBody] UpdateCategoryRequest request)
        {
            var (success, errorMessage) = await _foodCategoryService.UpdateFoodCategory(FoodCategoryId, request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpDelete("disable/{FoodCategoryId}")]
        public async Task<IActionResult> DisableFoodCategory(Guid FoodCategoryId)
        {
            var (success, errorMessage) = await _foodCategoryService.DisableFoodCategory(FoodCategoryId);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpGet("getFoodCategoryById/{FoodCategoryId}")]
        public async Task<IActionResult> GetFoodCategoryById(Guid FoodCategoryId)
        {
            var (category, errorMessage) = await _foodCategoryService.GetFoodCategoryById(FoodCategoryId);
            if (category == null)
                return NotFound(errorMessage);
            return Ok(category);
        }

        [HttpGet("getFoodCategoryByName/{name}")]
        public async Task<IActionResult> GetFoodCategoryByName(string name)
        {
            var (category, errorMessage) = await _foodCategoryService.GetFoodCategoryByName(name);
            if (category == null)
                return NotFound(errorMessage);
            return Ok(category);
        }

        [HttpPost("getPaginatedFoodCategories")]
        public async Task<IActionResult> GetPaginatedFoodCategories([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _foodCategoryService.GetPaginatedFoodCategoryList(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }
    }
}
