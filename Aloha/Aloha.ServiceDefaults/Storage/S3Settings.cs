namespace Aloha.ServiceDefaults.Storage
{
    public class S3Settings
    {
        /// <summary>Ten S3 bucket luu media.</summary>
        public string BucketName { get; set; } = string.Empty;

        /// <summary>Domain CloudFront de tra ve URL cong khai (vi du cdn.alohamarket.com hoac xxxx.cloudfront.net).</summary>
        public string CloudFrontDomain { get; set; } = string.Empty;

        /// <summary>Prefix (thu muc ao) trong bucket. Vi du "avatars".</summary>
        public string KeyPrefix { get; set; } = "uploads";

        /// <summary>Region cua bucket. Neu de trong se dung region mac dinh cua SDK.</summary>
        public string? Region { get; set; }
    }
}
