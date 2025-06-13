using Domain.Dto.Request;
using Domain.Dto.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IMedicineService
    {
        /// <summary>
        /// Tạo một loại thuốc mới với kiểm tra hợp lệ.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateMedicineRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một loại thuốc.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateMedicineRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một loại thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một loại thuốc theo ID.
        /// </summary>
        Task<(MedicineResponse Medicine, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả loại thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<MedicineResponse> Medicines, string ErrorMessage)> GetAllAsync(string medicineName = null, Guid? medicineCategoryId = null, CancellationToken cancellationToken = default);
    }
}