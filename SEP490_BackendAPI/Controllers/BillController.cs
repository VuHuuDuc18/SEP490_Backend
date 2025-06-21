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

        [HttpPost]
        public async Task<IActionResult> CreateBill([FromBody] CreateBillRequest request)
        {
            var (success, errorMessage) = await _billService.CreateAsync(request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn được tạo thành công." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBill(Guid id, [FromBody] UpdateBillRequest request)
        {
            var (success, errorMessage) = await _billService.UpdateAsync(id, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn được cập nhật thành công." });
        }

        [HttpDelete("items/{billItemId}")]
        public async Task<IActionResult> DeleteBillItem(Guid billItemId)
        {
            var (success, errorMessage) = await _billService.DeleteBillItemAsync(billItemId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mục hóa đơn được xóa thành công." });
        }

        [HttpPatch("{id}/disable")]
        public async Task<IActionResult> DisableBill(Guid id)
        {
            var (success, errorMessage) = await _billService.DisableAsync(id);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn được vô hiệu hóa thành công." });
        }

        [HttpPost("paginated")]
        public async Task<IActionResult> GetPaginatedBills([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _billService.GetPaginatedListAsync(request);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(result);
        }

        [HttpPost("items-paginated")]
        public async Task<IActionResult> GetBillItems(Guid billId, [FromBody] ListingRequest request)
        {
            var (items, errorMessage) = await _billService.GetBillItemsByBillIdAsync(billId, request);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(items);
        }

        [HttpPost("itemType")]
        public async Task<IActionResult> GetBillsByItemTypeAsync(string typeItem, [FromBody] ListingRequest request)
        {
            var (items, errorMessage) = await _billService.GetBillsByItemTypeAsync(request, typeItem);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBillById(Guid id)
        {
            var (bill, errorMessage) = await _billService.GetByIdAsync(id);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            if (bill == null)
                return NotFound(new { error = "Hóa đơn không tồn tại." });
            return Ok(bill);
        }
    }
}