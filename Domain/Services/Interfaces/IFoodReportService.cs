using Domain.Dto.Request;
using Domain.Dto.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IFoodReportService
    {
        /// <summary>
        /// Tạo một báo cáo thức ăn mới và trừ lượng còn lại trong LivestockCircleFood.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateFoodReportRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một báo cáo thức ăn và điều chỉnh lượng còn lại trong LivestockCircleFood.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateFoodReportRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một báo cáo thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một báo cáo thức ăn theo ID.
        /// </summary>
        Task<(FoodReportResponse FoodReport, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả báo cáo thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<FoodReportResponse> FoodReports, string ErrorMessage)> GetAllAsync(Guid? foodId = null, Guid? reportId = null, CancellationToken cancellationToken = default);
    }
}