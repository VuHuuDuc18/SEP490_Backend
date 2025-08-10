using Domain.Dto.Request;
using Domain.Dto.Request.Breed;
using ExcelControl.Handler;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response.Breed;
using Domain.IServices;
using Application.Wrappers;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreedController : ControllerBase
    {
        private readonly IBreedService _breedService;
        private readonly IBreedCategoryService _breedCategoryService;

        public BreedController(IBreedService breedService, IBreedCategoryService breedCategoryService)
        {
            _breedService = breedService ?? throw new ArgumentNullException(nameof(breedService));
            _breedCategoryService = breedCategoryService ?? throw new ArgumentNullException(nameof(breedCategoryService));
        }
        [HttpPost("breed-room-staff/get-breed-list")]
        public async Task<IActionResult> GetPaginatedBreeds([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _breedService.GetPaginatedBreedList(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("breed-room-staff/create-breed")]
        public async Task<IActionResult> CreateBreed([FromBody] CreateBreedRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _breedService.CreateBreed(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("breed-room-staff/update-breed")]
        public async Task<IActionResult> UpdateBreed([FromBody] UpdateBreedRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _breedService.UpdateBreed(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("breed-room-staff/disable-breed/{BreedId}")]
        public async Task<IActionResult> DisableBreed([FromRoute] Guid BreedId, CancellationToken cancellationToken = default)
        {
            var response = await _breedService.DisableBreed(BreedId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("get-breed-by-id/{BreedId}")]
        public async Task<IActionResult> GetBreedById([FromRoute] Guid BreedId, CancellationToken cancellationToken = default)
        {
            var response = await _breedService.GetBreedById(BreedId, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }

        //[HttpGet("get-breed-by-category")]
        //public async Task<IActionResult> GetBreedByCategory(string breedName = null, Guid? breedCategoryId = null, CancellationToken cancellationToken = default)
        //{
        //    var response = await _breedService.GetBreedByCategory(breedName, breedCategoryId, cancellationToken);
        //    if (!response.Succeeded)
        //        return NotFound(response);
        //    return Ok(response);
        //}

        [HttpGet("get-all-breed")]
        public async Task<IActionResult> GetAllBreed(CancellationToken cancellationToken = default)
        {
            try
            {
                var breeds = await _breedService.GetAllBreed(cancellationToken);
                return Ok(new Response<List<BreedResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy tất cả giống loài thành công",
                    Data = breeds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<List<BreedResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách giống loài",
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

                List<CellBreedItem> items;
                using (var stream = new MemoryStream())
                {

                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    items = ExportExcelHelper.ImportExcelFile<CellBreedItem>(stream);
                }

                return Ok(await _breedService.ExcelDataHandle(items));
            }
            catch (Exception ex)
            {
                return Ok(new Application.Wrappers.Response<bool>("Lỗi dữ liệu file"));
            }
        }
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal(Domain.Helper.Constants.LienceConstant.NonCommercialPersonal);
            var CategoryList = new List<BreedCategoryResponse>();

            CategoryList = await _breedCategoryService.GetAllCategory();

            var fileBytes = ExportExcelHelper.GenerateExcelTemplateAndData<CellBreedItem, BreedCategoryResponse>("Bảng giống", "Phân loại giống", CategoryList);
            string fileName = $"Breed-Import-Data-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }
    }
}