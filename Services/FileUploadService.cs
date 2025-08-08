using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace manyasligida.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName = "products");
        Task<bool> DeleteImageAsync(string imagePath);
        bool IsValidImage(IFormFile file);
        Task<(int width, int height)> GetImageDimensionsAsync(IFormFile file);
        Task<bool> IsResolutionSufficientAsync(IFormFile file, int minWidth, int minHeight);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName = "products")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Dosya seçilmedi.");

            if (!IsValidImage(file))
                throw new ArgumentException("Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");

            // Klasör yolu oluştur
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Benzersiz dosya adı oluştur
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Dosyayı kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Web erişim URL'ini döndür
            return $"/uploads/{folderName}/{fileName}";
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Dosya boyutu kontrolü
            if (file.Length > _maxFileSize)
                return false;

            // Dosya uzantısı kontrolü
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // MIME type kontrolü
            if (!file.ContentType.StartsWith("image/"))
                return false;

            return true;
        }

        public async Task<(int width, int height)> GetImageDimensionsAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (0, 0);

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            // System.Drawing.Common is available on Windows hosting
            using var image = System.Drawing.Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: true);
            return (image.Width, image.Height);
        }

        public async Task<bool> IsResolutionSufficientAsync(IFormFile file, int minWidth, int minHeight)
        {
            var (w, h) = await GetImageDimensionsAsync(file);
            return w >= minWidth && h >= minHeight;
        }
    }
} 