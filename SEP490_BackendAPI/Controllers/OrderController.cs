using Domain.DTOs.Request.Order;
using Domain.IServices;
using Microsoft.AspNetCore.Http;
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
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
        {
            return Ok(await _orderService.CreateOrder(request));
        }
        [HttpGet("view-order-details")]
        public async Task<IActionResult> ViewOrderDetails(Guid orderId)
        {
            return Ok(await _orderService.ViewOrderDetails(orderId));
        }
        [HttpPut("update-order")]
        public async Task<IActionResult> UpdateOrder(UpdateOrderRequest request)
        {
            return Ok(await _orderService.UpdateOrder(request));
        }
        [HttpPut("cancel-order/{orderId}")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            return Ok(await _orderService.CancelOrder(orderId));
        }
    }
}
