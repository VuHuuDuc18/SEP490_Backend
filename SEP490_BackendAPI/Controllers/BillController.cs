using System;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Request.Bill.Admin;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillController(IBillService billService)
        {
            _billService = billService ?? throw new ArgumentNullException(nameof(billService)); // Kiểm tra null cho billService
        }

        [HttpPost("request/food")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestFood([FromBody] CreateRequestDto request)
        {
            var (success, errorMessage) = await _billService.RequestFood(request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Yêu cầu Food được tạo thành công." });
        }

        [HttpPost("request/medicine")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestMedicine([FromBody] CreateRequestDto request)
        {
            var (success, errorMessage) = await _billService.RequestMedicine(request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Yêu cầu Medicine được tạo thành công." });
        }

        [HttpPost("request/breed")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestBreed([FromBody] CreateRequestDto request)
        {
            var (success, errorMessage) = await _billService.RequestBreed(request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Yêu cầu Breed được tạo thành công." });
        }

        [HttpPost("addItem/{billId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddItemToBill(Guid billId, [FromBody] CreateBillItemRequest item)
        {
            var (success, errorMessage) = await _billService.AddItemToBill(billId, item);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Item đã được thêm vào hóa đơn thành công." });
        }

        [HttpPut("updateItem/{billId}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemInBill(Guid billId, Guid itemId, [FromBody] CreateBillItemRequest item)
        {
            var (success, errorMessage) = await _billService.UpdateItemInBill(billId, itemId, item);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Item trong hóa đơn đã được cập nhật thành công." });
        }

        [HttpDelete("deleteItem/{billId}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteItemFromBill(Guid billId, Guid itemId)
        {
            var (success, errorMessage) = await _billService.DeleteItemFromBill(billId, itemId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Item đã được xóa khỏi hóa đơn thành công." });
        }

        //[HttpDelete("billItems/{billItemId}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> DisableBillItem(Guid billItemId)
        //{
        //    var (success, errorMessage) = await _billService.DisableBillItem(billItemId);
        //    if (!success)
        //        return BadRequest(new { error = errorMessage });
        //    return Ok(new { message = "Mục hóa đơn được xóa thành công." });
        //}

        [HttpPatch("disable/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisableBill(Guid billId)
        {
            var (success, errorMessage) = await _billService.DisableBill(billId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn được vô hiệu hóa thành công." });
        }

        [HttpPost("getBillItemsByBillId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBillItems(Guid billId, [FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _billService.GetBillItemsByBillId(billId, request);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(result);
        }

        [HttpGet("getBillById/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBillById(Guid billId)
        {
            var (bill, errorMessage) = await _billService.GetBillById(billId);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            if (bill == null)
                return NotFound(new { error = "Hóa đơn không tồn tại." });
            return Ok(bill);
        }
        [HttpPost("admin/updateBill")]
        public async Task<IActionResult> UpdateBill([FromRoute]Admin_UpdateBarnRequest request)
        {
            var result = _billService.AdminUpdateBill(request);
            return Ok(result);
        }

        [HttpPost("getPaginatedBillList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPaginatedBills([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _billService.GetPaginatedBillList(request);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(result);
        }

        [HttpPost("getBillsByItemType")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBillsByItemTypeAsync(string itemType, [FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _billService.GetBillsByItemType(request, itemType);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(result);
        }

      

        [HttpPatch("changeStatus/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeBillStatus(Guid billId, [FromQuery] string newStatus)
        {
            var (success, errorMessage) = await _billService.ChangeBillStatus(billId, newStatus);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = $"Trạng thái hóa đơn đã được thay đổi thành {newStatus} thành công." });
        }


    }
}