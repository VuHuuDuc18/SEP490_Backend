using Domain.Dto.Request;
using Domain.Dto.Request.DailyReport;
using Domain.Dto.Response;
using Domain.Dto.Response.DailyReport;
using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IDailyReportService
    {
        /// <summary>
        /// Tạo một báo cáo hàng ngày mới với kiểm tra hợp lệ.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateDailyReport(CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một báo cáo hàng ngày.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateDailyReport(Guid dailyReportId, UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một báo cáo hàng ngày bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DisableDailyReport(Guid dailyReportId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một báo cáo hàng ngày theo ID, bao gồm danh sách FoodReport và MedicineReport.
        /// </summary>
        Task<(DailyReportResponse DailyReport, string ErrorMessage)> GetDailyReportById(Guid dailyReportId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả báo cáo hàng ngày đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách FoodReport và MedicineReport.
        /// </summary>
        Task<(List<DailyReportResponse> DailyReports, string ErrorMessage)> GetDailyReportByLiveStockCircle(Guid? livestockCircleId = null, CancellationToken cancellationToken = default);

        Task<(PaginationSet<DailyReportResponse> Result, string ErrorMessage)> GetPaginatedDailyReportListByLiveStockCircle(
               Guid livestockCircleId,
               ListingRequest request,
               CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả thức ăn trong báo cáo hàng ngày.
        /// </summary>
        Task<(PaginationSet<FoodReportResponse> Result, string ErrorMessage)> GetFoodReportDetails(
              Guid reportId,
              ListingRequest request,
              CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả thức ăn trong báo cáo hàng ngày.
        /// </summary>
        Task<(PaginationSet<MedicineReportResponse> Result, string ErrorMessage)> GetMedicineReportDetails(
                  Guid reportId,
                  ListingRequest request,
                  CancellationToken cancellationToken = default);
        /// <summary>
        /// Lấy danh sách phân trang tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(PaginationSet<DailyReportResponse> Result, string ErrorMessage)> GetPaginatedDailyReportList(
                ListingRequest request,
                CancellationToken cancellationToken = default);

        Task<(bool HasReport, string ErrorMessage)> HasDailyReportToday(
            Guid livestockCircleId, CancellationToken cancellationToken = default);

        Task<(DailyReportResponse DailyReport, string ErrorMessage)> GetTodayDailyReport(
            Guid livestockCircleId, CancellationToken cancellationToken = default);
    }
}