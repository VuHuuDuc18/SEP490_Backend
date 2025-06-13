
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

namespace Domain.Services.Implements
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
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
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
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu danh mục thức ăn không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _foodCategoryRepository.GetById(id, checkError);
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
                x => x.Name == request.Name && x.Id != id && x.IsActive,
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
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var foodCategory = await _foodCategoryRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin danh mục thức ăn: {checkError.Value.Message}");

            if (foodCategory == null)
                return (false, "Không tìm thấy danh mục thức ăn.");

            try
            {
                foodCategory.IsActive = false;
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
        public async Task<(CategoryResponse Category, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var foodCategory = await _foodCategoryRepository.GetById(id, checkError);
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
        public async Task<(List<CategoryResponse> Categories, string ErrorMessage)> GetAllAsync(
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
    }
}