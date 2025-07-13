using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Medicine;
using Domain.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/medicine-categories")]
    [ApiController]
    public class MedicineCategoryController : ControllerBase
    {
        private readonly IMedicineCategoryService _medicineCategoryService;


        public MedicineCategoryController(IMedicineCategoryService medicineCategoryService)
        {
            _medicineCategoryService = medicineCategoryService;
        }

        [HttpPost("medicine-room-staff/get-medicine-category-list")]
        public async Task<IActionResult> GetPaginatedMedicineCategoryList([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _medicineCategoryService.GetPaginatedMedicineCategoryList(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("medicine-room-staff/create-medicine-category")]
        public async Task<IActionResult> CreateMedicineCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _medicineCategoryService.CreateMedicineCategory(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }


        [HttpPut("medicine-room-staff/update-medicine-category")]
        public async Task<IActionResult> UpdateMedicineCategory([FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _medicineCategoryService.UpdateMedicineCategory(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("medicine-room-staff/disable-medicine-category/{MedicineCategoryId}")]
        public async Task<IActionResult> DisableMedicineCategory([FromRoute] Guid MedicineCategoryId, CancellationToken cancellationToken = default)
        {
            var response = await _medicineCategoryService.DisableMedicineCategory(MedicineCategoryId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("get-medicine-category-by-id/{MedicineCategoryId}")]
        public async Task<IActionResult> GetMedicineCategoryById([FromRoute] Guid MedicineCategoryId, CancellationToken cancellationToken = default)
        {
            var response = await _medicineCategoryService.GetMedicineCategoryById(MedicineCategoryId, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }

        [HttpGet("get-medicine-category-by-name/{name?}")]
        public async Task<IActionResult> GetMedicineCategoryByName([FromRoute] string name = null, CancellationToken cancellationToken = default)
        {
            var response = await _medicineCategoryService.GetMedicineCategoryByName(name, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }

        [HttpGet("get-all-medicine-category")]
        public async Task<IActionResult> GetAllMedicineCategory(CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _medicineCategoryService.GetAllMedicineCategory(cancellationToken);
                return Ok(new Response<List<MedicineCategoryResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy tất cả danh mục thuốc thành công",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<List<MedicineCategoryResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách danh mục thuốc",
                    Errors = new List<string> { ex.Message }
                });
            }
        }


    }
}