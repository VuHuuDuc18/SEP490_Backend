using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Food;
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
        Task<(bool Success, string ErrorMessage)> CreateFoodCategory(CreateCategoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một danh mục thức ăn.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateFoodCategory(Guid FoodCategoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một danh mục thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DisableFoodCategory(Guid FoodCategoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một danh mục thức ăn theo ID.
        /// </summary>
        Task<(CategoryResponse Category, string ErrorMessage)> GetFoodCategoryById(Guid FoodCategoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả danh mục thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<CategoryResponse> Categories, string ErrorMessage)> GetFoodCategoryByName(string name = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách phân trang các danh mục thức ăn với tìm kiếm, lọc và sắp xếp.
        /// </summary>
        Task<(PaginationSet<CategoryResponse> Result, string ErrorMessage)> GetPaginatedFoodCategoryList(
            ListingRequest request,
            CancellationToken cancellationToken = default);

        public Task<List<FoodCategoryResponse>> GetAllCategory();
    }
}
