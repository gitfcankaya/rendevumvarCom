using System.IO;
using System.Threading.Tasks;

namespace RendevumVar.Application.Interfaces
{
    public interface IImageService
    {
        /// <summary>
        /// Uploads an image to storage and returns the URL
        /// </summary>
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder);

        /// <summary>
        /// Validates image file format and size
        /// </summary>
        Task<bool> ValidateImageAsync(Stream imageStream, string fileName);

        /// <summary>
        /// Deletes an image from storage
        /// </summary>
        Task<bool> DeleteImageAsync(string imageUrl);

        /// <summary>
        /// Gets the full URL for an image
        /// </summary>
        string GetImageUrl(string fileName, string folder);
    }
}
