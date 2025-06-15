using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class CloudinaryCloudService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _cloudName = "dpgk5pqt9";
        private readonly string _apiKey = "382542864398655";
        private readonly string _apiSecret = "ct6gqlmsftVgmj2C3A8tYoiQk0M";

        /// <summary>
        /// Khởi tạo service với Cloudinary đã cấu hình qua DI.
        /// </summary>
        public CloudinaryCloudService(IOptions<CloudinaryConfig> config)
        {
            var account = new Account(
                config?.Value?.CloudName ?? _cloudName,
                config?.Value?.ApiKey ?? _apiKey,
                config?.Value?.ApiSecret ?? _apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        /// <summary>
        /// Trích xuất ID từ URI của Cloudinary.
        /// </summary>
        private string GetIDFromURI(string uri)
        {
            string id = "";
            // Tìm ID dựa trên bất kỳ folder nào, nên không cố định tên folder
            string endString = ".";
            int end = uri.LastIndexOf(endString);
            if (end > 0)
            {
                int start = uri.LastIndexOf("/", end) + 1;
                id = uri.Substring(start, end - start);
            }
            return id;
        }

        /// <summary>
        /// Upload ảnh lên Cloudinary từ base64 và trả về liên kết.
        /// </summary>
        public async Task<string> UploadImage(string base64Image, string folder, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Image))
                    throw new ArgumentNullException(nameof(base64Image));
                if (string.IsNullOrEmpty(folder))
                    throw new ArgumentNullException(nameof(folder));

                var tempFilePath = Path.GetTempFileName();
                File.WriteAllBytes(tempFilePath, Convert.FromBase64String(base64Image.Split(',')[1]));

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(tempFilePath),
                    Folder = folder
                };
                var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
                File.Delete(tempFilePath);

                return result.SecureUrl.AbsoluteUri;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi upload ảnh lên Cloudinary: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa ảnh khỏi Cloudinary dựa trên URI.
        /// </summary>
        public async Task<string> DeleteImage(string uri, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(uri))
                    throw new ArgumentNullException(nameof(uri));

                string id = GetIDFromURI(uri);
                var deleteParams = new DeletionParams(id)
                {
                    ResourceType = ResourceType.Image
                };
                var result = await _cloudinary.DestroyAsync(deleteParams);
                return "Đã xóa thành công!";
            }
            catch (Exception)
            {
                throw new Exception("Có lỗi xuất hiện khi xóa ảnh!");
            }
        }
    }

    /// <summary>
    /// Cấu hình cho Cloudinary.
    /// </summary>
    public class CloudinaryConfig
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}