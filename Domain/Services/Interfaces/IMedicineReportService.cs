using Domain.Dto.Request;
using Domain.Dto.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IMedicineReportService
    {
        /// <summary>
        /// Tạo một báo cáo thuốc mới và trừ lượng còn lại trong LivestockCircleMedicine.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateMedicineReportRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một báo cáo thuốc và điều chỉnh lượng còn lại trong LivestockCircleMedicine.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateMedicineReportRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một báo cáo thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một báo cáo thuốc theo ID.
        /// </summary>
        Task<(MedicineReportResponse MedicineReport, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả báo cáo thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<MedicineReportResponse> MedicineReports, string ErrorMessage)> GetAllAsync(Guid? medicineId = null, Guid? reportId = null, CancellationToken cancellationToken = default);
    }
}