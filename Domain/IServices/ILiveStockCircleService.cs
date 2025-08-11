using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.LivestockCircle;
using Domain.Dto.Response;
using Domain.Dto.Response.LivestockCircle;
using Domain.DTOs.Request.LivestockCircle;
using Domain.DTOs.Response.LivestockCircle;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface ILivestockCircleService
    {
        public Task<Response<Guid>> CreateLiveStockCircle(CreateLivestockCircleRequest request);

        //Task<(bool Success, string ErrorMessage)> UpdateLiveStockCircle(
        //    Guid livestockCircleId, 
        //    UpdateLivestockCircleRequest request, 
        //    CancellationToken cancellationToken = default);

        Task<(bool Success, string ErrorMessage)> DisableLiveStockCircle(
            Guid livestockCircleId, 
            CancellationToken cancellationToken = default);

        Task<(LivestockCircleResponse Circle, string ErrorMessage)> GetLiveStockCircleById(
            Guid livestockCircleId, 
            CancellationToken cancellationToken = default);

        //Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetLiveStockCircleByBarnIdAndStatus(
        //    string status = null, 
        //    Guid? barnId = null, 
        //    CancellationToken cancellationToken = default);

        //Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetLiveStockCircleByTechnicalStaff(
        //    Guid technicalStaffId, 
        //    CancellationToken cancellationToken = default);

        //Task<(PaginationSet<LivestockCircleResponse> Result, string ErrorMessage)> GetPaginatedListByStatus(
        //    string status,
        //    ListingRequest request,
        //    CancellationToken cancellationToken = default);

        Task<(bool Success, string ErrorMessage)> UpdateAverageWeight(
            Guid livestockCircleId,
            float averageWeight,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Thay đổi trạng thái (Status) của một chu kỳ chăn nuôi.
        /// </summary>
        //Task<(bool Success, string ErrorMessage)> ChangeStatus(
        //    Guid livestockCircleId,
        //    string status,
        //    CancellationToken cancellationToken = default);

        //Lay lua chan nuoi hien tai cua chuong
        public Task<(LiveStockCircleActive Circle, string ErrorMessage)> GetActiveLiveStockCircleByBarnId(
           Guid barnId,
           CancellationToken cancellationToken = default);

        //danh sach thuc an con trong chuong
        Task<Response<PaginationSet<FoodRemainingResponse>>> GetFoodRemaining(
               Guid liveStockCircleId,
               ListingRequest request,
               CancellationToken cancellationToken = default);

        //danh sach thuoc con trong chuong
        Task<Response<PaginationSet<MedicineRemainingResponse>>> GetMedicineRemaining(
             Guid liveStockCircleId,
             ListingRequest request,
             CancellationToken cancellationToken = default);

        // cap nhat trang thai xuat chuong cho 1 lua nuoi
        public Task<Response<bool>> ReleaseBarn(ReleaseBarnRequest req);
        //lay danh sach cac chuong duoc giao quan ly
        //public Task<PaginationSet<LivestockCircleResponse>> GetAssignedBarn(Guid tsid,ListingRequest req);
        // danh sach lich su chan nuoi cua 1 chuong
        public Task<Response<PaginationSet<LiveStockCircleHistoryItem>>> GetLivestockCircleHistory(Guid barnId,ListingRequest req);

        public Task<Response<PaginationSet<ReleasedLivetockItem>>> GetReleasedLivestockCircleList(ListingRequest req);
        //public Task<Response<ReleasedLivetockDetail>> GetReleasedLivestockCircleById(Guid livestockCircleId);

        public Task<(bool Success, string ErrorMessage)> UpdateImageLiveStocCircle(
            Guid livestockCircleId,
            UpdateImageLiveStockCircle request,
            CancellationToken cancellationToken = default);
        Task<Response<string>> SetPreOrderField(SetPreOrderFieldRequest request);
    }
}