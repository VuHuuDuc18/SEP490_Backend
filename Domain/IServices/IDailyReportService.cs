using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.DailyReport;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using Domain.Dto.Response.DailyReport;
using Domain.Dto.Response.Food;
using Domain.Dto.Response.Medicine;
using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IDailyReportService
    {
        Task<Response<string>> CreateDailyReport(CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default);

        Task<Response<string>> UpdateDailyReport(UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default);

        Task<Response<string>> DisableDailyReport(Guid dailyReportId, CancellationToken cancellationToken = default);
        Task<Response<DailyReportResponse>> GetDailyReportById(Guid dailyReportId, CancellationToken cancellationToken = default);
        Task<(List<DailyReportResponse> DailyReports, string ErrorMessage)> GetDailyReportByLiveStockCircle(Guid? livestockCircleId = null, CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<DailyReportResponse>>> GetPaginatedDailyReportList(
           ListingRequest request,
           Guid? livestockCircleId = null,
           CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<FoodReportResponse>>> GetFoodReportDetails(
             Guid reportId,
             ListingRequest request,
             CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<MedicineReportResponse>>> GetMedicineReportDetails(
                   Guid reportId,
                   ListingRequest request,
                   CancellationToken cancellationToken = default);
        Task<Response<bool>> HasDailyReportToday(
                 Guid livestockCircleId,
                 CancellationToken cancellationToken = default);

        Task<Response<DailyReportResponse>> GetTodayDailyReport(
            Guid livestockCircleId,
            CancellationToken cancellationToken = default);

        Task<List<FoodResponse>> GetAllFoodRemainingOfLivestockCircle(
             Guid livestockCircleId,
            CancellationToken cancellationToken = default
            );

        Task<List<MedicineResponse>> GetAllMedicineRemainingOfLivestockCircle(
             Guid livestockCircleId,
            CancellationToken cancellationToken = default
            );

    }
}