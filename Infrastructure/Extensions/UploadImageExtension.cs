using Infrastructure.Services;


namespace Infrastructure.Extensions;
public static class UploadImageExtension
{
    public static async Task<string> UploadBase64ImageAsync(
        string base64Image,
        string folder,
        CloudinaryCloudService cloudService,
        CancellationToken cancellationToken = default)
    {
        var base64Data = ExtractBase64Data(base64Image);
        if (string.IsNullOrEmpty(base64Data))
            throw new ArgumentException("Dữ liệu ảnh không đúng định dạng base64.");

        var tempFilePath = Path.GetTempFileName();
        File.WriteAllBytes(tempFilePath, Convert.FromBase64String(base64Data));
        var imageLink = await cloudService.UploadImage(base64Image, folder, cancellationToken);
        File.Delete(tempFilePath);
        return imageLink;
    }

    private static string ExtractBase64Data(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;
        var parts = base64String.Split(',');
        if (parts.Length < 2)
            return null;
        return parts[1];
    }
}
