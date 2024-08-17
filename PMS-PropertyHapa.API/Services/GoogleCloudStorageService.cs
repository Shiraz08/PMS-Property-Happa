using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using PMS_PropertyHapa.API.ViewModels;
using System.Net;
using System.Text.RegularExpressions;

namespace PMS_PropertyHapa.API.Services
{
    public class GoogleCloudStorageService
    {
        private readonly GoogleCloudStorageOptions _options;

        public GoogleCloudStorageService(IOptions<GoogleCloudStorageOptions> options)
        {
            _options = options.Value;
        }

        //public async Task<string> UploadImageAsync(IFormFile file, string fileName)
        //{

        //    if (file == null || file.Length == 0)
        //        throw new ArgumentException("Invalid file.");

        //    var googleCredential = GoogleCredential.FromJson($@"
        //                        {{
        //                            ""type"": ""service_account"",
        //                            ""project_id"": ""{_options.ProjectId}"",
        //                            ""private_key_id"": ""{_options.PrivateKeyId}"",
        //                            ""private_key"": ""{_options.PrivateKey}"",
        //                            ""client_email"": ""{_options.ClientEmail}"",
        //                            ""client_id"": ""{_options.ClientId}"",
        //                            ""auth_uri"": ""{_options.AuthUri}"",
        //                            ""token_uri"": ""{_options.TokenUri}"",
        //                            ""auth_provider_x509_cert_url"": ""{_options.AuthProviderX509CertUrl}"",
        //                            ""client_x509_cert_url"": ""{_options.ClientX509CertUrl}"",
        //                            ""universe_domain"": ""{_options.UniverseDomain}""
        //                        }}");

        //    var storage = StorageClient.Create(googleCredential);
        //    //var fileName = file.FileName;
        //    var objects = storage.ListObjects(_options.BucketName, prefix: fileName).ToList();
        //    foreach (var obj in objects)
        //    {
        //        if (obj.Name.StartsWith(fileName))
        //        {
        //            await storage.DeleteObjectAsync(_options.BucketName, obj.Name);
        //        }
        //    }

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        await file.CopyToAsync(memoryStream);
        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        var storageObject = await storage.UploadObjectAsync(
        //            bucket: _options.BucketName,
        //            objectName: fileName,
        //            contentType: file.ContentType,
        //            source: memoryStream);

        //        return $"https://storage.googleapis.com/{_options.BucketName}/{storageObject.Name}";
        //    }
        //}

        public async Task<string> UploadImageAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("Invalid file.");
                return "Image upload failed: Invalid file.";
            }

            try
            {
                var googleCredential = GoogleCredential.FromJson($@"
            {{
                ""type"": ""service_account"",
                ""project_id"": ""{_options.ProjectId}"",
                ""private_key_id"": ""{_options.PrivateKeyId}"",
                ""private_key"": ""{_options.PrivateKey}"",
                ""client_email"": ""{_options.ClientEmail}"",
                ""client_id"": ""{_options.ClientId}"",
                ""auth_uri"": ""{_options.AuthUri}"",
                ""token_uri"": ""{_options.TokenUri}"",
                ""auth_provider_x509_cert_url"": ""{_options.AuthProviderX509CertUrl}"",
                ""client_x509_cert_url"": ""{_options.ClientX509CertUrl}"",
                ""universe_domain"": ""{_options.UniverseDomain}""
            }}");

                var storage = StorageClient.Create(googleCredential);

                var objects = storage.ListObjects(_options.BucketName, prefix: fileName).ToList();
                foreach (var obj in objects)
                {
                    if (obj.Name.StartsWith(fileName))
                    {
                        await storage.DeleteObjectAsync(_options.BucketName, obj.Name);
                    }
                }

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
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                Console.WriteLine("Google Cloud API billing account is disabled or closed: " + ex.Message);
                return "Image upload failed due to billing issues.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during image upload: " + ex.Message);
                return "Image upload failed due to an unexpected error.";
            }
        }



        public async Task<string> UploadImagebyBase64Async(string base64string, string fileName)
        {
            if (string.IsNullOrEmpty(base64string))
            {
                Console.WriteLine("Base64 string is null or empty.");
                return "Image upload failed: Base64 string is null or empty.";
            }

            base64string = StripDataUriPrefix(base64string);

            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(base64string);
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Invalid base64 string format: " + ex.Message);
                return "Image upload failed: Invalid base64 string format.";
            }

            try
            {
                var googleCredential = GoogleCredential.FromJson($@"
            {{
                ""type"": ""service_account"",
                ""project_id"": ""{_options.ProjectId}"",
                ""private_key_id"": ""{_options.PrivateKeyId}"",
                ""private_key"": ""{_options.PrivateKey}"",
                ""client_email"": ""{_options.ClientEmail}"",
                ""client_id"": ""{_options.ClientId}"",
                ""auth_uri"": ""{_options.AuthUri}"",
                ""token_uri"": ""{_options.TokenUri}"",
                ""auth_provider_x509_cert_url"": ""{_options.AuthProviderX509CertUrl}"",
                ""client_x509_cert_url"": ""{_options.ClientX509CertUrl}"",
                ""universe_domain"": ""{_options.UniverseDomain}""
            }}");

                var storage = StorageClient.Create(googleCredential);

                var objects = storage.ListObjects(_options.BucketName, prefix: fileName).ToList();
                foreach (var obj in objects)
                {
                    if (obj.Name.StartsWith(fileName))
                    {
                        await storage.DeleteObjectAsync(_options.BucketName, obj.Name);
                    }
                }

                using (var memoryStream = new MemoryStream(fileBytes))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    string contentType = GetContentType(fileName);

                    var storageObject = await storage.UploadObjectAsync(
                        bucket: _options.BucketName,
                        objectName: fileName,
                        contentType: contentType,
                        source: memoryStream);

                    return $"https://storage.googleapis.com/{_options.BucketName}/{storageObject.Name}";
                }
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                Console.WriteLine("Google Cloud API billing account is disabled or closed: " + ex.Message);
                return "Image upload failed due to billing issues.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during image upload: " + ex.Message);
                return "Image upload failed due to an unexpected error.";
            }
        }



        //public async Task<string> UploadImagebyBase64Async(string base64string, string fileName)
        //{

        //    if (string.IsNullOrEmpty(base64string))
        //        throw new ArgumentException("Base64 string is null or empty.");

        //    byte[] fileBytes = Convert.FromBase64String(base64string);

        //    var googleCredential = GoogleCredential.FromJson($@"
        //                {{
        //                    ""type"": ""service_account"",
        //                    ""project_id"": ""{_options.ProjectId}"",
        //                    ""private_key_id"": ""{_options.PrivateKeyId}"",
        //                    ""private_key"": ""{_options.PrivateKey}"",
        //                    ""client_email"": ""{_options.ClientEmail}"",
        //                    ""client_id"": ""{_options.ClientId}"",
        //                    ""auth_uri"": ""{_options.AuthUri}"",
        //                    ""token_uri"": ""{_options.TokenUri}"",
        //                    ""auth_provider_x509_cert_url"": ""{_options.AuthProviderX509CertUrl}"",
        //                    ""client_x509_cert_url"": ""{_options.ClientX509CertUrl}"",
        //                    ""universe_domain"": ""{_options.UniverseDomain}""
        //                }}");

        //    var storage = StorageClient.Create(googleCredential);

        //    var objects = storage.ListObjects(_options.BucketName, prefix: fileName).ToList();
        //    foreach (var obj in objects)
        //    {
        //        if (obj.Name.StartsWith(fileName))
        //        {
        //            await storage.DeleteObjectAsync(_options.BucketName, obj.Name);
        //        }
        //    }

        //    using (var memoryStream = new MemoryStream(fileBytes))
        //    {
        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        string contentType = GetContentType(fileName);

        //        var storageObject = await storage.UploadObjectAsync(
        //            bucket: _options.BucketName,
        //            objectName: fileName,
        //            contentType: contentType,
        //            source: memoryStream);

        //        return $"https://storage.googleapis.com/{_options.BucketName}/{storageObject.Name}";
        //    }
        //}
        //public async Task<string> UploadImagebyBase64Async(string base64string, string fileName)
        //{
        //    if (string.IsNullOrEmpty(base64string))
        //        throw new ArgumentException("Base64 string is null or empty.");

        //    base64string = StripDataUriPrefix(base64string);

        //    byte[] fileBytes;
        //    try
        //    {
        //        fileBytes = Convert.FromBase64String(base64string);
        //    }
        //    catch (FormatException ex)
        //    {
        //        throw new ArgumentException("Invalid base64 string format.", ex);
        //    }

        //    var googleCredential = GoogleCredential.FromJson($@"
        //                    {{
        //                        ""type"": ""service_account"",
        //                        ""project_id"": ""{_options.ProjectId}"",
        //                        ""private_key_id"": ""{_options.PrivateKeyId}"",
        //                        ""private_key"": ""{_options.PrivateKey}"",
        //                        ""client_email"": ""{_options.ClientEmail}"",
        //                        ""client_id"": ""{_options.ClientId}"",
        //                        ""auth_uri"": ""{_options.AuthUri}"",
        //                        ""token_uri"": ""{_options.TokenUri}"",
        //                        ""auth_provider_x509_cert_url"": ""{_options.AuthProviderX509CertUrl}"",
        //                        ""client_x509_cert_url"": ""{_options.ClientX509CertUrl}"",
        //                        ""universe_domain"": ""{_options.UniverseDomain}""
        //                    }}");

        //    var storage = StorageClient.Create(googleCredential);

        //    var objects = storage.ListObjects(_options.BucketName, prefix: fileName).ToList();
        //    foreach (var obj in objects)
        //    {
        //        if (obj.Name.StartsWith(fileName))
        //        {
        //            await storage.DeleteObjectAsync(_options.BucketName, obj.Name);
        //        }
        //    }

        //    using (var memoryStream = new MemoryStream(fileBytes))
        //    {
        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        string contentType = GetContentType(fileName);

        //        var storageObject = await storage.UploadObjectAsync(
        //            bucket: _options.BucketName,
        //            objectName: fileName,
        //            contentType: contentType,
        //            source: memoryStream);

        //        return $"https://storage.googleapis.com/{_options.BucketName}/{storageObject.Name}";
        //    }
        //}

        private string StripDataUriPrefix(string base64string)
        {
            // Check if base64 string has a data URI prefix and remove it if present
            var match = Regex.Match(base64string, @"data:(?<type>.*?);base64,(?<data>.*)");
            if (match.Success && match.Groups.Count == 3)
            {
                return match.Groups["data"].Value;
            }
            return base64string;
        }

        private string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (provider.TryGetContentType(fileName, out var contentType))
            {
                return contentType;
            }
            return "application/octet-stream"; 
        }

        public async Task<IFormFile> GetGoogleImageAsync(string fileName)
        {
            var googleCredential = GoogleCredential.FromJson($@"
                        {{
                            ""type"": ""service_account"",
                            ""project_id"": ""{_options.ProjectId}"",
                            ""private_key_id"": ""{_options.PrivateKeyId}"",
                            ""private_key"": ""{_options.PrivateKey}"",
                            ""client_email"": ""{_options.ClientEmail}"",
                            ""client_id"": ""{_options.ClientId}"",
                            ""auth_uri"": ""{_options.AuthUri}"",
                            ""token_uri"": ""{_options.TokenUri}"",
                            ""auth_provider_x509_cert_url"": ""{_options.AuthProviderX509CertUrl}"",
                            ""client_x509_cert_url"": ""{_options.ClientX509CertUrl}"",
                            ""universe_domain"": ""{_options.UniverseDomain}""
                        }}");

            var storage = StorageClient.Create(googleCredential);

            using (var memoryStream = new MemoryStream())
            {
                // Download the object from Google Cloud Storage
                var storageObject = await storage.GetObjectAsync(_options.BucketName, fileName);
                await storage.DownloadObjectAsync(_options.BucketName, fileName, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Infer content type
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fileName, out var contentType))
                {
                    contentType = "application/octet-stream"; // Default content type
                }

                // Create and return IFormFile
                var file = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };

                return file;
            }
        }

    }
}