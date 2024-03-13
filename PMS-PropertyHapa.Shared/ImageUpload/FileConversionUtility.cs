using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Shared.ImageUpload
{
    public class FileConversionUtility
    {
        private readonly string rootUploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public FileConversionUtility()
        {
            // Ensure the uploads directory exists
            if (!Directory.Exists(rootUploadDirectory))
            {
                Directory.CreateDirectory(rootUploadDirectory);
            }
        }

        public async Task<string> FileToBase64Async(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be null or empty.", nameof(file));
            }

            var filePath = Path.Combine(rootUploadDirectory, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            return Convert.ToBase64String(fileBytes);
        }

        public async Task<IFormFile> Base64ToFileAsync(string base64String, string fileName)
        {
            byte[] fileBytes = Convert.FromBase64String(base64String);
            var filePath = Path.Combine(rootUploadDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);

            var fileStream = new FileStream(filePath, FileMode.Open);
            IFormFile file = new FormFile(fileStream, 0, fileStream.Length, fileName, fileName);
            return file;
        }
    }
}
