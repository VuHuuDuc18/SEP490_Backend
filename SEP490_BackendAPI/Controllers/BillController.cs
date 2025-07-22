using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Request.Bill.Admin;
using Domain.Dto.Request.Breed;
using Domain.Dto.Request.LivestockCircle;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using Domain.Helper.Constants;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.FormulaExpressions.CompileResults;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillController : ControllerBase
    {
        private readonly IBillService _billService;
        private readonly ILivestockCircleService _livestockCircleService;
        public BillController(IBillService billService, ILivestockCircleService livestockCircleService)
        {
            _billService = billService;
            _livestockCircleService = livestockCircleService;
        }
        [HttpPost("technical-staff/get-bill-food-list")]
        public async Task<IActionResult> GetPaginatedBillsFoodByTechnicalStaff([FromBody] ListingRequest request)
        {
            var response = await _billService.GetPaginatedBillListByTechicalStaff(request, TypeBill.FOOD);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("technical-staff/get-bill-medicine-list")]
        public async Task<IActionResult> GetPaginatedBillsMedicineByTechnicalStaff([FromBody] ListingRequest request)
        {
            var response = await _billService.GetPaginatedBillListByTechicalStaff(request, TypeBill.MEDICINE);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("technical-staff/update-bill-food")]
        public async Task<IActionResult> UpdateBillFood([FromBody] UpdateBillFoodDto request)
        {
            var response = await _billService.UpdateBillFood(request);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("technical-staff/update-bill-medicine")]
        public async Task<IActionResult> UpdateBillMedicine([FromBody] UpdateBillMedicineDto request)
        {
            var response = await _billService.UpdateBillMedicine(request);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("technical-staff/request-food")]
        public async Task<IActionResult> RequestFood([FromBody] CreateFoodRequestDto request)
        {
            var response = await _billService.RequestFood(request);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("technical-staff/request-medicine")]
        public async Task<IActionResult> RequestMedicine([FromBody] CreateMedicineRequestDto request)
        {
            var response = await _billService.RequestMedicine(request);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        [HttpPut("technical-staff/cancel-bill")]
        public async Task<IActionResult> CancelBill(Guid billId)
        {
            return Ok(await _billService.CancelBill(billId));
        }


        [HttpPatch("disable/bill-item/{billItemId}")]
        public async Task<IActionResult> DisableBillItem([FromRoute] Guid billItemId)
        {
            var response = await _billService.DisableBillItem(billItemId);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPatch("disable/bill/{billId}")]
        public async Task<IActionResult> DisableBill([FromRoute] Guid billId)
        {
            var response = await _billService.DisableBill(billId);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("get-bill-items/{billId}")]
        public async Task<IActionResult> GetBillItemsByBillId([FromRoute] Guid billId, [FromBody] ListingRequest request)
        {
            var response = await _billService.GetBillItemsByBillId(billId, request);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("get-bill-by-id/{billId}")]
        public async Task<IActionResult> GetBillById([FromRoute] Guid billId)
        {
            var response = await _billService.GetBillById(billId);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        //[HttpPost("get-paginated-bills")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> GetPaginatedBills([FromBody] ListingRequest request)
        //{
        //    var (result, errorMessage) = await _billService.GetPaginatedBillList(request);
        //    if (errorMessage != null)
        //        return BadRequest(new { error = errorMessage });
        //    return Ok(result);
        //}


        [HttpPost("food-room-staff/get-list-request-food")]
        public async Task<IActionResult> GetRequestFood([FromBody] ListingRequest request)
        {
            var response = await _billService.GetBillRequestByType(request, TypeBill.FOOD);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("food-room-staff/get-bill-list-food-history")]
        public async Task<IActionResult> GetPaginatedBillsHistoryFood([FromBody] ListingRequest request)
        {
            var response = await _billService.GetPaginatedBillListHistory(request, TypeBill.FOOD);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }


        [HttpPost("medicine-room-staff/get-list-request-medicine")]
        public async Task<IActionResult> GetRequestMedicine([FromBody] ListingRequest request)
        {
            var response = await _billService.GetBillRequestByType(request, TypeBill.MEDICINE);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }


        [HttpPost("medicine-room-staff/get-bill-list-medicine-history")]
        public async Task<IActionResult> GetPaginatedBillsHistoryMedicine([FromBody] ListingRequest request)
        {
            var response = await _billService.GetPaginatedBillListHistory(request, TypeBill.MEDICINE);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("breed-room-staff/get-list-request-breed")]
        public async Task<IActionResult> GetRequestBreed([FromBody] ListingRequest request)
        {
            var response = await _billService.GetBillRequestByType(request, TypeBill.BREED);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("breed-room-staff/get-bill-list-breed-history")]
        public async Task<IActionResult> GetPaginatedBillsHistoryBreed([FromBody] ListingRequest request)
        {
            var response = await _billService.GetPaginatedBillListHistory(request, TypeBill.BREED);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }


        [HttpPut("approve-bill")]
        public async Task<IActionResult> ApproveBill(Guid billId)
        {
            return Ok(await _billService.ApproveBill(billId));
        }

        [HttpPut("reject-bill")]
        public async Task<IActionResult> RejectBill(Guid billId)
        {
            return Ok(await _billService.RejectBill(billId));
        }
        [HttpPost("worker/get-bill-list-approve-by-worker")]
        public async Task<IActionResult> GetApprovalBillsByWorker([FromBody] ListingRequest request)
        {
            var response = await _billService.GetApprovedBillsByWorker(request);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("worker/get-history-bill-list-by-worker")]
        public async Task<IActionResult> GetHistoryBillsByWorker([FromBody] ListingRequest request)
        {
            var response = await _billService.GetHistoryBillsByWorker(request);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("worker/confirm-bill")]
        public async Task<IActionResult> ConfirmBill(Guid billId)
        {
            return Ok(await _billService.ConfirmBill(billId));
        }
        [HttpPost("admin/request-breed")]
        [Authorize(Roles = "Company Admin")]
        public async Task<IActionResult> RequestBreed([FromBody] AdminCreateBreedBillRequest request)

        {
            try
            {
                var createLivestockCircleRequest = new CreateLivestockCircleRequest()
                {
                    BarnId = request.BarnId,
                    BreedId = request.BreedId,
                    LivestockCircleName = request.LivestockCircleName,
                    TechicalStaffId = request.TechnicalStaffId,
                    TotalUnit = request.Stock
                };
                var livestockCircle = await _livestockCircleService.CreateLiveStockCircle(createLivestockCircleRequest);
                if (!livestockCircle.Succeeded)
                {
                    return Ok(new Application.Wrappers.Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Không thể tạo yêu cầu giống"

                    });
                }
                var BillBreedToRequest = new CreateBreedRequestDto()
                {
                    LivestockCircleId = livestockCircle.Data,
                    Note = request.Note,
                    UserRequestId = Guid.Parse(User.FindFirst("uid")?.Value),
                    BreedItems = new List<BreedItemRequest>()
                    {
                        new BreedItemRequest()
                        {
                            ItemId = request.BreedId,
                            Quantity = request.Stock
                        }
                    }
                };

                var result = await _billService.RequestBreed(BillBreedToRequest);
                if (!result.Success)
                {
                    await _livestockCircleService.DisableLiveStockCircle(livestockCircle.Data);

                    return Ok(new Application.Wrappers.Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Không thể tạo yêu cầu giống"

                    });
                }

                //return BadRequest(new { error = result.ErrorMessage });
                return Ok(new Application.Wrappers.Response<bool>()
                {
                    Succeeded = true,
                    Message = "Tạo yêu cầu giống thành công"

                });

            }
            catch (Exception ex)
            {
                return Ok(new Application.Wrappers.Response<bool>()
                {
                    Succeeded = false,
                    Message = "Không thể tạo yêu cầu giống"

                });
            }
        }

        [HttpPost("admin/updateBill")]
        public async Task<IActionResult> UpdateBill([FromBody] Admin_UpdateBarnRequest request)
        {
            var result = await _billService.AdminUpdateBill(request);
            return Ok(result);
        }

        //[HttpPut("update/breed/{billId}")]
        //public async Task<IActionResult> UpdateBillBreed([FromRoute] Guid billId, [FromBody] UpdateBillBreedDto request)
        //{
        //    var (success, errorMessage) = await _billService.UpdateBillBreed(billId, request);
        //    if (!success)
        //        return BadRequest(new { error = errorMessage });
        //    return Ok(new { message = "Hóa đơn với mặt hàng giống đã được cập nhật thành công." });
        //}
    }
}