using Application.Wrappers;
using Domain.DTOs.Request.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IOrderService
    {
        Task<Response<string>> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken = default);
        Task<Response<ViewOrderDetailsResponse>> ViewOrderDetails(Guid OrderId, CancellationToken cancellationToken = default);
        Task<Response<string>> UpdateOrder(UpdateOrderRequest request, CancellationToken cancellationToken = default);
        Task<Response<string>> CancelOrder(Guid OrderId, CancellationToken cancellationToken = default);
    }
}
