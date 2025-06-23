using System;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
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
            _billService = billService ?? throw new ArgumentNullException(nameof(billService));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBill([FromBody] CreateBillRequest request)
        {
            var (success, errorMessage) = await _billService.CreateBill(request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn được tạo thành công." });
        }

        [HttpPut("update/{billId}")]
        public async Task<IActionResult> UpdateBill(Guid billId, [FromBody] UpdateBillRequest request)
        {
            var (success, errorMessage) = await _billService.UpdateBill(billId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn được cập nhật thành công." });
        }

        [HttpDelete("billItems/{billItemId}")]
        public async Task<IActionResult> DisbaleBillItem(Guid billItemId)
        {
            var (success, errorMessage) = await _billService.DisableBillItem(billItemId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mục hóa đơn được xóa thành công." });
        }

        [HttpPatch("disable/{billId}")]
        public async Task<IActionResult> DisableBill(Guid billId)
        {
            var (success, errorMessage) = await _billService.DisableBill(billId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn được vô hiệu hóa thành công." });
        }

        [HttpPost("getPaginatedBillList")]
        public async Task<IActionResult> GetPaginatedBills([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _billService.GetPaginatedBillList(request);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(result);
        }

        [HttpPost("getBillItemsByBillId")]
        public async Task<IActionResult> GetBillItems(Guid billId, [FromBody] ListingRequest request)
        {
            var (items, errorMessage) = await _billService.GetBillItemsByBillId(billId, request);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(items);
        }

        [HttpPost("getBillsByItemType")]
        public async Task<IActionResult> GetBillsByItemTypeAsync(string typeItem, [FromBody] ListingRequest request)
        {
            var (items, errorMessage) = await _billService.GetBillsByItemType(request, typeItem);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(items);
        }

        [HttpGet("getBillById/{billId}")]
        public async Task<IActionResult> GetBillById(Guid billId)
        {
            var (bill, errorMessage) = await _billService.GetBillById(billId);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            if (bill == null)
                return NotFound(new { error = "Hóa đơn không tồn tại." });
            return Ok(bill);
        }
    }
}