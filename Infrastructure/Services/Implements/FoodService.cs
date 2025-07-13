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

namespace Infrastructure.Services.Implements
{
    public class FoodService : IFoodService
    {
        private readonly IRepository<Food> _foodRepository;
        private readonly IRepository<FoodCategory> _foodCategoryRepository;
        private readonly IRepository<ImageFood> _imageFoodRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;

        /// <summary>
        /// Khởi tạo service với repository của Food và CloudinaryCloudService.
        /// </summary>
        public FoodService(IRepository<Food> foodRepository, IRepository<ImageFood> imageFoodRepository, CloudinaryCloudService cloudinaryCloudService, IRepository<FoodCategory> fcrepo)
        {
            _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
            _imageFoodRepository = imageFoodRepository ?? throw new ArgumentNullException(nameof(imageFoodRepository));
            _cloudinaryCloudService = cloudinaryCloudService ?? throw new ArgumentNullException(nameof(cloudinaryCloudService));
            _foodCategoryRepository = fcrepo;
        }

        /// <summary>
        /// Tạo một loại thức ăn mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateFood(CreateFoodRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu thức ăn không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xem thức ăn với tên này đã tồn tại chưa trong cùng danh mục
            var checkError = new Ref<CheckError>();
            var exists = await _foodRepository.CheckExist(
                x => x.FoodName == request.FoodName && x.FoodCategoryId == request.FoodCategoryId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra thức ăn tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Thức ăn với tên '{request.FoodName}' trong danh mục này đã tồn tại.");

            var food = new Food
            {
                FoodName = request.FoodName,
                FoodCategoryId = request.FoodCategoryId,
                Stock = request.Stock,
                WeighPerUnit = request.WeighPerUnit
            };

            try
            {
                _foodRepository.Insert(food);
                await _foodRepository.CommitAsync(cancellationToken);

                // Upload thumbnail lên Cloudinary trong folder được chỉ định
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

                // Upload ảnh khác lên Cloudinary trong folder được chỉ định
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

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một loại thức ăn, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateFood(Guid FoodId, UpdateFoodRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu thức ăn không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _foodRepository.GetByIdAsync(FoodId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin thức ăn: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy thức ăn.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xung đột tên với các thức ăn khác trong cùng danh mục
            var exists = await _foodRepository.CheckExist(
                x => x.FoodName == request.FoodName && x.FoodCategoryId == request.FoodCategoryId && x.Id != FoodId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra thức ăn tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Thức ăn với tên '{request.FoodName}' trong danh mục này đã tồn tại.");

            try
            {
                existing.FoodName = request.FoodName;
                existing.FoodCategoryId = request.FoodCategoryId;
                existing.Stock = request.Stock;
                existing.WeighPerUnit = request.WeighPerUnit;

                _foodRepository.Update(existing);
                await _foodRepository.CommitAsync(cancellationToken);

                // Xóa ảnh và thumbnail cũ
                var existingImages = await _imageFoodRepository.GetQueryable(x => x.FoodId == FoodId).ToListAsync(cancellationToken);
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
                            FoodId = FoodId,
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
                                FoodId = FoodId,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageFoodRepository.Insert(imageFood);
                        }
                    }
                }
                await _imageFoodRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một loại thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DisableFood(Guid FoodId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var food = await _foodRepository.GetByIdAsync(FoodId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin thức ăn: {checkError.Value.Message}");

            if (food == null)
                return (false, "Không tìm thấy thức ăn.");

            try
            {
                food.IsActive = !food.IsActive;
                _foodRepository.Update(food);
                await _foodRepository.CommitAsync(cancellationToken);

                // Xóa ảnh và thumbnail liên quan khỏi Cloudinary
                //var images = await _imageFoodRepository.GetQueryable(x => x.FoodId == FoodId).ToListAsync(cancellationToken);
                //foreach (var image in images)
                //{
                //    _imageFoodRepository.Remove(image);
                //    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                //}
                //await _imageFoodRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một loại thức ăn theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        public async Task<(FoodResponse Food, string ErrorMessage)> GetFoodById(Guid FoodId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var food = await _foodRepository.GetByIdAsync(FoodId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin thức ăn: {checkError.Value.Message}");

            if (food == null)
                return (null, "Không tìm thấy thức ăn.");


            var images = await _imageFoodRepository.GetQueryable(x => x.FoodId == FoodId).ToListAsync(cancellationToken);
            var foodCategoryResponse = new FoodCategoryResponse()
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
                Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                // Folder = images.FirstOrDefault()?.ImageLink.Split('/')[4] // Lấy folder từ URL (giả định cấu trúc URL)
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        public async Task<(List<FoodResponse> Foods, string ErrorMessage)> GetFoodByCategory(
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
                var responses = new List<FoodResponse>();
                foreach (var food in foods)
                {
                    var foodCategoryResponse = new FoodCategoryResponse()
                    {
                        Id = food.FoodCategory.Id,
                        Name = food.FoodCategory.Name,
                        Description = food.FoodCategory.Description
                    };
                    var images = await _imageFoodRepository.GetQueryable(x => x.FoodId == food.Id).ToListAsync(cancellationToken);
                    responses.Add(new FoodResponse
                    {
                        Id = food.Id,
                        FoodName = food.FoodName,
                        FoodCategory = foodCategoryResponse,
                        Stock = food.Stock,
                        WeighPerUnit = food.WeighPerUnit,
                        IsActive = food.IsActive,
                        ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                        // Folder = images.FirstOrDefault()?.ImageLink.Split('/')[4] // Lấy folder từ URL (giả định cấu trúc URL)
                    });
                }
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách thức ăn: {ex.Message}");
            }
        }

        public async Task<(PaginationSet<FoodResponse> Result, string ErrorMessage)> GetPaginatedFoodList(
    ListingRequest request,
    CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Food).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _foodRepository.GetQueryable(x => x.IsActive);

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
                    var foodCategoryResponse = new FoodCategoryResponse()
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

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
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
