using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Domain.IServices;
using Microsoft.EntityFrameworkCore;
using Domain.Dto.Request.Breed;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response;
using Domain.Dto.Request;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Application.Wrappers;

namespace Infrastructure.Services.Implements
{
    public class BreedService : IBreedService
    {
        private readonly IRepository<Breed> _breedRepository;
        private readonly IRepository<BreedCategory> _breedCategoryRepository;
        private readonly IRepository<ImageBreed> _imageBreedRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;
        private readonly Guid _currentUserId;

        /// <summary>
        /// Khởi tạo service với repository của Breed và CloudinaryCloudService.
        /// </summary>
        public BreedService(
            IRepository<Breed> breedRepository,
            IRepository<ImageBreed> imageBreedRepository,
            CloudinaryCloudService cloudinaryCloudService,
            IRepository<BreedCategory> breedCategoryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _breedRepository = breedRepository ?? throw new ArgumentNullException(nameof(breedRepository));
            _imageBreedRepository = imageBreedRepository ?? throw new ArgumentNullException(nameof(imageBreedRepository));
            _cloudinaryCloudService = cloudinaryCloudService ?? throw new ArgumentNullException(nameof(cloudinaryCloudService));
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

        /// <summary>
        /// Tạo một giống loài mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<Response<string>> CreateBreed(CreateBreedRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu giống loài không được null",
                        Errors = new List<string> { "Dữ liệu giống loài không được null" }
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

                var exists = await _breedRepository.GetQueryable(x =>
                    x.BreedName == request.BreedName && x.BreedCategoryId == request.BreedCategoryId && x.IsActive)
                    .AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Giống loài với tên '{request.BreedName}' trong danh mục này đã tồn tại",
                        Errors = new List<string> { $"Giống loài với tên '{request.BreedName}' trong danh mục này đã tồn tại" }
                    };
                }

                var breedCategory = await _breedCategoryRepository.GetByIdAsync(request.BreedCategoryId);
                if (breedCategory == null || !breedCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục giống loài không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục giống loài không tồn tại hoặc đã bị xóa" }
                    };
                }

                var breed = new Breed
                {
                    BreedName = request.BreedName,
                    BreedCategoryId = request.BreedCategoryId,
                    Stock = request.Stock,
                    IsActive = true,
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.UtcNow
                };

                _breedRepository.Insert(breed);
                await _breedRepository.CommitAsync(cancellationToken);

                // Upload thumbnail lên Cloudinary
                if (!string.IsNullOrEmpty(request.Thumbnail))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        request.Thumbnail, "breed", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        var imageBreed = new ImageBreed
                        {
                            BreedId = breed.Id,
                            ImageLink = imageLink,
                            Thumnail = "true"
                        };
                        _imageBreedRepository.Insert(imageBreed);
                    }
                }

                // Upload ảnh khác lên Cloudinary
                if (request.ImageLinks != null && request.ImageLinks.Any())
                {
                    foreach (var imageLink in request.ImageLinks)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
                            imageLink, "breed", _cloudinaryCloudService, cancellationToken);
                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageBreed = new ImageBreed
                            {
                                BreedId = breed.Id,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageBreedRepository.Insert(imageBreed);
                        }
                    }
                }
                await _imageBreedRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo giống loài thành công",
                    Data = $"Giống loài đã được tạo thành công. ID: {breed.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo giống loài",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Cập nhật thông tin một giống loài, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<Response<string>> UpdateBreed(UpdateBreedRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu giống loài không được null",
                        Errors = new List<string> { "Dữ liệu giống loài không được null" }
                    };
                }

                var breed = await _breedRepository.GetByIdAsync(request.BreedId);
                if (breed == null || !breed.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Giống loài không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Giống loài không tồn tại hoặc đã bị xóa" }
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

                var exists = await _breedRepository.GetQueryable(x =>
                    x.BreedName == request.BreedName && x.BreedCategoryId == request.BreedCategoryId && x.Id != request.BreedId && x.IsActive)
                    .AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Giống loài với tên '{request.BreedName}' trong danh mục này đã tồn tại",
                        Errors = new List<string> { $"Giống loài với tên '{request.BreedName}' trong danh mục này đã tồn tại" }
                    };
                }

                var breedCategory = await _breedCategoryRepository.GetByIdAsync(request.BreedCategoryId);
                if (breedCategory == null || !breedCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục giống loài không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục giống loài không tồn tại hoặc đã bị xóa" }
                    };
                }

                breed.BreedName = request.BreedName;
                breed.BreedCategoryId = request.BreedCategoryId;
                breed.Stock = request.Stock;
                breed.UpdatedBy = _currentUserId;
                breed.UpdatedDate = DateTime.UtcNow;

                _breedRepository.Update(breed);
                await _breedRepository.CommitAsync(cancellationToken);

                // Xóa ảnh và thumbnail cũ
                var existingImages = await _imageBreedRepository.GetQueryable(x => x.BreedId == request.BreedId).ToListAsync(cancellationToken);
                foreach (var image in existingImages)
                {
                    _imageBreedRepository.Remove(image);
                    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                }
                await _imageBreedRepository.CommitAsync(cancellationToken);

                // Upload thumbnail mới
                if (!string.IsNullOrEmpty(request.Thumbnail))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        request.Thumbnail, "breed", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        var imageBreed = new ImageBreed
                        {
                            BreedId = request.BreedId,
                            ImageLink = imageLink,
                            Thumnail = "true"
                        };
                        _imageBreedRepository.Insert(imageBreed);
                    }
                }

                // Upload ảnh khác
                if (request.ImageLinks != null && request.ImageLinks.Any())
                {
                    foreach (var imageLink in request.ImageLinks)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
                            imageLink, "breed", _cloudinaryCloudService, cancellationToken);
                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageBreed = new ImageBreed
                            {
                                BreedId = request.BreedId,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageBreedRepository.Insert(imageBreed);
                        }
                    }
                }
                await _imageBreedRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Cập nhật giống loài thành công",
                    Data = $"Giống loài đã được cập nhật thành công. ID: {breed.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật giống loài",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> DisableBreed(Guid breedId, CancellationToken cancellationToken = default)
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

                var breed = await _breedRepository.GetByIdAsync(breedId);
                if (breed == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Giống loài không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Giống loài không tồn tại hoặc đã bị xóa" }
                    };
                }

                breed.IsActive = !breed.IsActive;
                breed.UpdatedBy = _currentUserId;
                breed.UpdatedDate = DateTime.UtcNow;

                _breedRepository.Update(breed);
                await _breedRepository.CommitAsync(cancellationToken);

                // Xóa ảnh và thumbnail liên quan khỏi Cloudinary
                //var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == breedId).ToListAsync(cancellationToken);
                //foreach (var image in images)
                //{
                //    _imageBreedRepository.Remove(image);
                //    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                //}
                //await _imageBreedRepository.CommitAsync(cancellationToken);

                if (breed.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Khôi phục giống thành công",
                        Data = $"Giống đã được khôi phục thành công. ID: {breed.Id}"
                    };
                }
                else
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Xóa giống thành công",
                        Data = $"Giống đã được xóa thành công. ID: {breed.Id}"
                    };

                }
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xóa giống loài",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<BreedResponse>> GetBreedById(Guid breedId, CancellationToken cancellationToken = default)
        {
            try
            {
                var breed = await _breedRepository.GetQueryable(x => x.Id == breedId && x.IsActive)
                    .Include(x => x.BreedCategory)
                    .FirstOrDefaultAsync(cancellationToken);

                if (breed == null)
                {
                    return new Response<BreedResponse>()
                    {
                        Succeeded = false,
                        Message = "Giống loài không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Giống loài không tồn tại hoặc đã bị xóa" }
                    };
                }

                var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == breedId).ToListAsync(cancellationToken);
                var breedCategoryResponse = new BreedCategoryResponse
                {
                    Id = breed.BreedCategory.Id,
                    Name = breed.BreedCategory.Name,
                    Description = breed.BreedCategory.Description
                };

                var response = new BreedResponse
                {
                    Id = breed.Id,
                    BreedName = breed.BreedName,
                    BreedCategory = breedCategoryResponse,
                    Stock = breed.Stock,
                    IsActive = breed.IsActive,
                    ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                };

                return new Response<BreedResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy thông tin giống loài thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<BreedResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy thông tin giống loài",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<List<BreedResponse>>> GetBreedByCategory(
            string breedName = null,
            Guid? breedCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _breedRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(breedName))
                    query = query.Where(x => x.BreedName.Contains(breedName));

                if (breedCategoryId.HasValue)
                    query = query.Where(x => x.BreedCategoryId == breedCategoryId.Value);

                var breeds = await query.ToListAsync(cancellationToken);
                var breedIds = breeds.Select(b => b.Id).ToList();
                var images = await _imageBreedRepository.GetQueryable(x => breedIds.Contains(x.BreedId)).ToListAsync(cancellationToken);
                var imageGroups = images.GroupBy(x => x.BreedId).ToDictionary(g => g.Key, g => g.ToList());

                var responses = new List<BreedResponse>();
                foreach (var breed in breeds)
                {
                    var breedCategoryResponse = new BreedCategoryResponse
                    {
                        Id = breed.BreedCategory.Id,
                        Name = breed.BreedCategory.Name,
                        Description = breed.BreedCategory.Description,
                    };
                    var breedImages = imageGroups.GetValueOrDefault(breed.Id, new List<ImageBreed>());
                    responses.Add(new BreedResponse
                    {
                        Id = breed.Id,
                        BreedName = breed.BreedName,
                        BreedCategory = breedCategoryResponse,
                        Stock = breed.Stock,
                        IsActive = breed.IsActive,
                        ImageLinks = breedImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = breedImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    });
                }

                return new Response<List<BreedResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách giống loài thành công",
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                return new Response<List<BreedResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách giống loài",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<PaginationSet<BreedResponse>>> GetPaginatedBreedList(
            ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return new Response<PaginationSet<BreedResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BreedResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(BreedResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                var invalidFieldsSearch = request.SearchString?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BreedResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }
                if (invalidFieldsSearch.Any())
                {
                    return new Response<PaginationSet<BreedResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFieldsSearch)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<BreedResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                var query = _breedRepository.GetQueryable();
                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var breedIds = paginationResult.Items.Select(b => b.Id).ToList();
                var images = await _imageBreedRepository.GetQueryable(x => breedIds.Contains(x.BreedId)).ToListAsync(cancellationToken);
                var imageGroups = images.GroupBy(x => x.BreedId).ToDictionary(g => g.Key, g => g.ToList());

                var responses = new List<BreedResponse>();
                foreach (var breed in paginationResult.Items)
                {
                    var breedCategoryResponse = new BreedCategoryResponse
                    {
                        Id = breed.BreedCategory.Id,
                        Name = breed.BreedCategory.Name,
                        Description = breed.BreedCategory.Description
                    };
                    var breedImages = imageGroups.GetValueOrDefault(breed.Id, new List<ImageBreed>());
                    responses.Add(new BreedResponse
                    {
                        Id = breed.Id,
                        BreedName = breed.BreedName,
                        BreedCategory = breedCategoryResponse,
                        Stock = breed.Stock,
                        IsActive = breed.IsActive,
                        ImageLinks = breedImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = breedImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    });
                }

                var result = new PaginationSet<BreedResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<BreedResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách phân trang thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BreedResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách phân trang",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<List<BreedResponse>> GetAllBreed(CancellationToken cancellationToken = default)
        {
            var breeds = await _breedRepository.GetQueryable(x => x.IsActive)
                .Include(x => x.BreedCategory)
                .ToListAsync(cancellationToken);

            var breedIds = breeds.Select(b => b.Id).ToList();
            var images = await _imageBreedRepository.GetQueryable(x => breedIds.Contains(x.BreedId)).ToListAsync(cancellationToken);
            var imageGroups = images.GroupBy(x => x.BreedId).ToDictionary(g => g.Key, g => g.ToList());

            return breeds.Select(breed =>
            {
                var breedCategoryResponse = new BreedCategoryResponse
                {
                    Id = breed.BreedCategory.Id,
                    Name = breed.BreedCategory.Name,
                    Description = breed.BreedCategory.Description
                };
                var breedImages = imageGroups.GetValueOrDefault(breed.Id, new List<ImageBreed>());
                return new BreedResponse
                {
                    Id = breed.Id,
                    BreedName = breed.BreedName,
                    BreedCategory = breedCategoryResponse,
                    Stock = breed.Stock,
                    IsActive = breed.IsActive,
                    ImageLinks = breedImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = breedImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                };
            }).ToList();
        }
        public async Task<bool> ExcelDataHandle(List<CellBreedItem> data)
        {
            try
            {
                foreach (CellBreedItem item in data)
                {
                    var breedDetail = await _breedRepository.GetQueryable(x => x.IsActive).FirstOrDefaultAsync(x => StringKeyComparer.CompareStrings(x.BreedName, item.Ten));
                    var ListCategory = await _breedCategoryRepository.GetQueryable(x => x.IsActive).ToListAsync();
                    if (breedDetail == null)
                    {
                        // add breed
                        var breedCategoryDetail = ListCategory.FirstOrDefault(x => StringKeyComparer.CompareStrings(x.Name, item.Phan_Loai));
                        if (breedCategoryDetail == null)
                        {
                            // add category
                            var breedCategoryToInsert = new BreedCategory()
                            {
                                Name = item.Phan_Loai,
                                Description = item.Phan_Loai
                            };

                            _breedCategoryRepository.Insert(breedCategoryToInsert);
                            await _breedCategoryRepository.CommitAsync();
                            //gan lai
                            breedCategoryDetail = breedCategoryToInsert;
                        }

                        // create new breed
                        Breed breedToInsert = new Breed()
                        {
                            BreedName = item.Ten,
                            BreedCategoryId = breedCategoryDetail.Id,
                            Stock = item.So_luong,
                            //WeighPerUnit = item.Trong_luong_Theo_Kg
                        };
                        _breedRepository.Insert(breedToInsert);
                    }
                    else
                    {
                        breedDetail.Stock += item.So_luong;
                    }
                    // update stock


                }

                return await _breedRepository.CommitAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Loi du lieu");
            }
        }
    }
}