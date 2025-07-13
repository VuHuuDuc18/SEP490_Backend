using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Breed;
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
    public class BreedCategoryService : IBreedCategoryService
    {
        private readonly IRepository<BreedCategory> _breedCategoryRepository;
        private readonly Guid _currentUserId;

        /// <summary>
        /// Khởi tạo service với repository của BreedCategory.
        /// </summary>
        public BreedCategoryService(IRepository<BreedCategory> breedCategoryRepository, IHttpContextAccessor httpContextAccessor)
        {
            _breedCategoryRepository = breedCategoryRepository ?? throw new ArgumentNullException(nameof(breedCategoryRepository));

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
        public async Task<Response<string>> CreateBreedCategory(CreateCategoryRequest request, CancellationToken cancellationToken = default)
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
                        Message = "Dữ liệu danh mục giống không được null",
                        Errors = new List<string> { "Dữ liệu danh mục giống không được null" }
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

                var exists = await _breedCategoryRepository.GetQueryable(x =>
                    x.Name == request.Name && x.IsActive)
                    .AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Danh mục giống với tên '{request.Name}' đã tồn tại",
                        Errors = new List<string> { $"Danh mục giống với tên '{request.Name}' đã tồn tại" }
                    };
                }

                var breedCategory = new BreedCategory
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = true,
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.UtcNow
                };

                _breedCategoryRepository.Insert(breedCategory);
                await _breedCategoryRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo danh mục giống thành công",
                    Data = $"Danh mục giống đã được tạo thành công. ID: {breedCategory.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo danh mục giống",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> UpdateBreedCategory(UpdateCategoryRequest request, CancellationToken cancellationToken = default)
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
                        Message = "Dữ liệu danh mục giống không được null",
                        Errors = new List<string> { "Dữ liệu danh mục giống không được null" }
                    };
                }

                var breedCategory = await _breedCategoryRepository.GetByIdAsync(request.Id);
                if (breedCategory == null || !breedCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục giống không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục giống không tồn tại hoặc đã bị xóa" }
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

                var exists = await _breedCategoryRepository.GetQueryable(x =>
                    x.Name == request.Name && x.Id != request.Id && x.IsActive)
                    .AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Danh mục giống với tên '{request.Name}' đã tồn tại",
                        Errors = new List<string> { $"Danh mục giống với tên '{request.Name}' đã tồn tại" }
                    };
                }

                breedCategory.Name = request.Name;
                breedCategory.Description = request.Description;
                breedCategory.UpdatedBy = _currentUserId;
                breedCategory.UpdatedDate = DateTime.UtcNow;

                _breedCategoryRepository.Update(breedCategory);
                await _breedCategoryRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Cập nhật danh mục giống thành công",
                    Data = $"Danh mục giống đã được cập nhật thành công. ID: {breedCategory.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật danh mục giống",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> DisableBreedCategory(Guid breedCategoryId, CancellationToken cancellationToken = default)
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

                var breedCategory = await _breedCategoryRepository.GetByIdAsync(breedCategoryId);
                if (breedCategory == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục giống không tồn tại",
                        Errors = new List<string> { "Danh mục giống không tồn tại" }
                    };
                }

                breedCategory.IsActive = !breedCategory.IsActive;
                breedCategory.UpdatedBy = _currentUserId;
                breedCategory.UpdatedDate = DateTime.UtcNow;

                _breedCategoryRepository.Update(breedCategory);
                await _breedCategoryRepository.CommitAsync(cancellationToken);

                if (breedCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Khôi phục danh mục giống thành công",
                        Data = $"Danh mục giống đã được khôi phục thành công. ID: {breedCategory.Id}"
                    };
                }
                else
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Xóa danh mục giống thành công",
                        Data = $"Danh mục giống đã được xóa thành công. ID: {breedCategory.Id}"
                    };

                }
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xóa danh mục giống",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<BreedCategoryResponse>> GetBreedCategoryById(Guid breedCategoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                var breedCategory = await _breedCategoryRepository.GetByIdAsync(breedCategoryId);
                if (breedCategory == null || !breedCategory.IsActive)
                {
                    return new Response<BreedCategoryResponse>()
                    {
                        Succeeded = false,
                        Message = "Danh mục giống không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục giống không tồn tại hoặc đã bị xóa" }
                    };
                }

                var response = new BreedCategoryResponse
                {
                    Id = breedCategory.Id,
                    Name = breedCategory.Name,
                    Description = breedCategory.Description
                };

                return new Response<BreedCategoryResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy thông tin danh mục giống thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<BreedCategoryResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy thông tin danh mục giống",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<List<BreedCategoryResponse>>> GetBreedCategoryByName(
            string name = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _breedCategoryRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(x => x.Name.Contains(name));

                var categories = await query.ToListAsync(cancellationToken);
                var responses = categories.Select(c => new BreedCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList();

                return new Response<List<BreedCategoryResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách danh mục giống thành công",
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                return new Response<List<BreedCategoryResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách danh mục giống",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<BreedCategoryResponse>>> GetPaginatedBreedCategoryList(
            ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return new Response<PaginationSet<BreedCategoryResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BreedCategoryResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(BreedCategoryResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BreedCategoryResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<BreedCategoryResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                var query = _breedCategoryRepository.GetQueryable();

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items.Select(c => new BreedCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList();

                var result = new PaginationSet<BreedCategoryResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<BreedCategoryResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách phân trang thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BreedCategoryResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách phân trang",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<List<BreedCategoryResponse>> GetAllCategory(CancellationToken cancellationToken = default)
        {
            var data = await _breedCategoryRepository.GetQueryable(x => x.IsActive).ToListAsync(cancellationToken);
            return data.Select(it => new BreedCategoryResponse
            {
                Id = it.Id,
                Name = it.Name,
                Description = it.Description
            }).ToList();
        }
    }
}