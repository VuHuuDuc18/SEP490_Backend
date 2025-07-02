using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Request.Bill.Admin;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> RequestFood([FromBody] CreateFoodRequestDto request)
        {
            var (success, errorMessage) = await _billService.RequestFood(request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Yêu cầu Food được tạo thành công." });
        }

        [HttpPost("request/medicine")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestMedicine([FromBody] CreateMedicineRequestDto request)
        {
            var (success, errorMessage) = await _billService.RequestMedicine(request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Yêu cầu Medicine được tạo thành công." });
        }

        [HttpPost("request/breed")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestBreed([FromBody] CreateBreedRequestDto request)
        {
            var (success, errorMessage) = await _billService.RequestBreed(request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Yêu cầu Breed được tạo thành công." });
        }

        [HttpPost("add/food-item/{billId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddFoodItemToBill([FromRoute] Guid billId, [FromBody] AddFoodItemToBillDto request)
        {
            var (success, errorMessage) = await _billService.AddFoodItemToBill(billId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng thức ăn đã được thêm vào hóa đơn thành công." });
        }

        [HttpPost("add/medicine-item/{billId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddMedicineItemToBill([FromRoute] Guid billId, [FromBody] AddMedicineItemToBillDto request)
        {
            var (success, errorMessage) = await _billService.AddMedicineItemToBill(billId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng thuốc đã được thêm vào hóa đơn thành công." });
        }

        [HttpPost("add/breed-item/{billId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddBreedItemToBill([FromRoute] Guid billId, [FromBody] AddBreedItemToBillDto request)
        {
            var (success, errorMessage) = await _billService.AddBreedItemToBill(billId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng giống đã được thêm vào hóa đơn thành công." });
        }

        [HttpPut("update/food-item/{billId}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateFoodItemInBill([FromRoute] Guid billId, [FromRoute] Guid itemId, [FromBody] UpdateFoodItemInBillDto request)
        {
            var (success, errorMessage) = await _billService.UpdateFoodItemInBill(billId, itemId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng thức ăn trong hóa đơn đã được cập nhật thành công." });
        }

        [HttpPut("update/medicine-item/{billId}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMedicineItemInBill([FromRoute] Guid billId, [FromRoute] Guid itemId, [FromBody] UpdateMedicineItemInBillDto request)
        {
            var (success, errorMessage) = await _billService.UpdateMedicineItemInBill(billId, itemId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng thuốc trong hóa đơn đã được cập nhật thành công." });
        }

        [HttpPut("update/breed-item/{billId}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBreedItemInBill([FromRoute] Guid billId, [FromRoute] Guid itemId, [FromBody] UpdateBreedItemInBillDto request)
        {
            var (success, errorMessage) = await _billService.UpdateBreedItemInBill(billId, itemId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng giống trong hóa đơn đã được cập nhật thành công." });
        }

        [HttpDelete("delete/food-item/{billId}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteFoodItemFromBill([FromRoute] Guid billId, [FromRoute] Guid itemId)
        {
            var (success, errorMessage) = await _billService.DeleteFoodItemFromBill(billId, itemId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng thức ăn đã được xóa khỏi hóa đơn thành công." });
        }

        [HttpDelete("delete/medicine-item/{billId}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMedicineItemFromBill([FromRoute] Guid billId, [FromRoute] Guid itemId)
        {
            var (success, errorMessage) = await _billService.DeleteMedicineItemFromBill(billId, itemId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng thuốc đã được xóa khỏi hóa đơn thành công." });
        }

        [HttpDelete("delete/breed-item/{billId}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteBreedItemFromBill([FromRoute] Guid billId, [FromRoute] Guid itemId)
        {
            var (success, errorMessage) = await _billService.DeleteBreedItemFromBill(billId, itemId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mặt hàng giống đã được xóa khỏi hóa đơn thành công." });
        }

        [HttpPatch("disable/bill-item/{billItemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisableBillItem([FromRoute] Guid billItemId)
        {
            var (success, errorMessage) = await _billService.DisableBillItem(billItemId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Mục hóa đơn được vô hiệu hóa thành công." });
        }

        [HttpPatch("disable/bill/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisableBill([FromRoute] Guid billId)
        {
            var (success, errorMessage) = await _billService.DisableBill(billId);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn được vô hiệu hóa thành công." });
        }

        [HttpPost("get-bill-items/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBillItemsByBillId([FromRoute] Guid billId, [FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _billService.GetBillItemsByBillId(billId, request);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(result);
        }

        [HttpGet("get-bill/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBillById([FromRoute] Guid billId)
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

        [HttpPost("get-paginated-bills")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPaginatedBills([FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _billService.GetPaginatedBillList(request);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(result);
        }

        [HttpPost("get-bills-by-type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBillsByItemTypeAsync(string itemType, [FromBody] ListingRequest request)
        {
            var (result, errorMessage) = await _billService.GetBillsByItemType(request, itemType);
            if (errorMessage != null)
                return BadRequest(new { error = errorMessage });
            return Ok(result);
        }

        [HttpPatch("change-status/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeBillStatus([FromRoute] Guid billId, [FromQuery] string newStatus)
        {
            var (success, errorMessage) = await _billService.ChangeBillStatus(billId, newStatus);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = $"Trạng thái hóa đơn đã được thay đổi thành {newStatus} thành công." });
        }

        [HttpPut("update/food/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBillFood([FromRoute] Guid billId, [FromBody] UpdateBillFoodDto request)
        {
            var (success, errorMessage) = await _billService.UpdateBillFood(billId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn với mặt hàng thức ăn đã được cập nhật thành công." });
        }

        [HttpPut("update/medicine/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBillMedicine([FromRoute] Guid billId, [FromBody] UpdateBillMedicineDto request)
        {
            var (success, errorMessage) = await _billService.UpdateBillMedicine(billId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn với mặt hàng thuốc đã được cập nhật thành công." });
        }

        [HttpPut("update/breed/{billId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBillBreed([FromRoute] Guid billId, [FromBody] UpdateBillBreedDto request)
        {
            var (success, errorMessage) = await _billService.UpdateBillBreed(billId, request);
            if (!success)
                return BadRequest(new { error = errorMessage });
            return Ok(new { message = "Hóa đơn với mặt hàng giống đã được cập nhật thành công." });
        }
    }
}