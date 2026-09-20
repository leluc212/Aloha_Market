using Amazon.S3;
using Aloha.ServiceDefaults.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aloha.ServiceDefaults.DependencyInjection
{
    public static class StorageServiceExtensions
    {
        /// <summary>
        /// Dang ky S3 storage. Doc cau hinh tu section "S3" (BucketName, CloudFrontDomain, KeyPrefix, Region).
        /// AWS credential lay theo chuoi mac dinh cua SDK (IAM role cua EC2 khi chay tren AWS).
        /// </summary>
        public static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<S3Settings>(configuration.GetSection("S3"));

            // AmazonS3 client dung default credential chain (IAM instance profile tren EC2).
            services.AddSingleton<IAmazonS3>(_ =>
            {
                var region = configuration["S3:Region"] ?? configuration["AWS:Region"];
                return string.IsNullOrWhiteSpace(region)
                    ? new AmazonS3Client()
                    : new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(region));
            });

            services.AddScoped<IStorageService, S3StorageService>();
            return services;
        }
    }
}
