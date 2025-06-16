using Domain.Dto.Request;
using Domain.Dto.Response;
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
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateDailyReportRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một báo cáo hàng ngày.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateDailyReportRequest requestDto, CancellationToken cancellationToken = default);

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
    }
}