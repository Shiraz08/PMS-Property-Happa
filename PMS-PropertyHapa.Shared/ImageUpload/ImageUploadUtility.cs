using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Shared.ImageUpload
{
    public static class ImageUploadUtility
    {
        public static async Task<(string fileName, string base64String)> UploadImageAsync(IFormFile file, string uploadDirectory)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty.", nameof(file));

            if (string.IsNullOrWhiteSpace(uploadDirectory))
                throw new ArgumentException("Upload directory cannot be null or empty.", nameof(uploadDirectory));

            // Combine the current directory path with the uploads directory
            string currentDirectory = Directory.GetCurrentDirectory();
            string combinedPath = Path.Combine("PMS-Property-Happa\\PMS-PropertyHapa.Shared", uploadDirectory);

            // Create the uploads directory if it does not exist
            if (!Directory.Exists(combinedPath))
                Directory.CreateDirectory(combinedPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(combinedPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var base64String = Convert.ToBase64String(await ReadAllBytesAsync(filePath));

            return (fileName, base64String);
        }

        private static async Task<byte[]> ReadAllBytesAsync(string path)
        {
            byte[] buffer;
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, (int)fileStream.Length);
            }
            return buffer;
        }
    }
}
