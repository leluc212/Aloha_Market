using System.Text.Json;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Aloha.ServiceDefaults.DependencyInjection
{
    public static class SecretsManagerExtensions
    {
        /// <summary>
        /// Nap secret tu AWS Secrets Manager vao IConfiguration khi chay tren AWS.
        /// Kich hoat bang bien moi truong ALOHA_SECRET_NAMES (danh sach ten secret, phan cach dau phay).
        /// Neu khong set (moi truong local/dev) thi bo qua, khong goi AWS.
        ///
        /// Cac key trong secret dung "__" se duoc chuyen thanh ":" cho IConfiguration,
        /// vi du "ConnectionStrings__SupabaseConnection" -> "ConnectionStrings:SupabaseConnection".
        /// </summary>
        public static IHostApplicationBuilder AddAlohaSecrets(this IHostApplicationBuilder builder)
        {
            var secretNames = Environment.GetEnvironmentVariable("ALOHA_SECRET_NAMES");
            if (string.IsNullOrWhiteSpace(secretNames))
            {
                return builder; // Khong cau hinh -> chay local, dung appsettings nhu cu.
            }

            var regionName = Environment.GetEnvironmentVariable("AWS_REGION")
                             ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
                             ?? "ap-southeast-1";

            using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(regionName));

            var merged = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in secretNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var response = client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = name })
                                     .GetAwaiter().GetResult();

                if (string.IsNullOrWhiteSpace(response.SecretString))
                {
                    continue;
                }

                var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response.SecretString);
                if (parsed is null)
                {
                    continue;
                }

                foreach (var kv in parsed)
                {
                    var key = kv.Key.Replace("__", ":");
                    merged[key] = kv.Value.ValueKind == JsonValueKind.String
                        ? kv.Value.GetString()
                        : kv.Value.ToString();
                }
            }

            builder.Configuration.AddInMemoryCollection(merged);
            return builder;
        }
    }
}
