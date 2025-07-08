using Domain.Dto.Request;
using Domain.Dto.Request.Medicine;
using Domain.Dto.Response;
using Domain.Dto.Response.Medicine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IMedicineService
    {
        /// <summary>
        /// Tạo một loại thuốc mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateMedicine(CreateMedicineRequest request,  CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một loại thuốc, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateMedicine(Guid MedicineId, UpdateMedicineRequest request,  CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một loại thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DisableMedicine(Guid MedicineId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một loại thuốc theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(MedicineResponse Medicine, string ErrorMessage)> GetMedicineById(Guid MedicineId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả loại thuốc đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(List<MedicineResponse> Medicines, string ErrorMessage)> GetMedicineByCategory(string medicineName = null, Guid? medicineCategoryId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách phân trang tìm kiếm lọc tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(PaginationSet<MedicineResponse> Result, string ErrorMessage)> GetPaginatedMedicineList(
           ListingRequest request,
           CancellationToken cancellationToken = default);

        public Task<bool> ExcelDataHandle(List<CellMedicineItem> data);


    }
}