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

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreedController : ControllerBase
    {
        private readonly IBreedService _breedService;
        private readonly IBreedCategoryService _breedCategoryService;

        /// <summary>
        /// Khởi tạo controller với service để xử lý logic giống loài.
        /// </summary>
        public BreedController(IBreedService breedService, IBreedCategoryService breedCategoryService)
        {
            _breedService = breedService ?? throw new ArgumentNullException(nameof(breedService));
            _breedCategoryService = breedCategoryService;
        }

        /// <summary>
        /// Tạo một giống loài mới, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateBreed([FromBody] CreateBreedRequest request,  CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedService.CreateBreed(request,cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin một giống loài, bao gồm upload ảnh và thumbnail.
        /// </summary>
        [HttpPut("update/{BreedId}")]
        public async Task<IActionResult> UpdateBreed([FromRoute] Guid BreedId, [FromBody] UpdateBreedRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedService.UpdateBreed(BreedId, request, cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        [HttpPut("disable/{BreedId}")]
        public async Task<IActionResult> DisableBreed([FromRoute] Guid BreedId,CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _breedService.DisableBreed(BreedId,cancellationToken);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một giống loài theo ID.
        /// </summary>
        [HttpGet("getBreedById/{BreedId}")]
        public async Task<IActionResult> GetBreedById([FromRoute] Guid BreedId, CancellationToken cancellationToken = default)
        {
            var (breed, errorMessage) = await _breedService.GetBreedById(BreedId, cancellationToken);
            if (breed == null)
                return NotFound(errorMessage ?? "Không tìm thấy giống loài.");
            return Ok(breed);
        }

        /// <summary>
        /// Lấy danh sách tất cả giống loài đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet("getBreedByCategory")]
        public async Task<IActionResult> GetBreedByCategory(string breedName = null, Guid? breedCategoryId = null, CancellationToken cancellationToken = default)
        {
            var (breeds, errorMessage) = await _breedService.GetBreedByCategory(breedName, breedCategoryId, cancellationToken);
            if (breeds == null)
                return NotFound(errorMessage ?? "Không tìm thấy danh sách giống loài.");
            return Ok(breeds);
        }

        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpPost("getPaginatedBreedList")]
        public async Task<IActionResult> GetPaginatedBreeds([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _breedService.GetPaginatedBreedList(request);
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
                throw new Exception("Lỗi dữ liệu file");
            }
        }
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal(Domain.Helper.Constants.LienceConstant.NonCommercialPersonal);
            var CategoryList = new List<BreedCategoryResponse>();

            CategoryList = await _breedCategoryService.GetAllCategory();

            var fileBytes = ExportExcelHelper.GenerateExcelTemplateAndData<CellBreedItem, BreedCategoryResponse>("Bảng thuốc", "Phân loại mẫu thuốc", CategoryList);
            string fileName = $"breed-Import-Data-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }
    }
}