using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Category;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IMedicineCategoryService
    {
        /// <summary>
        /// Tạo một danh mục thuốc mới với kiểm tra hợp lệ.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateMedicineCategory(CreateCategoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một danh mục thuốc.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateMedicineCategory(Guid MedicineCategoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một danh mục thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DisableMedicineCategory(Guid MedicineCategoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một danh mục thuốc theo ID.
        /// </summary>
        Task<(CategoryResponse Category, string ErrorMessage)> GetMedicineCategoryById(Guid MedicineCategoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả danh mục thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<CategoryResponse> Categories, string ErrorMessage)> GetMedicineCategoryByName(string name = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách phân trang các danh mục thuốc với tìm kiếm, lọc và sắp xếp.
        /// </summary>
        Task<(PaginationSet<CategoryResponse> Result, string ErrorMessage)> GetPaginatedMedicineCategoryList(
            ListingRequest request,
            CancellationToken cancellationToken = default);
    }
}