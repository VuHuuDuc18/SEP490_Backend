﻿using Domain.Dto.Request;
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
using Domain.DTOs.Request.LivestockCircle;

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

        //[HttpPut("update/{livestockCircleId}")]
        //public async Task<IActionResult> Update(Guid livestockCircleId, [FromBody] UpdateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        //{
        //    var (success, errorMessage) = await _livestockCircleService.UpdateLiveStockCircle(livestockCircleId, request, cancellationToken);
        //    if (!success)
        //    {
        //        if (errorMessage.Contains("Không tìm thấy"))
        //            return NotFound(new { error = errorMessage });
        //        return BadRequest(new { error = errorMessage });
        //    }

        //    return Ok();
        //}
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
        //[HttpGet("getLiveStockCircleByBarnIdAndStatus")]
        //public async Task<IActionResult> GetLiveStockCircleByBarnIdAndStatus(string status = null, Guid? barnId = null, CancellationToken cancellationToken = default)
        //{
        //    var (circles, errorMessage) = await _livestockCircleService.GetLiveStockCircleByBarnIdAndStatus(status, barnId, cancellationToken);
        //    if (circles == null)
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorMessage });

        //    return Ok(circles);
        //}

        [HttpGet("getCurrentLiveStockCircleByBarnId/{barnId}")]
        public async Task<IActionResult> GetLiveStockCircleByBarnId([FromRoute] Guid barnId, CancellationToken cancellationToken = default)
        {
            var (circles, errorMessage) = await _livestockCircleService.GetActiveLiveStockCircleByBarnId(barnId, cancellationToken);
            if (circles == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorMessage });

            return Ok(circles);
        }



        /// <summary>
        /// Lấy danh sách chu kỳ chăn nuôi theo ID của nhân viên kỹ thuật.
        /// </summary>
        //[HttpGet("getLiveStockCircleByTechnicalStaff/{technicalStaffId}")]
        //public async Task<IActionResult> GetByTechnicalStaff(Guid technicalStaffId, CancellationToken cancellationToken = default)
        //{
        //    var (circles, errorMessage) = await _livestockCircleService.GetLiveStockCircleByTechnicalStaff(technicalStaffId, cancellationToken);
        //    if (circles == null)
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { error = errorMessage });

        //    return Ok(circles);
        //}
        /// <summary>
        /// Cập nhật trung bình cân của chu kỳ chăn nuôi
        /// </summary>
        [HttpPatch("update-livestockCircle-average-weight/{livestockCircleId}")]
        public async Task<IActionResult> UpdateLivestockCircleAverageWeight([FromRoute]Guid livestockCircleId, [FromBody] UpdateAvgWeightDTO request)
        {
            var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, request);
            if (!success)
                return BadRequest(errorMessage);
            return Ok();
        }
        //[HttpPost("technical-staff/assignedbarn")]
        //public async Task<IActionResult> GetAssignedBarn([FromBody] ListingRequest req)
        //{
        //    //Guid technicalStaffId;
        //    try
        //    {
        //        Guid.TryParse(User.FindFirst("uid")?.Value, out Guid technicalStaffId);
        //        var result = await _livestockCircleService.GetAssignedBarn(technicalStaffId, req);
        //        if (result.Items == null)
        //            return StatusCode(StatusCodes.Status500InternalServerError);

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("User không hợp lệ");
        //    }
        //}
        [HttpPost("livestockCircle-history/{barnId}")]
        public async Task<IActionResult> GetLivestockCircleHistory([FromRoute] Guid barnId, [FromBody] ListingRequest req)
        {
            //Guid technicalStaffId;        
            return Ok(await _livestockCircleService.GetLivestockCircleHistory(barnId, req));
        }

        //[HttpPost("sale/getBarn")]
        //public async Task<IActionResult> GetReleasedBarn([FromBody] ListingRequest req)
        //{
        //    var result = await _livestockCircleService.GetReleasedLivestockCircleList(req);
        //    return Ok(result);
        //}
        //[HttpGet("sale/getBarnById/{id}")]
        //public async Task<IActionResult> GetReleasedBarnById([FromRoute] Guid id)
        //{
        //    var result = await _livestockCircleService.GetLiveStockCircleById(id);
        //    return Ok(result);
        //}
        [HttpPut("sale/set-pre-order-field")]
        public async Task<IActionResult> SetPreOrderField([FromBody] SetPreOrderFieldRequest request)
        {            
            return Ok(await _livestockCircleService.SetPreOrderField(request));
        }


        [HttpPost("get-food-remaining/{liveStockCircleId}")]
        public async Task<IActionResult> GetFoodRemaining(
            [FromRoute] Guid liveStockCircleId,
            [FromBody] ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            return Ok(await _livestockCircleService.GetFoodRemaining(liveStockCircleId, request));
        }

        [HttpPost("get-medicine-remaining/{liveStockCircleId}")]
        public async Task<IActionResult> GetMedicineRemaining(
            [FromRoute] Guid liveStockCircleId,
            [FromBody] ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            return Ok(await _livestockCircleService.GetMedicineRemaining(liveStockCircleId, request));
        }
        [HttpPut("technical-taff/release-barn")]
        public async Task<IActionResult> ReleaseBarn([FromBody] ReleaseBarnRequest req)
        {
            return Ok(await _livestockCircleService.ReleaseBarn(req));
        }
    }
}