using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Request.Bill.Admin;

namespace Domain.Services.Interfaces
{
    public interface IBillService
    {

        //Task<(bool Success, string ErrorMessage)> CreateBill(
        //    CreateBillRequest request,
        //    CancellationToken cancellationToken = default);

        //Task<(bool Success, string ErrorMessage)> UpdateBill(
        //    Guid billId,
        //    UpdateBillRequest request,
        //    CancellationToken cancellationToken = default);

        Task<(bool Success, string ErrorMessage)> DisableBillItem(Guid billItemId, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> DisableBill(Guid billId, CancellationToken cancellationToken = default);
        Task<(PaginationSet<BillItemResponse> Result, string ErrorMessage)> GetBillItemsByBillId(Guid billId, ListingRequest request, CancellationToken cancellationToken = default);
        Task<(BillResponse Bill, string ErrorMessage)> GetBillById(Guid billId, CancellationToken cancellationToken = default);
        Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetPaginatedBillList(ListingRequest request, CancellationToken cancellationToken = default);
        Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetBillsByItemType(ListingRequest request, string itemCategory, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> ChangeBillStatus(Guid billId, string newStatus, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> AddFoodItemToBill(Guid billId, AddFoodItemToBillDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> AddMedicineItemToBill(Guid billId, AddMedicineItemToBillDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> AddBreedItemToBill(Guid billId, AddBreedItemToBillDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> UpdateFoodItemInBill(Guid billId, Guid itemId, UpdateFoodItemInBillDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> UpdateMedicineItemInBill(Guid billId, Guid itemId, UpdateMedicineItemInBillDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> UpdateBreedItemInBill(Guid billId, Guid itemId, UpdateBreedItemInBillDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> DeleteFoodItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> DeleteMedicineItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> DeleteBreedItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> RequestFood(CreateFoodRequestDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> RequestMedicine(CreateMedicineRequestDto request, CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> RequestBreed(CreateBreedRequestDto request, CancellationToken cancellationToken = default);
        public Task<bool> AdminUpdateBill(Admin_UpdateBarnRequest request);
    }
}
