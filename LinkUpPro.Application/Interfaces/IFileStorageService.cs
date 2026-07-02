using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(IFormFile file, string subfolder);

        void DeleteIfExists(string? relativeUrl);
    }
}
