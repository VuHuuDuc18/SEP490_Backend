using Domain.Dto.Request.DailyReport;
using Domain.Dto.Response.DailyReport;
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
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một báo cáo hàng ngày.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một báo cáo hàng ngày bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một báo cáo hàng ngày theo ID, bao gồm danh sách FoodReport và MedicineReport.
        /// </summary>
        Task<(DailyReportResponse DailyReport, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả báo cáo hàng ngày đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách FoodReport và MedicineReport.
        /// </summary>
        Task<(List<DailyReportResponse> DailyReports, string ErrorMessage)> GetAllAsync(Guid? livestockCircleId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả thức ăn trong báo cáo hàng ngày.
        /// </summary>
        Task<(List<FoodReportResponse> FoodReports, string ErrorMessage)> GetFoodReportDetailsAsync(Guid reportId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả thức ăn trong báo cáo hàng ngày.
        /// </summary>
        Task<(List<MedicineReportResponse> MedicineReports, string ErrorMessage)> GetMedicineReportDetailsAsync(Guid reportId, CancellationToken cancellationToken = default);
    }
}