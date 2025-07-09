using Domain.Dto.Request;
using Domain.DTOs.Request.Order;
using Domain.IServices;
using Microsoft.AspNetCore.Mvc;

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
    }
}
