using Domain.Dto.Request;
using Domain.DTOs.Request.Order;
using Domain.DTOs.Response.Order;
using Domain.IServices;
using ExcelControl.Handler;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace SEP490_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost("customer/create-order")]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
        {
            return Ok(await _orderService.CustomerCreateOrder(request));
        }
        [HttpGet("customer/view-order-details")]
        public async Task<IActionResult> ViewOrderDetails(Guid orderId)
        {
            return Ok(await _orderService.CustomerOrderDetails(orderId));
        }
        [HttpPut("customer/update-order")]
        public async Task<IActionResult> UpdateOrder(UpdateOrderRequest request)
        {
            return Ok(await _orderService.CustomerUpdateOrder(request));
        }
        [HttpPut("customer/cancel-order")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            return Ok(await _orderService.CustomerCancelOrder(orderId));
        }
        [HttpGet("customer/get-all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            return Ok(await _orderService.CustomerGetAllOrders());
        }
        [HttpPost("customer/get-pagination-list")]
        public async Task<IActionResult> GetPaginationList(ListingRequest request)
        {
            return Ok(await _orderService.CustomerGetPagiantionList(request));
        }
        [HttpPost("admin/get-statistic")]
        public async Task<IActionResult> GetStatisticList([FromBody]StatisticsOrderRequest request)
        {
            var result = await _orderService.GetStatisticData(request);
            return Ok(result);
        }
        [HttpPost("admin/export")]
        public async Task<IActionResult> ExportStatistic([FromBody] StatisticsOrderRequest request)
        {
            try
            {
                var data = await _orderService.GetStatisticData(request);

                var summaryItem = new OrderItem()
                {
                    Revenue = data.Data.TotalRevenue,
                    BadUnitStockSold = data.Data.TotalBadUnitStockSold,
                    GoodUnitStockSold = data.Data.TotalGoodUnitStockSold,
                    BreedName = "Total"
                };

                var result = data.Data.Datas;
                result.Add(summaryItem);

                // Thiết lập license context cho EPPlus (phiên bản mới)
                ExcelPackage.License.SetNonCommercialPersonal("Company Admin");

                using (var excelPackage = new ExcelPackage())
                {
                    // Thêm sheet và dữ liệu vào file excel
                    ExportExcelHelper.AddSheetToExcelFile(excelPackage, result, "Sold detail", "Danh sách doanh thu từng giống");

                    // Lưu file excel vào thư mục temp và lấy tên file
                    string fileName = ExportExcelHelper.SaveExcelFile(excelPackage, "Company Admin", "Báo cáo doanh thu");

                    if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                        return BadRequest("Invalid file name.");

                    var filePath = Path.Combine(Path.GetTempPath(), fileName);

                    if (!System.IO.File.Exists(filePath))
                        return NotFound("File not found.");

                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                    // Trả về file excel cho client download
                    return File(fileBytes,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                fileName);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                // _logger.LogError(ex, "Error exporting statistic");

                return StatusCode(500, "An error occurred while exporting the statistic.");
            }
        }
        [HttpPost("sale/get-order-list")]
        public async Task<IActionResult> GetOrderList(ListingRequest request)
        {
            return Ok(await _orderService.SaleGetAllOrder(request));
        }
        [HttpPost("worker/worker-get-order-list")]
        public async Task<IActionResult> WorderGetOrderList(ListingRequest request)
        {
            return Ok(await _orderService.WorkerGetallOrder(request));
        }
        [HttpPut("sale/approve-order")]
        public async Task<IActionResult> ApproveOrder(ApproveOrderRequest request)
        {
            return Ok(await _orderService.ApproveOrder(request));
        }
        [HttpPut("sale/deny-order/{orderId}")]
        public async Task<IActionResult> DenyOrder(Guid orderId)
        {
            return Ok(await _orderService.DenyOrder(orderId));
        }

    }
}
