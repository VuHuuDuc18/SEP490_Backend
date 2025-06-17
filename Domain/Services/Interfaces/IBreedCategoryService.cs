using Domain.Dto.Request;
using Domain.Dto.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IBreedCategoryService
    {
        /// <summary>
        /// Tạo một danh mục giống mới với kiểm tra hợp lệ.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một danh mục giống.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một danh mục giống bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một danh mục giống theo ID.
        /// </summary>
        Task<(CategoryResponse Category, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả danh mục giống đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<CategoryResponse> Categories, string ErrorMessage)> GetAllAsync(string name = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách phân trang các danh mục con giống với tìm kiếm, lọc và sắp xếp.
        /// </summary>
        Task<(PaginationSet<CategoryResponse> Result, string ErrorMessage)> GetPaginatedListAsync(
            ListingRequest request,
            CancellationToken cancellationToken = default);
    }
}