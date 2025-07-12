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
        [HttpGet("customer/get-pagination-list")]
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
            var data = await _orderService.GetStatisticData(request);
            var SummaryItem = new OrderItem()
            {
                Revenue = data.TotalRevenue,
                BadUnitStockSold = data.TotalBadUnitStockSold,
                GoodUnitStockSold = data.TotalGoodUnitStockSold
            };
            var result = data.datas;
            result.Add(SummaryItem);

            // create excel file
            ExcelPackage package = new ExcelPackage();
           

            using (var excelPackage = new ExcelPackage())
            {
                ExcelPackage.License.SetNonCommercialPersonal("Vu Duc");
                //B2 fill data
                // Thêm sheet với dữ liệu
                ExportExcelHelper.AddSheetToExcelFile(excelPackage, result, "Sold detail", "Danh sách doanh thu từng giống");

                // Lưu file vào thư mục tạm và lấy tên file
                string fileName = ExportExcelHelper.SaveExcelFile(excelPackage, "Company Admin", "Báo cáo doanh thu");

                //B3 xuat data
                if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    return BadRequest("Invalid file name.");

                var path = Path.Combine(Path.GetTempPath(), fileName);
                if (!System.IO.File.Exists(path))
                    return NotFound("File not found.");

                var bytes = System.IO.File.ReadAllBytes(path);
                return File(bytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);
            }
        }
    }
}
