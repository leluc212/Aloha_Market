using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Aloha.ServiceDefaults.Storage
{
    /// <summary>
    /// Upload anh len S3 (private bucket) va tra ve URL phuc vu qua CloudFront.
    /// </summary>
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3;
        private readonly S3Settings _settings;

        public S3StorageService(IAmazonS3 s3, IOptions<S3Settings> options)
        {
            _s3 = s3;
            _settings = options.Value;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                throw new ArgumentException("File rong.", nameof(file));
            }

            var key = BuildObjectKey(file.FileName);

            using var stream = file.OpenReadStream();
            var transfer = new TransferUtility(_s3);
            await transfer.UploadAsync(new TransferUtilityUploadRequest
            {
                InputStream  = stream,
                Key          = key,
                BucketName   = _settings.BucketName,
                ContentType  = file.ContentType,
                // Bucket la private; truy cap cong khai di qua CloudFront (OAC).
            });

            return BuildPublicUrl(key);
        }

        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files)
        {
            var urls = new List<string>();
            foreach (var file in files)
            {
                urls.Add(await UploadImageAsync(file));
            }
            return urls;
        }

        private string BuildObjectKey(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            var prefix = string.IsNullOrWhiteSpace(_settings.KeyPrefix)
                ? string.Empty
                : $"{_settings.KeyPrefix.TrimEnd('/')}/";
            return $"{prefix}{Guid.NewGuid():N}{ext}";
        }

        private string BuildPublicUrl(string key)
        {
            var domain = _settings.CloudFrontDomain.TrimEnd('/');
            // Neu chua cau hinh CloudFront thi fallback ve URL S3 (khong khuyen khich cho production).
            if (string.IsNullOrWhiteSpace(domain))
            {
                return $"https://{_settings.BucketName}.s3.amazonaws.com/{key}";
            }

            var scheme = domain.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? string.Empty : "https://";
            return $"{scheme}{domain}/{key}";
        }
    }
}
