using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Request.Bill.Admin;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IBillService
    {
        //
        Task<Response<bool>> DisableBillItem(
              Guid billItemId,
              CancellationToken cancellationToken = default);

        //
        Task<Response<bool>> DisableBill(
              Guid billId,
              CancellationToken cancellationToken = default);

        //
        Task<Response<PaginationSet<BillItemResponse>>> GetBillItemsByBillId(
           Guid billId,
           ListingRequest request,
           CancellationToken cancellationToken = default);
        //
        Task<Response<BillResponse>> GetBillById(
             Guid billId,
             CancellationToken cancellationToken = default);
        Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetPaginatedBillList(ListingRequest request, CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<BillResponse>>> GetPaginatedBillListHistory(
                    ListingRequest request,
                    string billType,
                    CancellationToken cancellationToken = default);        
        Task<Response<PaginationSet<BillResponse>>> GetBillRequestByType(
             ListingRequest request,
             string billType,
             CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> ChangeBillStatus(Guid billId, string newStatus, CancellationToken cancellationToken = default);
        Task<Response<bool>> RejectBill(
            Guid billId,
            CancellationToken cancellationToken = default);
        Task<Response<bool>> CancelBill(
            Guid billId,
            CancellationToken cancellationToken = default);
        Task<Response<bool>> ApproveBill(
            Guid billId,
            CancellationToken cancellationToken = default);
        Task<Response<bool>> ConfirmBill(
            Guid billId,
            CancellationToken cancellationToken = default);

        //Task<(bool Success, string ErrorMessage)> AddFoodItemToBill(Guid billId, AddFoodItemToBillDto request, CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> AddMedicineItemToBill(Guid billId, AddMedicineItemToBillDto request, CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> AddBreedItemToBill(Guid billId, AddBreedItemToBillDto request, CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> UpdateFoodItemInBill(Guid billId, Guid itemId, UpdateFoodItemInBillDto request, CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> UpdateMedicineItemInBill(Guid billId, Guid itemId, UpdateMedicineItemInBillDto request, CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> UpdateBreedItemInBill(Guid billId, Guid itemId, UpdateBreedItemInBillDto request, CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> DeleteFoodItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> DeleteMedicineItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default);
        //Task<(bool Success, string ErrorMessage)> DeleteBreedItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default);
        Task<Response<bool>> RequestFood(
            CreateFoodRequestDto request,
            CancellationToken cancellationToken = default);
        Task<Response<bool>> RequestMedicine(
               CreateMedicineRequestDto request,
               CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> RequestBreed(CreateBreedRequestDto request, CancellationToken cancellationToken = default);
        Task<Response<bool>> UpdateBillFood(
             UpdateBillFoodDto request,
             CancellationToken cancellationToken = default);
        Task<Response<bool>> UpdateBillMedicine(
             UpdateBillMedicineDto request,
             CancellationToken cancellationToken = default);
        Task<(bool Success, string ErrorMessage)> UpdateBillBreed(Guid billId, UpdateBillBreedDto request, CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<BillResponse>>> GetApprovedBillsByWorker(
            ListingRequest request,
            CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<BillResponse>>> GetHistoryBillsByWorker(
            ListingRequest request,
            CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<BillResponse>>> GetPaginatedBillListByTechicalStaff(
              ListingRequest request,
              string billType,
              CancellationToken cancellationToken = default);
        public Task<bool> AdminUpdateBill(Admin_UpdateBarnRequest request);
    }
}
