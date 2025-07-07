using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Services.Interfaces;
using Infrastructure.Services.Implements;
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateMedicineCategory([FromBody] CreateCategoryRequest request)
        {
            var (success, errorMessage) = await _medicineCategoryService.CreateMedicineCategory(request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpPut("update/{MedicineCategoryId}")]
        public async Task<IActionResult> UpdateMedicineCategory([FromRoute] Guid MedicineCategoryId, [FromBody] UpdateCategoryRequest request)
        {
            var (success, errorMessage) = await _medicineCategoryService.UpdateMedicineCategory(MedicineCategoryId, request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpDelete("disable/{MedicineCategoryId}")]
        public async Task<IActionResult> DisableMedicineCategory([FromRoute] Guid MedicineCategoryId)
        {
            var (success, errorMessage) = await _medicineCategoryService.DisableMedicineCategory(MedicineCategoryId);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpGet("getMedicineCategoryByName/{name}")]
        public async Task<IActionResult> GetMedicineCategoryByName([FromRoute] string name)
        {
            var (category, errorMessage) = await _medicineCategoryService.GetMedicineCategoryByName(name);
            if (category == null)
                return NotFound(errorMessage);
            return Ok(category);
        }


        [HttpGet("getMedicineCategoryById/{MedicineCategoryId}")]
        public async Task<IActionResult> GetMedicineCategoryById([FromRoute] Guid MedicineCategoryId)
        {
            var (category, errorMessage) = await _medicineCategoryService.GetMedicineCategoryById(MedicineCategoryId);
            if (category == null)
                return NotFound(errorMessage);
            return Ok(category);
        }

        [HttpPost("getPaginatedMedicineCategoryList")]
        public async Task<IActionResult> GetPaginatedMedicineCategoryList([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _medicineCategoryService.GetPaginatedMedicineCategoryList(request);
            if (errorMessage != null)
                return BadRequest(errorMessage);
            return Ok(result);
        }
    }
}
