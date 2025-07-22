using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Dto.Response.User;
using Domain.DTOs.Request.Order;
using Domain.DTOs.Response.Order;

namespace Domain.IServices
{
    public interface IOrderService
    {
        Task<Response<string>> CustomerCreateOrder(CreateOrderRequest request, CancellationToken cancellationToken = default);
        Task<Response<OrderResponse>> CustomerOrderDetails(Guid OrderId, CancellationToken cancellationToken = default);
        Task<Response<string>> CustomerUpdateOrder(UpdateOrderRequest request, CancellationToken cancellationToken = default);
        Task<Response<string>> CustomerCancelOrder(Guid OrderId, CancellationToken cancellationToken = default);
        Task<Response<List<OrderResponse>>> CustomerGetAllOrders(CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<OrderResponse>>> CustomerGetPagiantionList(ListingRequest request, CancellationToken cancellationToken = default);
        public Task<Response<StatisticsOrderResponse>> GetStatisticData(StatisticsOrderRequest request);
        public Task<Response<PaginationSet<OrderResponse>>> SaleGetAllOrder(ListingRequest request);
        public Task<Response<bool>> ApproveOrder(ApproveOrderRequest request);
        public Task<Response<PaginationSet<OrderResponse>>> WorkerGetallOrder(ListingRequest request);

    }
}
