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
    public class MedicineService : IMedicineService
    {
        private readonly IRepository<Medicine> _medicineRepository;
        private readonly IRepository<ImageMedicine> _imageMedicineRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;

        /// <summary>
        /// Khởi tạo service với repository của Medicine và CloudinaryCloudService.
        /// </summary>
        public MedicineService(IRepository<Medicine> medicineRepository, IRepository<ImageMedicine> imageMedicineRepository, CloudinaryCloudService cloudinaryCloudService)
        {
            _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
            _imageMedicineRepository = imageMedicineRepository ?? throw new ArgumentNullException(nameof(imageMedicineRepository));
            _cloudinaryCloudService = cloudinaryCloudService ?? throw new ArgumentNullException(nameof(cloudinaryCloudService));
        }

        /// <summary>
        /// Tạo một loại thuốc mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateMedicineRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu thuốc không được null.");
            if (string.IsNullOrEmpty(folder))
                return (false, "Tên folder là bắt buộc.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var checkError = new Ref<CheckError>();
            var exists = await _medicineRepository.CheckExist(
                x => x.MedicineName == request.MedicineName && x.MedicineCategoryId == request.MedicineCategoryId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra thuốc tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Thuốc với tên '{request.MedicineName}' trong danh mục này đã tồn tại.");

            var medicine = new Medicine
            {
                MedicineName = request.MedicineName,
                MedicineCategoryId = request.MedicineCategoryId,
                Stock = request.Stock,
            };

            try
            {
                _medicineRepository.Insert(medicine);
                await _medicineRepository.CommitAsync(cancellationToken);

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
                        var imageMedicine = new ImageMedicine
                        {
                            MedicineId = medicine.Id,
                            ImageLink = imageLink,
                            Thumnail = "true"
                        };
                        _imageMedicineRepository.Insert(imageMedicine);
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
                            var imageMedicine = new ImageMedicine
                            {
                                MedicineId = medicine.Id,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageMedicineRepository.Insert(imageMedicine);
                        }
                    }
                }
                await _imageMedicineRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một loại thuốc, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateMedicineRequest request, string folder, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu thuốc không được null.");
            if (string.IsNullOrEmpty(folder))
                return (false, "Tên folder là bắt buộc.");

            var checkError = new Ref<CheckError>();
            var existing = await _medicineRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin thuốc: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy thuốc.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var exists = await _medicineRepository.CheckExist(
                x => x.MedicineName == request.MedicineName && x.MedicineCategoryId == request.MedicineCategoryId && x.Id != id && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra thuốc tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Thuốc với tên '{request.MedicineName}' trong danh mục này đã tồn tại.");

            try
            {
                existing.MedicineName = request.MedicineName;
                existing.MedicineCategoryId = request.MedicineCategoryId;
                existing.Stock = request.Stock;

                _medicineRepository.Update(existing);
                await _medicineRepository.CommitAsync(cancellationToken);

                var existingImages = await _imageMedicineRepository.GetQueryable(x => x.MedicineId == id).ToListAsync(cancellationToken);
                foreach (var image in existingImages)
                {
                  //  _imageMedicineRepository.Delete(image);
                    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                }
                await _imageMedicineRepository.CommitAsync(cancellationToken);

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
                        var imageMedicine = new ImageMedicine
                        {
                            MedicineId = id,
                            ImageLink = imageLink,
                            Thumnail = "true"
                        };
                        _imageMedicineRepository.Insert(imageMedicine);
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
                            var imageMedicine = new ImageMedicine
                            {
                                MedicineId = id,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageMedicineRepository.Insert(imageMedicine);
                        }
                    }
                }
                await _imageMedicineRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một loại thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var medicine = await _medicineRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin thuốc: {checkError.Value.Message}");

            if (medicine == null)
                return (false, "Không tìm thấy thuốc.");

            try
            {
                medicine.IsActive = false;
                _medicineRepository.Update(medicine);
                await _medicineRepository.CommitAsync(cancellationToken);

                var images = await _imageMedicineRepository.GetQueryable(x => x.MedicineId == id).ToListAsync(cancellationToken);
                foreach (var image in images)
                {
                  //  _imageMedicineRepository.Delete(image);
                    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                }
                await _imageMedicineRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một loại thuốc theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        public async Task<(MedicineResponse Medicine, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var medicine = await _medicineRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin thuốc: {checkError.Value.Message}");

            if (medicine == null)
                return (null, "Không tìm thấy thuốc.");

            var images = await _imageMedicineRepository.GetQueryable(x => x.MedicineId == id).ToListAsync(cancellationToken);
            var response = new MedicineResponse
            {
                Id = medicine.Id,
                MedicineName = medicine.MedicineName,
                MedicineCategoryId = medicine.MedicineCategoryId,
                Stock = medicine.Stock,              
                IsActive = medicine.IsActive,
                ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
               // Folder = images.FirstOrDefault()?.ImageLink.Split('/')[4] // Lấy folder từ URL (giả định cấu trúc URL)
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại thuốc đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        public async Task<(List<MedicineResponse> Medicines, string ErrorMessage)> GetAllAsync(
            string medicineName = null,
            Guid? medicineCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _medicineRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(medicineName))
                    query = query.Where(x => x.MedicineName.Contains(medicineName));

                if (medicineCategoryId.HasValue)
                    query = query.Where(x => x.MedicineCategoryId == medicineCategoryId.Value);

                var medicines = await query.ToListAsync(cancellationToken);
                var responses = new List<MedicineResponse>();
                foreach (var medicine in medicines)
                {
                    var images = await _imageMedicineRepository.GetQueryable(x => x.MedicineId == medicine.Id).ToListAsync(cancellationToken);
                    responses.Add(new MedicineResponse
                    {
                        Id = medicine.Id,
                        MedicineName = medicine.MedicineName,
                        MedicineCategoryId = medicine.MedicineCategoryId,
                        Stock = medicine.Stock,                      
                        IsActive = medicine.IsActive,
                        ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                       // Folder = images.FirstOrDefault()?.ImageLink.Split('/')[4] // Lấy folder từ URL (giả định cấu trúc URL)
                    });
                }
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách thuốc: {ex.Message}");
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