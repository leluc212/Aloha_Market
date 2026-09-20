using Microsoft.AspNetCore.Http;

namespace Aloha.ServiceDefaults.Storage
{
    /// <summary>
    /// Dich vu luu tru file/anh. Trien khai hien tai dung AWS S3 + CloudFront.
    /// Giu nguyen chu ky method de tuong thich voi code goi (thay cho ICloudinaryService cu).
    /// </summary>
    public interface IStorageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<string>> UploadImagesAsync(List<IFormFile> files);
    }
}
