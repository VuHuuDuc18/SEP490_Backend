using Application.Wrappers;
using Domain.DTOs.Request.Order;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Domain.Helper.Constants;

namespace Infrastructure.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly Guid _currentUserId;
        public OrderService
        (
            IRepository<Order> orderRepository,
            IHttpContextAccessor httpContextAccessor,
            IRepository<LivestockCircle> livestockCircleRepository
        )
        {
            _orderRepository = orderRepository;
            _livestockCircleRepository = livestockCircleRepository;

            // Lấy current user từ JWT token claims
            _currentUserId = Guid.Empty;
            var currentUser = httpContextAccessor.HttpContext?.User;
            if (currentUser != null)
            {
                var userIdClaim = currentUser.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    _currentUserId = Guid.Parse(userIdClaim);
                }
            }
        }

        public async Task<Response<string>> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            if (_currentUserId == Guid.Empty) { 
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Hãy đăng nhập và thử lại",
                    Errors = new List<string>() { "Hãy đăng nhập và thử lại" }
                };
            }
            if (request.GoodUnitStock <= 0 || request.BadUnitStock < 0)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Số lượng con tốt hoặc con xấu phải lớn hơn 0",
                    Errors = new List<string>() { "Số lượng con tốt hoặc con xấu phải lớn hơn 0" }
                };
            }
            if (request.PickupDate < DateTime.UtcNow)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại",
                        Errors = new List<string>() { "Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại" }
                    };
                }
            try
            {
                
                var order = new Order()
                {
                    CustomerId = _currentUserId,
                    LivestockCircleId = request.LivestockCircleId,
                    GoodUnitStock = request.GoodUnitStock,
                    BadUnitStock = request.BadUnitStock,
                    Status = OrderStatus.PENDING,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = _currentUserId,
                    IsActive = true,
                    PickupDate = request.PickupDate
                };
                var livestockCircle = await _livestockCircleRepository.GetById(request.LivestockCircleId);
                if (livestockCircle == null)
                {
                    return new Response<string>("Lỗi khi tạo đơn.")
                    {
                        Errors = new List<string>()
                        {
                            "Chu kì nuôi không tồn tại."
                        }
                    };
                }
                if (order.GoodUnitStock >= livestockCircle.GoodUnitNumber || order.BadUnitStock>= livestockCircle.BadUnitNumber)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Số lượng con tốt hoặc con xấu vượt quá số lượng con trong chu kì nuôi.",
                        Errors = new List<string>() { "Số lượng con tốt hoặc con xấu vượt quá số lượng con trong chu kì nuôi." }
                    };
                }
                if (((DateTime)order.PickupDate - (DateTime)livestockCircle.ReleaseDate).Days > 3)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Ngày lấy hàng phải trong vòng 3 ngày kể từ ngày xuất chuồng",
                        Errors = new List<string>() { "Ngày lấy hàng phải trong vòng 3 ngày kể từ ngày xuất chuồng" }
                    };
                }
                _orderRepository.Insert(order);
                await _orderRepository.CommitAsync(cancellationToken);
                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo đơn hàng thành công",
                    Data = "Đơn hàng đã được tạo thành công. ID: " + order.Id
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo đơn hàng",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public Task<Response<ViewOrderDetailsResponse>> ViewOrderDetails(Guid OrderId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Response<string>> UpdateOrder(UpdateOrderRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Response<string>> CancelOrder(Guid OrderId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
