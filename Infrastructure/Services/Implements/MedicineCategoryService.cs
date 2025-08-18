﻿using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Category;
using Domain.Dto.Response.Medicine;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class MedicineCategoryService : IMedicineCategoryService
    {
        private readonly IRepository<MedicineCategory> _medicineCategoryRepository;
        private readonly Guid _currentUserId;

        public MedicineCategoryService(IRepository<MedicineCategory> medicineCategoryRepository, IHttpContextAccessor httpContextAccessor)
        {
            _medicineCategoryRepository = medicineCategoryRepository;

            // Lấy current user từ JWT token claims
            _currentUserId = Guid.Empty;
            var currentUser = httpContextAccessor.HttpContext?.User;
            if (currentUser != null)
            {
                var userIdClaim = currentUser.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    _currentUserId = Guid.Parse(userIdClaim);
                }
            }
        }

        public async Task<Response<string>> CreateMedicineCategory(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<string>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu danh mục thuốc không được null",
                        Errors = new List<string> { "Dữ liệu danh mục thuốc không được null" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                var exists = await _medicineCategoryRepository.GetQueryable(x =>
                    x.Name == request.Name && x.IsActive).AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Danh mục thuốc với tên '{request.Name}' đã tồn tại",
                        Errors = new List<string> { $"Danh mục thuốc với tên '{request.Name}' đã tồn tại" }
                    };
                }

                var medicineCategory = new MedicineCategory
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = true,
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.UtcNow
                };

                _medicineCategoryRepository.Insert(medicineCategory);
                await _medicineCategoryRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo danh mục thuốc thành công",
                    Data = $"Danh mục thuốc đã được tạo thành công. ID: {medicineCategory.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo danh mục thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> UpdateMedicineCategory(UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<string>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu danh mục thuốc không được null",
                        Errors = new List<string> { "Dữ liệu danh mục thuốc không được null" }
                    };
                }

                var medicineCategory = await _medicineCategoryRepository.GetByIdAsync(request.Id);
                if (medicineCategory == null || !medicineCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục thuốc không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục thuốc không tồn tại hoặc đã bị xóa" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                var exists = await _medicineCategoryRepository.GetQueryable(x =>
                    x.Name == request.Name && x.Id != request.Id && x.IsActive).AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Danh mục thuốc với tên '{request.Name}' đã tồn tại",
                        Errors = new List<string> { $"Danh mục thuốc với tên '{request.Name}' đã tồn tại" }
                    };
                }

                medicineCategory.Name = request.Name;
                medicineCategory.Description = request.Description;
                medicineCategory.UpdatedBy = _currentUserId;
                medicineCategory.UpdatedDate = DateTime.UtcNow;

                _medicineCategoryRepository.Update(medicineCategory);
                await _medicineCategoryRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Cập nhật danh mục thuốc thành công",
                    Data = $"Danh mục thuốc đã được cập nhật thành công. ID: {medicineCategory.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật danh mục thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> DisableMedicineCategory(Guid medicineCategoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<string>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                var medicineCategory = await _medicineCategoryRepository.GetByIdAsync(medicineCategoryId);
                if (medicineCategory == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục thuốc không tồn tại",
                        Errors = new List<string> { "Danh mục thuốc không tồn tại" }
                    };
                }

                medicineCategory.IsActive = !medicineCategory.IsActive;
                medicineCategory.UpdatedBy = _currentUserId;
                medicineCategory.UpdatedDate = DateTime.UtcNow;

                _medicineCategoryRepository.Update(medicineCategory);
                await _medicineCategoryRepository.CommitAsync(cancellationToken);

                if (medicineCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Khôi phục danh mục thuốc thành công",
                        Data = $"Danh mục thuốc đã được khôi phục thành công. ID: {medicineCategory.Id}"
                    };
                }
                else
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Xóa danh mục thuốc thành công",
                        Data = $"Danh mục thuốc đã được xóa thành công. ID: {medicineCategory.Id}"
                    };

                }
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xóa danh mục thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<CategoryResponse>> GetMedicineCategoryById(Guid medicineCategoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                var medicineCategory = await _medicineCategoryRepository.GetByIdAsync(medicineCategoryId);
                if (medicineCategory == null || !medicineCategory.IsActive)
                {
                    return new Response<CategoryResponse>()
                    {
                        Succeeded = false,
                        Message = "Danh mục thuốc không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục thuốc không tồn tại hoặc đã bị xóa" }
                    };
                }

                var response = new CategoryResponse
                {
                    Id = medicineCategory.Id,
                    Name = medicineCategory.Name,
                    Description = medicineCategory.Description,
                    IsActive = medicineCategory.IsActive
                };

                return new Response<CategoryResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy thông tin danh mục thuốc thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<CategoryResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy thông tin danh mục thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<List<CategoryResponse>>> GetMedicineCategoryByName(string name = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _medicineCategoryRepository.GetQueryable(x => x.IsActive);

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

                return new Response<List<CategoryResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách danh mục thuốc thành công",
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                return new Response<List<CategoryResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách danh mục thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<PaginationSet<CategoryResponse>>> GetPaginatedMedicineCategoryList(ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return new Response<PaginationSet<CategoryResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<CategoryResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(CategoryResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<CategoryResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<CategoryResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                var query = _medicineCategoryRepository.GetQueryable();

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

                return new Response<PaginationSet<CategoryResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách phân trang thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<CategoryResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách phân trang",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<List<MedicineCategoryResponse>> GetAllMedicineCategory(CancellationToken cancellationToken = default)
        {
            var data = await _medicineCategoryRepository.GetQueryable(x => x.IsActive).ToListAsync(cancellationToken);
            return data.Select(it => new MedicineCategoryResponse
            {
                Id = it.Id,
                Name = it.Name,
                Description = it.Description,
            }).ToList();
        }
    }
}