using Domain.Dto.Request.LivestockCircle;
using Domain.Dto.Response;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivestockCircleController : ControllerBase
    {
        private readonly ILivestockCircleService _livestockCircleService;

        public LivestockCircleController(ILivestockCircleService livestockCircleService)
        {
            _livestockCircleService = livestockCircleService ?? throw new ArgumentNullException(nameof(livestockCircleService));
        }

        /// <summary>
        /// Tạo một chu kỳ chăn nuôi mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _livestockCircleService.CreateAsync(request, cancellationToken);
            if (!success)
                return BadRequest(new { error = errorMessage });

            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Cập nhật thông tin một chu kỳ chăn nuôi.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _livestockCircleService.UpdateAsync(id, request, cancellationToken);
            if (!success)
            {
                if (errorMessage.Contains("Không tìm thấy"))
                    return NotFound(new { error = errorMessage });
                return BadRequest(new { error = errorMessage });
            }

            return Ok();
        }

        /// <summary>
        /// Xóa mềm một chu kỳ chăn nuôi.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _livestockCircleService.DeleteAsync(id, cancellationToken);
            if (!success)
            {
                if (errorMessage.Contains("Không tìm thấy"))
                    return NotFound(new { error = errorMessage });
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorMessage });
            }

            return Ok();
        }

        /// <summary>
        /// Lấy thông tin một chu kỳ chăn nuôi theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var (circle, errorMessage) = await _livestockCircleService.GetByIdAsync(id, cancellationToken);
            if (circle == null)
                return NotFound(new { error = errorMessage });

            return Ok(circle);
        }

        /// <summary>
        /// Lấy danh sách tất cả chu kỳ chăn nuôi đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(string status = null, Guid? barnId = null, CancellationToken cancellationToken = default)
        {
            var (circles, errorMessage) = await _livestockCircleService.GetAllAsync(status, barnId, cancellationToken);
            if (circles == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorMessage });

            return Ok(circles);
        }

        /// <summary>
        /// Lấy danh sách chu kỳ chăn nuôi theo ID của nhân viên kỹ thuật.
        /// </summary>
        [HttpGet("technical-staff/{technicalStaffId}")]
        public async Task<IActionResult> GetByTechnicalStaff(Guid technicalStaffId, CancellationToken cancellationToken = default)
        {
            var (circles, errorMessage) = await _livestockCircleService.GetByTechnicalStaffAsync(technicalStaffId, cancellationToken);
            if (circles == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorMessage });

            return Ok(circles);
        }
        /// <summary>
        /// Cập nhật trung bình cân của chu kỳ chăn nuôi
        /// </summary>
        [HttpPatch("livestock-circles/{id}/average-weight")]
        public async Task<IActionResult> UpdateLivestockCircleAverageWeight(Guid id, [FromBody] float averageWeight)
        {
            var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeightAsync(id, averageWeight);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }
    }
}