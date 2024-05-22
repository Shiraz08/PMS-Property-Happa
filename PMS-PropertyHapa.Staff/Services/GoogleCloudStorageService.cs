using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using PMS_PropertyHapa.Staff.Models.ViewModels;

namespace PMS_PropertyHapa.Staff.Services
{
    public class GoogleCloudStorageService
    {
        GoogleCloudStorageOptions _options = new GoogleCloudStorageOptions();
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            var googleCredential = GoogleCredential.FromJson(@"
        {
            ""type"": ""service_account"",
            ""project_id"": """ + _options.ProjectId + @""",
            ""private_key_id"": """ + _options.PrivateKeyId + @""",
            ""private_key"": """ + _options.PrivateKey + @""",
            ""client_email"": """ + _options.ClientEmail + @""",
            ""client_id"": """ + _options.ClientId + @""",
            ""auth_uri"": """ + _options.AuthUri + @""",
            ""token_uri"": """ + _options.TokenUri + @""",
            ""auth_provider_x509_cert_url"": """ + _options.AuthProviderX509CertUrl + @""",
            ""client_x509_cert_url"": """ + _options.ClientX509CertUrl + @""",
            ""universe_domain"": """ + _options.UniverseDomain + @"""
        }");

            var storage = StorageClient.Create(googleCredential);
            var fileName = file.FileName;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var storageObject = await storage.UploadObjectAsync(
                    bucket: _options.BucketName,
                    objectName: fileName,
                    contentType: file.ContentType,
                    source: memoryStream);

                return $"https://storage.googleapis.com/{_options.BucketName}/{storageObject.Name}";
            }
        }
    }
}
