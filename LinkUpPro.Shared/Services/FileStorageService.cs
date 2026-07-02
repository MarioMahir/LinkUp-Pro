using LinkUpPro.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Shared.Services
{
    public class FileStorageService : IFileStorageService
    {
        public async Task<string> SaveAsync(IFormFile file, string subfolder)
        {
            var extension = Path.GetExtension(file.FileName);

            var folder = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "images", subfolder);

            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + extension;
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{subfolder}/{fileName}";
        }

        public void DeleteIfExists(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
            {
                return;
            }

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(path))
            {
                try { File.Delete(path); } catch { /* best effort */ }
            }
        }
    }
}
