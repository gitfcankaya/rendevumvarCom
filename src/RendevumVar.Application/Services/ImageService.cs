using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RendevumVar.Application.Interfaces;

namespace RendevumVar.Application.Services
{
    public class ImageService : IImageService
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string _uploadsPath;

        public ImageService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Get uploads path from configuration or use default
            _uploadsPath = _configuration["ImageStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            // Ensure directory exists
            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
            }
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder)
        {
            // Validate the image
            if (!await ValidateImageAsync(imageStream, fileName))
            {
                throw new InvalidOperationException("Invalid image file");
            }

            // Generate unique filename
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Determine storage path
            var uploadsFolder = Path.Combine(_uploadsPath, folder);

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Reset stream position
            imageStream.Position = 0;

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Return URL
            return GetImageUrl(uniqueFileName, folder);
        }

        public async Task<bool> ValidateImageAsync(Stream imageStream, string fileName)
        {
            // Check extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return false;
            }

            // Check file size
            if (imageStream.Length > _maxFileSize)
            {
                return false;
            }

            // Additional validation: Check if it's a valid image by reading header
            // For now, we'll just check extension and size
            await Task.CompletedTask; // To make async meaningful
            return true;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                // Extract filename from URL
                var uri = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                var path = uri.IsAbsoluteUri ? uri.LocalPath : imageUrl;

                var fileName = Path.GetFileName(path);
                var folder = Path.GetFileName(Path.GetDirectoryName(path));

                var filePath = Path.Combine(_uploadsPath, folder ?? "", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    await Task.CompletedTask;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetImageUrl(string fileName, string folder)
        {
            // For local storage, return relative URL
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
            return $"{baseUrl}/images/{folder}/{fileName}";
        }
    }
}
