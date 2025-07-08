using Domain.Dto.Request;
using Domain.Dto.Request.LivestockCircle;
using Domain.Dto.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Entities.EntityModel;
using Domain.Helper.Constants;
using Domain.IServices;

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
        //[HttpPost("create")]
        //public async Task<IActionResult> Create([FromBody] CreateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        //{
        //    var (success, errorMessage) = await _livestockCircleService.CreateLiveStockCircle(request, cancellationToken);
        //    if (!success)
        //        return BadRequest(new { error = errorMessage });

        //    return StatusCode(StatusCodes.Status201Created);
        //}

        /// <summary>
        /// Cập nhật thông tin một chu kỳ chăn nuôi.
        /// </summary>
        [HttpPut("update/{livestockCircleId}")]
        public async Task<IActionResult> Update(Guid livestockCircleId, [FromBody] UpdateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _livestockCircleService.UpdateLiveStockCircle(livestockCircleId, request, cancellationToken);
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
        [HttpDelete("disable/{livestockCircleId}")]
        public async Task<IActionResult> DisableLiveStockCircle(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var (success, errorMessage) = await _livestockCircleService.DisableLiveStockCircle(livestockCircleId, cancellationToken);
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
        [HttpGet("getLiveStockCircleById/{livestockCircleId}")]
        public async Task<IActionResult> GetById(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var (circle, errorMessage) = await _livestockCircleService.GetLiveStockCircleById(livestockCircleId, cancellationToken);
            if (circle == null)
                return NotFound(new { error = errorMessage });

            return Ok(circle);
        }

        /// <summary>
        /// Lấy danh sách tất cả chu kỳ chăn nuôi theo status/barn
        /// </summary>
        [HttpGet("getLiveStockCircleByBarnIdAndStatus")]
        public async Task<IActionResult> GetLiveStockCircleByBarnIdAndStatus(string status = null, Guid? barnId = null, CancellationToken cancellationToken = default)
        {
            var (circles, errorMessage) = await _livestockCircleService.GetLiveStockCircleByBarnIdAndStatus(status, barnId, cancellationToken);
            if (circles == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorMessage });

            return Ok(circles);
        }

        /// <summary>
        /// Lấy danh sách chu kỳ chăn nuôi theo ID của nhân viên kỹ thuật.
        /// </summary>
        [HttpGet("getLiveStockCircleByTechnicalStaff/{technicalStaffId}")]
        public async Task<IActionResult> GetByTechnicalStaff(Guid technicalStaffId, CancellationToken cancellationToken = default)
        {
            var (circles, errorMessage) = await _livestockCircleService.GetLiveStockCircleByTechnicalStaff(technicalStaffId, cancellationToken);
            if (circles == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorMessage });

            return Ok(circles);
        }
        /// <summary>
        /// Cập nhật trung bình cân của chu kỳ chăn nuôi
        /// </summary>
        [HttpPatch("updateLivestockCircleAverageWeight/{livestockCircleId}")]
        public async Task<IActionResult> UpdateLivestockCircleAverageWeight(Guid livestockCircleId, [FromBody] float averageWeight)
        {
            var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }
        [HttpPost("technical-staff/assignedbarn")]
        public async Task<IActionResult> GetAssignedBarn([FromBody] ListingRequest req)
        {
            //Guid technicalStaffId;
            try
            {
                Guid.TryParse(User.FindFirst("uid")?.Value, out Guid technicalStaffId);
                var result = await _livestockCircleService.GetAssignedBarn(technicalStaffId, req);
                if (result.Items == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new Exception("User không hợp lệ");
            }



        }
        [HttpPost("admin/livestockCircleHistory/{id}")]
        public async Task<IActionResult> GetLivestockCircleHistory([FromRoute]Guid barnId,[FromBody] ListingRequest req)
        {
            //Guid technicalStaffId;
            try
            {
                
                var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, req);
                if (result.Items == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Chuong không hợp lệ");
            }



        }
        [HttpPost("changeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody]ChangeStatusRequest req)
        {
            if (req.Status.Equals(StatusConstant.RELEASESTAT))
            {
                var result = await _livestockCircleService.ReleaseBarn(req.LivestockCircleId);
                return Ok(result);
            }
            else
            {
                var result = await _livestockCircleService.ChangeStatus(req.LivestockCircleId, req.Status);
                return Ok(result.Success?result.Success : result.ErrorMessage);
            }
        }
        [HttpPost("sale/getBarn")]
        public async Task<IActionResult> getReleasedBarn([FromBody]ListingRequest req)
        {
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(req);
            return Ok(result);
        }
        [HttpGet("sale/getBarnById/{id}")]
        public async Task<IActionResult> getReleasedBarnById([FromRoute]Guid id)
        {
            var result = await _livestockCircleService.GetReleasedLivestockCircleById(id);
            return Ok(result);
        }
    } 
}