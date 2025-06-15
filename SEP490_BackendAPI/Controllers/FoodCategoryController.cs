using Domain.Dto.Request;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var (success, errorMessage) = await _foodCategoryService.CreateAsync(request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            var (success, errorMessage) = await _foodCategoryService.UpdateAsync(id, request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var (category, errorMessage) = await _foodCategoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(errorMessage);
            return Ok(category);
        }
    }
}
