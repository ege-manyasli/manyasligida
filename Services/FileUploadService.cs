using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace manyasligida.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName = "products");
        Task<string> UploadVideoAsync(IFormFile file, string folderName = "videos");
        Task<bool> DeleteImageAsync(string imagePath);
        bool IsValidImage(IFormFile file);
        bool IsValidVideo(IFormFile file);
        Task<(int width, int height)> GetImageDimensionsAsync(IFormFile file);
        Task<bool> IsResolutionSufficientAsync(IFormFile file, int minWidth, int minHeight);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm" };
        private readonly long _maxImageFileSize = 5 * 1024 * 1024; // 5MB
        private readonly long _maxVideoFileSize = 50 * 1024 * 1024; // 50MB

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

        public async Task<string> UploadVideoAsync(IFormFile file, string folderName = "videos")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Video dosyası seçilmedi.");

            if (!IsValidVideo(file))
                throw new ArgumentException("Geçersiz video formatı. Sadece MP4, AVI, MOV, WMV, FLV ve WEBM dosyaları kabul edilir.");

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
            if (file.Length > _maxImageFileSize)
                return false;

            // Dosya uzantısı kontrolü
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
                return false;

            // MIME type kontrolü
            if (!file.ContentType.StartsWith("image/"))
                return false;

            return true;
        }

        public bool IsValidVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Dosya boyutu kontrolü
            if (file.Length > _maxVideoFileSize)
                return false;

            // Dosya uzantısı kontrolü
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedVideoExtensions.Contains(extension))
                return false;

            // MIME type kontrolü
            if (!file.ContentType.StartsWith("video/"))
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