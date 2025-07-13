using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response.Breed;
using Domain.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/breed-categories")]
    [ApiController]
    public class BreedCategoryController : ControllerBase
    {
        private readonly IBreedCategoryService _breedCategoryService;

        public BreedCategoryController(IBreedCategoryService breedCategoryService)
        {
            _breedCategoryService = breedCategoryService ?? throw new ArgumentNullException(nameof(breedCategoryService));
        }
        [HttpPost("breed-room-staff/get-breed-category-list")]
        public async Task<IActionResult> GetPaginatedBreedCategoryList([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _breedCategoryService.GetPaginatedBreedCategoryList(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        [HttpPost("breed-room-staff/create-breed-category")]
        public async Task<IActionResult> CreateBreedCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _breedCategoryService.CreateBreedCategory(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        [HttpPut("breed-room-staff/update-breed-category")]
        public async Task<IActionResult> UpdateBreedCategory([FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _breedCategoryService.UpdateBreedCategory(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        [HttpPut("breed-room-staff/disable-breed-category/{BreedCategoryId}")]
        public async Task<IActionResult> DisableBreedCategory([FromRoute] Guid BreedCategoryId, CancellationToken cancellationToken = default)
        {
            var response = await _breedCategoryService.DisableBreedCategory(BreedCategoryId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("get-breed-category-by-id/{BreedCategoryId}")]
        public async Task<IActionResult> GetBreedCategoryById([FromRoute] Guid BreedCategoryId, CancellationToken cancellationToken = default)
        {
            var response = await _breedCategoryService.GetBreedCategoryById(BreedCategoryId, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }
        //[HttpGet("getBreedCategoryByName")]
        //public async Task<IActionResult> GetBreedCategoryByName(string name = null, CancellationToken cancellationToken = default)
        //{
        //    var response = await _breedCategoryService.GetBreedCategoryByName(name, cancellationToken);
        //    if (!response.Succeeded)
        //        return NotFound(response);
        //    return Ok(response);
        //}

      

        [HttpGet("get-all-breed-category")]
        public async Task<IActionResult> GetAllBreedCategory(CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _breedCategoryService.GetAllCategory(cancellationToken);
                return Ok(new Response<List<BreedCategoryResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy tất cả danh mục giống thành công",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<List<BreedCategoryResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách danh mục giống",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
