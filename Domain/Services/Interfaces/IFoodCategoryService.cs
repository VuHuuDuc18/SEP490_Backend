using Domain.Dto.Request;
using Domain.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IFoodCategoryService
    {
        /// <summary>
        /// Tạo một danh mục thức ăn mới với kiểm tra hợp lệ.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một danh mục thức ăn.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một danh mục thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một danh mục thức ăn theo ID.
        /// </summary>
        Task<(CategoryResponse Category, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả danh mục thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<CategoryResponse> Categories, string ErrorMessage)> GetAllAsync(string name = null, CancellationToken cancellationToken = default);
    }
}
