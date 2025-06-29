using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Request.Bill.Admin;

namespace Domain.Services.Interfaces
{
    public interface IBillService
    {

        Task<(bool Success, string ErrorMessage)> CreateBill(
            CreateBillRequest request,
            CancellationToken cancellationToken = default);

        Task<(bool Success, string ErrorMessage)> UpdateBill(
            Guid billId,
            UpdateBillRequest request,
            CancellationToken cancellationToken = default);

        Task<(bool Success, string ErrorMessage)> DisableBillItem(Guid billItemId, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> DisableBill(Guid billId, CancellationToken cancellationToken = default);
        Task<(PaginationSet<BillItemResponse> Result, string ErrorMessage)> GetBillItemsByBillId(Guid billId, ListingRequest request, CancellationToken cancellationToken = default);
        Task<(BillResponse Bill, string ErrorMessage)> GetBillById(Guid billId, CancellationToken cancellationToken = default);
        Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetPaginatedBillList(ListingRequest request, CancellationToken cancellationToken = default);
        Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetBillsByItemType(ListingRequest request, string itemType, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> ChangeBillStatus(Guid billId, string newStatus, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> AddItemToBill(Guid billId, CreateBillItemRequest item, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> UpdateItemInBill(Guid billId, Guid itemId, CreateBillItemRequest item, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> DeleteItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> RequestFood(CreateRequestDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> RequestMedicine(CreateRequestDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> RequestBreed(CreateRequestDto request, CancellationToken cancellationToken = default);
        public Task<bool> AdminUpdateBill(Admin_UpdateBarnRequest request);


    }
}