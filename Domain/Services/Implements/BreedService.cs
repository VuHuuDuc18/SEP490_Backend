﻿using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Services.Interfaces;
using Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.Implements
{
    public class BreedService : IBreedService
    {
        private readonly IRepository<Breed> _breedRepository;
        private readonly IRepository<ImageBreed> _imageBreedRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;

        /// <summary>
        /// Khởi tạo service với repository của Breed và CloudinaryCloudService.
        /// </summary>
        public BreedService(IRepository<Breed> breedRepository, IRepository<ImageBreed> imageBreedRepository, CloudinaryCloudService cloudinaryCloudService)
        {
            _breedRepository = breedRepository ?? throw new ArgumentNullException(nameof(breedRepository));
            _imageBreedRepository = imageBreedRepository ?? throw new ArgumentNullException(nameof(imageBreedRepository));
            _cloudinaryCloudService = cloudinaryCloudService ?? throw new ArgumentNullException(nameof(cloudinaryCloudService));
        }

        /// <summary>
        /// Tạo một giống loài mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateBreedRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu giống loài không được null.");
            if (string.IsNullOrEmpty(folder))
                return (false, "Tên folder là bắt buộc.");

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
                    var base64Data = ExtractBase64Data(request.Thumbnail);
                    if (string.IsNullOrEmpty(base64Data))
                        return (false, "Dữ liệu thumbnail không đúng định dạng base64.");

                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllBytes(tempFilePath, Convert.FromBase64String(base64Data));
                    var imageLink = await _cloudinaryCloudService.UploadImage(request.Thumbnail, folder, cancellationToken);
                    File.Delete(tempFilePath);

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
                        var base64Data = ExtractBase64Data(imageLink);
                        if (string.IsNullOrEmpty(base64Data))
                            return (false, "Dữ liệu ảnh không đúng định dạng base64.");

                        var tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, Convert.FromBase64String(base64Data));
                        var uploadedLink = await _cloudinaryCloudService.UploadImage(imageLink, folder, cancellationToken);
                        File.Delete(tempFilePath);

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
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateBreedRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu giống loài không được null.");
            if (string.IsNullOrEmpty(folder))
                return (false, "Tên folder là bắt buộc.");

            var checkError = new Ref<CheckError>();
            var existing = await _breedRepository.GetById(id, checkError);
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
                x => x.BreedName == request.BreedName && x.BreedCategoryId == request.BreedCategoryId && x.Id != id && x.IsActive,
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

                var existingImages = await _imageBreedRepository.GetQueryable(x => x.BreedId == id).ToListAsync(cancellationToken);
                foreach (var image in existingImages)
                {
                  //  _imageBreedRepository.Delete(image);
                    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                }
                await _imageBreedRepository.CommitAsync(cancellationToken);

                if (!string.IsNullOrEmpty(request.Thumbnail))
                {
                    var base64Data = ExtractBase64Data(request.Thumbnail);
                    if (string.IsNullOrEmpty(base64Data))
                        return (false, "Dữ liệu thumbnail không đúng định dạng base64.");

                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllBytes(tempFilePath, Convert.FromBase64String(base64Data));
                    var imageLink = await _cloudinaryCloudService.UploadImage(request.Thumbnail, folder, cancellationToken);
                    File.Delete(tempFilePath);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        var imageBreed = new ImageBreed
                        {
                            BreedId = id,
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
                        var base64Data = ExtractBase64Data(imageLink);
                        if (string.IsNullOrEmpty(base64Data))
                            return (false, "Dữ liệu ảnh không đúng định dạng base64.");

                        var tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, Convert.FromBase64String(base64Data));
                        var uploadedLink = await _cloudinaryCloudService.UploadImage(imageLink, folder, cancellationToken);
                        File.Delete(tempFilePath);

                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageBreed = new ImageBreed
                            {
                                BreedId = id,
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
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var breed = await _breedRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin giống loài: {checkError.Value.Message}");

            if (breed == null)
                return (false, "Không tìm thấy giống loài.");

            try
            {
                breed.IsActive = false;
                _breedRepository.Update(breed);
                await _breedRepository.CommitAsync(cancellationToken);

                var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == id).ToListAsync(cancellationToken);
                foreach (var image in images)
                {
                    //_imageBreedRepository.Delete(image);
                    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                }
                await _imageBreedRepository.CommitAsync(cancellationToken);

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
        public async Task<(BreedResponse Breed, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var breed = await _breedRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin giống loài: {checkError.Value.Message}");

            if (breed == null)
                return (null, "Không tìm thấy giống loài.");

            var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == id).ToListAsync(cancellationToken);
            var response = new BreedResponse
            {
                Id = breed.Id,
                BreedName = breed.BreedName,
                BreedCategoryId = breed.BreedCategoryId,
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
        public async Task<(List<BreedResponse> Breeds, string ErrorMessage)> GetAllAsync(
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
                    var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == breed.Id).ToListAsync(cancellationToken);
                    responses.Add(new BreedResponse
                    {
                        Id = breed.Id,
                        BreedName = breed.BreedName,
                        BreedCategoryId = breed.BreedCategoryId,
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

        /// <summary>
        /// Trích xuất phần dữ liệu base64 từ chuỗi (bỏ qua phần header như data:image/jpeg;base64).
        /// </summary>
        private string ExtractBase64Data(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            var parts = base64String.Split(',');
            if (parts.Length < 2)
                return null;

            return parts[1];
        }
    }
}