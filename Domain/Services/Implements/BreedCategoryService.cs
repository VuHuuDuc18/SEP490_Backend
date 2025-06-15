using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Dto.Request;
using Domain.Dto.Response;

namespace Domain.Services.Implements
{
    public class BreedCategoryService : IBreedCategoryService
    {
        private readonly IRepository<BreedCategory> _breedCategoryRepository;

        /// <summary>
        /// Khởi tạo service với repository của BreedCategory.
        /// </summary>
        public BreedCategoryService(IRepository<BreedCategory> breedCategoryRepository)
        {
            _breedCategoryRepository = breedCategoryRepository ?? throw new ArgumentNullException(nameof(breedCategoryRepository));
        }

        /// <summary>
        /// Tạo một danh mục giống mới với kiểm tra hợp lệ.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu danh mục giống không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xem danh mục với tên này đã tồn tại chưa
            var checkError = new Ref<CheckError>();
            var exists = await _breedCategoryRepository.CheckExist(
                x => x.Name == request.Name && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra danh mục tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Danh mục giống với tên '{request.Name}' đã tồn tại.");

            var breedCategory = new BreedCategory
            {
                Name = request.Name,
                Description = request.Description
            };

            try
            {
                _breedCategoryRepository.Insert(breedCategory);
                await _breedCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo danh mục giống: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một danh mục giống.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu danh mục giống không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _breedCategoryRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin danh mục giống: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy danh mục giống.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xung đột tên với các danh mục đang hoạt động khác
            var exists = await _breedCategoryRepository.CheckExist(
                x => x.Name == request.Name && x.Id != id && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra danh mục tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Danh mục giống với tên '{request.Name}' đã tồn tại.");

            try
            {
                existing.Name = request.Name;
                existing.Description = request.Description;

                _breedCategoryRepository.Update(existing);
                await _breedCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật danh mục giống: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một danh mục giống bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var breedCategory = await _breedCategoryRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin danh mục giống: {checkError.Value.Message}");

            if (breedCategory == null)
                return (false, "Không tìm thấy danh mục giống.");

            try
            {
                breedCategory.IsActive = false;
                _breedCategoryRepository.Update(breedCategory);
                await _breedCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa danh mục giống: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một danh mục giống theo ID.
        /// </summary>
        public async Task<(CategoryResponse Category, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var breedCategory = await _breedCategoryRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin danh mục giống: {checkError.Value.Message}");

            if (breedCategory == null)
                return (null, "Không tìm thấy danh mục giống.");

            var response = new CategoryResponse
            {
                Id = breedCategory.Id,
                Name = breedCategory.Name,
                Description = breedCategory.Description,
                IsActive = breedCategory.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục giống đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<CategoryResponse> Categories, string ErrorMessage)> GetAllAsync(
            string name = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _breedCategoryRepository.GetQueryable(x => x.IsActive);

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
                return (null, $"Lỗi khi lấy danh sách danh mục giống: {ex.Message}");
            }
        }
    }
}