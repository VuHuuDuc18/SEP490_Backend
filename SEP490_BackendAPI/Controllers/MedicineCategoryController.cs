using Domain.Dto.Request;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/medicine-categories")]
    public class MedicineCategoryController : ControllerBase
    {
        private readonly IMedicineCategoryService _medicineCategoryService;

        public MedicineCategoryController(IMedicineCategoryService medicineCategoryService)
        {
            _medicineCategoryService = medicineCategoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var (success, errorMessage) = await _medicineCategoryService.CreateAsync(request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            var (success, errorMessage) = await _medicineCategoryService.UpdateAsync(id, request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var (category, errorMessage) = await _medicineCategoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(errorMessage);
            return Ok(category);
        }
    }
}
