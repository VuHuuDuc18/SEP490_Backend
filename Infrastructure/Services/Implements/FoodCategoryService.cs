
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Extensions;

using Domain.Dto.Response.Food;
using Domain.Dto.Response.Medicine;


namespace Infrastructure.Services.Implements
{
    public class FoodCategoryService : IFoodCategoryService
    {
        private readonly IRepository<FoodCategory> _foodCategoryRepository;

        /// <summary>
        /// Khởi tạo service với repository của FoodCategory.
        /// </summary>
        public FoodCategoryService(IRepository<FoodCategory> foodCategoryRepository)
        {
            _foodCategoryRepository = foodCategoryRepository ?? throw new ArgumentNullException(nameof(foodCategoryRepository));
        }

        /// <summary>
        /// Tạo một danh mục thức ăn mới với kiểm tra hợp lệ.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateFoodCategory(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu danh mục thức ăn không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xem danh mục với tên này đã tồn tại chưa
            var checkError = new Ref<CheckError>();
            var exists = await _foodCategoryRepository.CheckExist(
                x => x.Name == request.Name && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra danh mục tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Danh mục thức ăn với tên '{request.Name}' đã tồn tại.");

            var foodCategory = new FoodCategory
            {
                Name = request.Name,
                Description = request.Description
            };

            try
            {
                _foodCategoryRepository.Insert(foodCategory);
                await _foodCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo danh mục thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một danh mục thức ăn.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateFoodCategory(Guid FoodCategoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu danh mục thức ăn không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _foodCategoryRepository.GetById(FoodCategoryId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin danh mục thức ăn: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy danh mục thức ăn.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xung đột tên với các danh mục đang hoạt động khác
            var exists = await _foodCategoryRepository.CheckExist(
                x => x.Name == request.Name && x.Id != FoodCategoryId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra danh mục tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Danh mục thức ăn với tên '{request.Name}' đã tồn tại.");

            try
            {
                existing.Name = request.Name;
                existing.Description = request.Description;

                _foodCategoryRepository.Update(existing);
                await _foodCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật danh mục thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một danh mục thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DisableFoodCategory(Guid FoodCategoryId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var foodCategory = await _foodCategoryRepository.GetById(FoodCategoryId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin danh mục thức ăn: {checkError.Value.Message}");

            if (foodCategory == null)
                return (false, "Không tìm thấy danh mục thức ăn.");

            try
            {
                //foodCategory.IsActive = false;
                foodCategory.IsActive = !foodCategory.IsActive;
                _foodCategoryRepository.Update(foodCategory);
                await _foodCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa danh mục thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một danh mục thức ăn theo ID.
        /// </summary>
        public async Task<(CategoryResponse Category, string ErrorMessage)> GetFoodCategoryById(Guid FoodCategoryId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var foodCategory = await _foodCategoryRepository.GetById(FoodCategoryId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin danh mục thức ăn: {checkError.Value.Message}");

            if (foodCategory == null)
                return (null, "Không tìm thấy danh mục thức ăn.");

            var response = new CategoryResponse
            {
                Id = foodCategory.Id,
                Name = foodCategory.Name,
                Description = foodCategory.Description,
                IsActive = foodCategory.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<CategoryResponse> Categories, string ErrorMessage)> GetFoodCategoryByName(
            string name = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _foodCategoryRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(x => x.Name.Contains(name));

                var categories = await query.ToListAsync(cancellationToken);
                var responses = categories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                }).ToList();
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách danh mục thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách phân trang các danh mục thuốc với tìm kiếm, lọc và sắp xếp.
        /// </summary>
        public async Task<(PaginationSet<CategoryResponse> Result, string ErrorMessage)> GetPaginatedFoodCategoryList(
            ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(MedicineCategory).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _foodCategoryRepository.GetQueryable(x => x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                }).ToList();

                var result = new PaginationSet<CategoryResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        public async Task<List<FoodCategoryResponse>> GetAllCategory()
        {
            var data = await _foodCategoryRepository.GetQueryable(x => x.IsActive).ToListAsync();
            return data.Select(it => new FoodCategoryResponse()
            {
                //IsActive = it.IsActive,
                Name = it.Name,
                Description = it.Description,
                Id = it.Id
            }).ToList();
        }
    }
}