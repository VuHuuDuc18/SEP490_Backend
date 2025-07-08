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

namespace Infrastructure.Services.Implements
{
    public class BreedService : IBreedService
    {
        private readonly IRepository<Breed> _breedRepository;
        private readonly IRepository<BreedCategory> _breedCategoryRepository;
        private readonly IRepository<ImageBreed> _imageBreedRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;

        /// <summary>
        /// Khởi tạo service với repository của Breed và CloudinaryCloudService.
        /// </summary>
        public BreedService(IRepository<Breed> breedRepository, IRepository<ImageBreed> imageBreedRepository, CloudinaryCloudService cloudinaryCloudService, IRepository<BreedCategory> breedCategoryRepository)
        {
            _breedRepository = breedRepository ?? throw new ArgumentNullException(nameof(breedRepository));
            _imageBreedRepository = imageBreedRepository ?? throw new ArgumentNullException(nameof(imageBreedRepository));
            _cloudinaryCloudService = cloudinaryCloudService ?? throw new ArgumentNullException(nameof(cloudinaryCloudService));
            _breedCategoryRepository = breedCategoryRepository;
        }

        /// <summary>
        /// Tạo một giống loài mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateBreed(CreateBreedRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu giống loài không được null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var checkError = new Ref<CheckError>();
            var exists = await _breedRepository.CheckExist(
                x => x.BreedName == request.BreedName && x.BreedCategoryId == request.BreedCategoryId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra giống loài tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Giống loài với tên '{request.BreedName}' trong danh mục này đã tồn tại.");

            var breed = new Breed
            {
                BreedName = request.BreedName,
                BreedCategoryId = request.BreedCategoryId,
                Stock = request.Stock
            };

            try
            {
                _breedRepository.Insert(breed);
                await _breedRepository.CommitAsync(cancellationToken);

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

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo giống loài: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một giống loài, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateBreed(Guid BreedId, UpdateBreedRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu giống loài không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _breedRepository.GetById(BreedId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin giống loài: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy giống loài.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var exists = await _breedRepository.CheckExist(
                x => x.BreedName == request.BreedName && x.BreedCategoryId == request.BreedCategoryId && x.Id != BreedId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra giống loài tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Giống loài với tên '{request.BreedName}' trong danh mục này đã tồn tại.");

            try
            {
                existing.BreedName = request.BreedName;
                existing.BreedCategoryId = request.BreedCategoryId;
                existing.Stock = request.Stock;

                _breedRepository.Update(existing);
                await _breedRepository.CommitAsync(cancellationToken);

                var existingImages = await _imageBreedRepository.GetQueryable(x => x.BreedId == BreedId).ToListAsync(cancellationToken);
                foreach (var image in existingImages)
                {
                    _imageBreedRepository.Remove(image);
                    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                }
                await _imageBreedRepository.CommitAsync(cancellationToken);

                if (!string.IsNullOrEmpty(request.Thumbnail))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        request.Thumbnail, "breed", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        var imageBreed = new ImageBreed
                        {
                            BreedId = BreedId,
                            ImageLink = imageLink,
                            Thumnail = "true"
                        };
                        _imageBreedRepository.Insert(imageBreed);
                    }
                }

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
                                BreedId = BreedId,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageBreedRepository.Insert(imageBreed);
                        }
                    }
                }
                await _imageBreedRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật giống loài: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một giống loài bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DisableBreed(Guid BreedId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var breed = await _breedRepository.GetById(BreedId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin giống loài: {checkError.Value.Message}");

            if (breed == null)
                return (false, "Không tìm thấy giống loài.");

            try
            {
                breed.IsActive = !breed.IsActive;
                _breedRepository.Update(breed);
                await _breedRepository.CommitAsync(cancellationToken);

                //var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == BreedId).ToListAsync(cancellationToken);
                //foreach (var image in images)
                //{
                //    _imageBreedRepository.Remove(image);
                //    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                //}
                //await _imageBreedRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa giống loài: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một giống loài theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        public async Task<(BreedResponse Breed, string ErrorMessage)> GetBreedById(Guid BreedId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var breed = await _breedRepository.GetById(BreedId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin giống loài: {checkError.Value.Message}");

            if (breed == null)
                return (null, "Không tìm thấy giống loài.");

            var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == BreedId).ToListAsync(cancellationToken);

            var breedCategoryResponse = new BreedCategoryResponse()
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
                Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
               // Folder = images.FirstOrDefault()?.ImageLink.Split('/')[4] // Lấy folder từ URL (giả định cấu trúc URL)
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả giống loài đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        public async Task<(List<BreedResponse> Breeds, string ErrorMessage)> GetBreedByCategory(
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
                var responses = new List<BreedResponse>();
                foreach (var breed in breeds)
                {
                    var breedCategoryResponse = new BreedCategoryResponse()
                    {
                        Id = breed.BreedCategory.Id,
                        Name = breed.BreedCategory.Name,
                        Description = breed.BreedCategory.Description
                    };
                    var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == breed.Id).ToListAsync(cancellationToken);
                    responses.Add(new BreedResponse
                    {
                        Id = breed.Id,
                        BreedName = breed.BreedName,
                        BreedCategory = breedCategoryResponse,
                        Stock = breed.Stock,
                        IsActive = breed.IsActive,
                        ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                       // Folder = images.FirstOrDefault()?.ImageLink.Split('/')[4] // Lấy folder từ URL (giả định cấu trúc URL)
                    });
                }
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách giống loài: {ex.Message}");
            }
        }

        public async Task<(PaginationSet<BreedResponse> Result, string ErrorMessage)> GetPaginatedBreedList(ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Breed).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _breedRepository.GetQueryable(x => x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var breedIds = paginationResult.Items.Select(f => f.Id).ToList();
                var images = await _imageBreedRepository.GetQueryable(x => breedIds.Contains(x.BreedId)).ToListAsync(cancellationToken);
                var imageGroups = images.GroupBy(x => x.BreedId).ToDictionary(g => g.Key, g => g.ToList());

                var responses = new List<BreedResponse>();
                foreach (var breed in paginationResult.Items)
                {
                    var breedCategoryResponse = new BreedCategoryResponse()
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

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
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