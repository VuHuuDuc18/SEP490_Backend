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
using Domain.Dto.Request.Food;
using Domain.Dto.Response.Food;
using Domain.Dto.Response;
using Domain.Dto.Request;
using Infrastructure.Extensions;
using Domain.Dto.Response.Breed;
using Microsoft.AspNetCore.Http;
using Application.Wrappers;

namespace Infrastructure.Services.Implements
{
    public class FoodService : IFoodService
    {
        private readonly IRepository<Food> _foodRepository;
        private readonly IRepository<FoodCategory> _foodCategoryRepository;
        private readonly IRepository<ImageFood> _imageFoodRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;
        private readonly Guid _currentUserId;

        public FoodService(
            IRepository<Food> foodRepository,
            IRepository<ImageFood> imageFoodRepository,
            CloudinaryCloudService cloudinaryCloudService,
            IRepository<FoodCategory> foodCategoryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _foodRepository = foodRepository;
            _imageFoodRepository = imageFoodRepository;
            _cloudinaryCloudService = cloudinaryCloudService;
            _foodCategoryRepository = foodCategoryRepository;

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

        public async Task<Response<string>> CreateFood(CreateFoodRequest request, CancellationToken cancellationToken = default)
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
                        Message = "Dữ liệu thức ăn không được null",
                        Errors = new List<string> { "Dữ liệu thức ăn không được null" }
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

                var exists = await _foodRepository.GetQueryable(x =>
                    x.FoodName == request.FoodName && x.FoodCategoryId == request.FoodCategoryId && x.IsActive)
                    .AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Thức ăn với tên '{request.FoodName}' trong danh mục này đã tồn tại",
                        Errors = new List<string> { $"Thức ăn với tên '{request.FoodName}' trong danh mục này đã tồn tại" }
                    };
                }

                var foodCategory = await _foodCategoryRepository.GetByIdAsync(request.FoodCategoryId);
                if (foodCategory == null || !foodCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục thức ăn không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục thức ăn không tồn tại hoặc đã bị xóa" }
                    };
                }

                var food = new Food
                {
                    FoodName = request.FoodName,
                    FoodCategoryId = request.FoodCategoryId,
                    Stock = request.Stock,
                    WeighPerUnit = request.WeighPerUnit,
                    IsActive = true,
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.UtcNow
                };

                _foodRepository.Insert(food);
                await _foodRepository.CommitAsync(cancellationToken);

                // Upload thumbnail lên Cloudinary
                if (!string.IsNullOrEmpty(request.Thumbnail))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        request.Thumbnail, "food", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        var imageFood = new ImageFood
                        {
                            FoodId = food.Id,
                            ImageLink = imageLink,
                            Thumnail = "true"
                        };
                        _imageFoodRepository.Insert(imageFood);
                    }
                }

                // Upload ảnh khác lên Cloudinary
                if (request.ImageLinks != null && request.ImageLinks.Any())
                {
                    foreach (var imageLink in request.ImageLinks)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
                            imageLink, "food", _cloudinaryCloudService, cancellationToken);
                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageFood = new ImageFood
                            {
                                FoodId = food.Id,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageFoodRepository.Insert(imageFood);
                        }
                    }
                }
                await _imageFoodRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo thức ăn thành công",
                    Data = $"Thức ăn đã được tạo thành công. ID: {food.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo thức ăn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> UpdateFood(UpdateFoodRequest request, CancellationToken cancellationToken = default)
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
                        Message = "Dữ liệu thức ăn không được null",
                        Errors = new List<string> { "Dữ liệu thức ăn không được null" }
                    };
                }

                var food = await _foodRepository.GetByIdAsync(request.FoodId);
                if (food == null || !food.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Thức ăn không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Thức ăn không tồn tại hoặc đã bị xóa" }
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

                var exists = await _foodRepository.GetQueryable(x =>
                    x.FoodName == request.FoodName && x.FoodCategoryId == request.FoodCategoryId && x.Id != request.FoodId && x.IsActive)
                    .AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Thức ăn với tên '{request.FoodName}' trong danh mục này đã tồn tại",
                        Errors = new List<string> { $"Thức ăn với tên '{request.FoodName}' trong danh mục này đã tồn tại" }
                    };
                }

                var foodCategory = await _foodCategoryRepository.GetByIdAsync(request.FoodCategoryId);
                if (foodCategory == null || !foodCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục thức ăn không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục thức ăn không tồn tại hoặc đã bị xóa" }
                    };
                }

                food.FoodName = request.FoodName;
                food.FoodCategoryId = request.FoodCategoryId;
                food.Stock = request.Stock;
                food.WeighPerUnit = request.WeighPerUnit;
                food.UpdatedBy = _currentUserId;
                food.UpdatedDate = DateTime.UtcNow;

                _foodRepository.Update(food);
                await _foodRepository.CommitAsync(cancellationToken);

                // Xóa ảnh và thumbnail cũ
                var existingImages = await _imageFoodRepository.GetQueryable(x => x.FoodId == request.FoodId).ToListAsync(cancellationToken);
                foreach (var image in existingImages)
                {
                    _imageFoodRepository.Remove(image);
                    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                }
                await _imageFoodRepository.CommitAsync(cancellationToken);

                // Upload thumbnail mới
                if (!string.IsNullOrEmpty(request.Thumbnail))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        request.Thumbnail, "food", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        var imageFood = new ImageFood
                        {
                            FoodId = request.FoodId,
                            ImageLink = imageLink,
                            Thumnail = "true"
                        };
                        _imageFoodRepository.Insert(imageFood);
                    }
                }

                // Upload ảnh khác
                if (request.ImageLinks != null && request.ImageLinks.Any())
                {
                    foreach (var imageLink in request.ImageLinks)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
                            imageLink, "food", _cloudinaryCloudService, cancellationToken);
                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageFood = new ImageFood
                            {
                                FoodId = request.FoodId,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageFoodRepository.Insert(imageFood);
                        }
                    }
                }
                await _imageFoodRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Cập nhật thức ăn thành công",
                    Data = $"Thức ăn đã được cập nhật thành công. ID: {food.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật thức ăn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> DisableFood(Guid foodId, CancellationToken cancellationToken = default)
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

                var food = await _foodRepository.GetByIdAsync(foodId);
                if (food == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Thức ăn không tồn tại",
                        Errors = new List<string> { "Thức ăn không tồn tại" }
                    };
                }

                food.IsActive = !food.IsActive;
                food.UpdatedBy = _currentUserId;
                food.UpdatedDate = DateTime.UtcNow;

                _foodRepository.Update(food);
                await _foodRepository.CommitAsync(cancellationToken);

                // Xóa ảnh và thumbnail liên quan khỏi Cloudinary
                //var images = await _imageFoodRepository.GetQueryable(x => x.FoodId == foodId).ToListAsync(cancellationToken);
                //foreach (var image in images)
                //{
                //    _imageFoodRepository.Remove(image);
                //    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                //}
                //await _imageFoodRepository.CommitAsync(cancellationToken);

                if (food.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Khôi phục thức ăn thành công",
                        Data = $"Thức ăn đã được khôi phục thành công. ID: {food.Id}"
                    };
                }
                else
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Xóa thức ăn thành công",
                        Data = $"Thức ăn đã được xóa thành công. ID: {food.Id}"
                    };

                }
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xóa thức ăn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Lấy thông tin một loại thức ăn theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        public async Task<Response<FoodResponse>> GetFoodById(Guid foodId, CancellationToken cancellationToken = default)
        {
            try
            {
                var food = await _foodRepository.GetQueryable(x => x.Id == foodId && x.IsActive)
                    .Include(x => x.FoodCategory)
                    .FirstOrDefaultAsync(cancellationToken);

                if (food == null)
                {
                    return new Response<FoodResponse>()
                    {
                        Succeeded = false,
                        Message = "Thức ăn không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Thức ăn không tồn tại hoặc đã bị xóa" }
                    };
                }

                var images = await _imageFoodRepository.GetQueryable(x => x.FoodId == foodId).ToListAsync(cancellationToken);
                var foodCategoryResponse = new FoodCategoryResponse
                {
                    Id = food.FoodCategory.Id,
                    Name = food.FoodCategory.Name,
                    Description = food.FoodCategory.Description
                };

                var response = new FoodResponse
                {
                    Id = food.Id,
                    FoodName = food.FoodName,
                    FoodCategory = foodCategoryResponse,
                    Stock = food.Stock,
                    WeighPerUnit = food.WeighPerUnit,
                    IsActive = food.IsActive,
                    ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                };

                return new Response<FoodResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy thông tin thức ăn thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<FoodResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy thông tin thức ăn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<List<FoodResponse>>> GetFoodByCategory(
            string foodName = null,
            Guid? foodCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _foodRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(foodName))
                    query = query.Where(x => x.FoodName.Contains(foodName));

                if (foodCategoryId.HasValue)
                    query = query.Where(x => x.FoodCategoryId == foodCategoryId.Value);

                var foods = await query.ToListAsync(cancellationToken);
                var foodIds = foods.Select(f => f.Id).ToList();
                var images = await _imageFoodRepository.GetQueryable(x => foodIds.Contains(x.FoodId)).ToListAsync(cancellationToken);
                var imageGroups = images.GroupBy(x => x.FoodId).ToDictionary(g => g.Key, g => g.ToList());

                var responses = new List<FoodResponse>();
                foreach (var food in foods)
                {
                    var foodCategoryResponse = new FoodCategoryResponse
                    {
                        Id = food.FoodCategory.Id,
                        Name = food.FoodCategory.Name,
                        Description = food.FoodCategory.Description,
                    };
                    var foodImages = imageGroups.GetValueOrDefault(food.Id, new List<ImageFood>());
                    responses.Add(new FoodResponse
                    {
                        Id = food.Id,
                        FoodName = food.FoodName,
                        FoodCategory = foodCategoryResponse,
                        Stock = food.Stock,
                        WeighPerUnit = food.WeighPerUnit,
                        IsActive = food.IsActive,
                        ImageLinks = foodImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = foodImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    });
                }

                return new Response<List<FoodResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách thức ăn thành công",
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                return new Response<List<FoodResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách thức ăn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<FoodResponse>>> GetPaginatedFoodList(
            ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return new Response<PaginationSet<FoodResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<FoodResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(FoodResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<FoodResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<FoodResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                var query = _foodRepository.GetQueryable();

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var foodIds = paginationResult.Items.Select(f => f.Id).ToList();
                var images = await _imageFoodRepository.GetQueryable(x => foodIds.Contains(x.FoodId)).ToListAsync(cancellationToken);
                var imageGroups = images.GroupBy(x => x.FoodId).ToDictionary(g => g.Key, g => g.ToList());

                var responses = new List<FoodResponse>();
                foreach (var food in paginationResult.Items)
                {
                    var foodCategoryResponse = new FoodCategoryResponse
                    {
                        Id = food.FoodCategory.Id,
                        Name = food.FoodCategory.Name,
                        Description = food.FoodCategory.Description
                    };
                    var foodImages = imageGroups.GetValueOrDefault(food.Id, new List<ImageFood>());
                    responses.Add(new FoodResponse
                    {
                        Id = food.Id,
                        FoodName = food.FoodName,
                        FoodCategory = foodCategoryResponse,
                        Stock = food.Stock,
                        WeighPerUnit = food.WeighPerUnit,
                        IsActive = food.IsActive,
                        ImageLinks = foodImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = foodImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    });
                }

                var result = new PaginationSet<FoodResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<FoodResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách phân trang thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<FoodResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách phân trang",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

       
        public async Task<List<FoodResponse>> GetAllFood(CancellationToken cancellationToken = default)
        {
            var foods = await _foodRepository.GetQueryable(x => x.IsActive)
                .Include(x => x.FoodCategory)
                .ToListAsync(cancellationToken);

            var foodIds = foods.Select(f => f.Id).ToList();
            var images = await _imageFoodRepository.GetQueryable(x => foodIds.Contains(x.FoodId)).ToListAsync(cancellationToken);
            var imageGroups = images.GroupBy(x => x.FoodId).ToDictionary(g => g.Key, g => g.ToList());

            return foods.Select(food =>
            {
                var foodCategoryResponse = new FoodCategoryResponse
                {
                    Id = food.FoodCategory.Id,
                    Name = food.FoodCategory.Name,
                    Description = food.FoodCategory.Description
                };
                var foodImages = imageGroups.GetValueOrDefault(food.Id, new List<ImageFood>());
                return new FoodResponse
                {
                    Id = food.Id,
                    FoodName = food.FoodName,
                    FoodCategory = foodCategoryResponse,
                    Stock = food.Stock,
                    WeighPerUnit = food.WeighPerUnit,
                    IsActive = food.IsActive,
                    ImageLinks = foodImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = foodImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                };
            }).ToList();
        }

        public async Task<bool> ExcelDataHandle(List<CellFoodItem> data)
        {
            try
            {
                foreach (CellFoodItem item in data)
                {
                    var FoodDetail = await _foodRepository.GetQueryable(x => x.IsActive).FirstOrDefaultAsync(x => StringKeyComparer.CompareStrings(x.FoodName, item.Ten));
                    var ListCategory = await _foodCategoryRepository.GetQueryable(x => x.IsActive).ToListAsync();
                    if (FoodDetail == null)
                    {
                        // add food
                        var FoodCategoryDetail = ListCategory.FirstOrDefault(x => StringKeyComparer.CompareStrings(x.Name, item.Phan_Loai));
                        if (FoodCategoryDetail == null)
                        {
                            // add category
                            var FoodCategoryToInsert = new FoodCategory()
                            {
                                Name = item.Phan_Loai,
                                Description = item.Phan_Loai
                            };

                            _foodCategoryRepository.Insert(FoodCategoryToInsert);
                            await _foodCategoryRepository.CommitAsync();
                            //gan lai
                            FoodCategoryDetail = FoodCategoryToInsert;
                        }

                        // create new food
                        Food FoodToInsert = new Food()
                        {
                            FoodName = item.Ten,
                            FoodCategoryId = FoodCategoryDetail.Id,
                            Stock = item.So_luong,
                            WeighPerUnit = item.Trong_luong_Theo_Kg
                        };
                        _foodRepository.Insert(FoodToInsert);
                    }
                    else
                    {
                        FoodDetail.Stock += item.So_luong;
                    }
                    // update stock


                }

                return await _foodRepository.CommitAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Loi du lieu");
            }
        }
    }
}
