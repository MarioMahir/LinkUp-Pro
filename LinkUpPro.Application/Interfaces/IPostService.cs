using LinkUpPro.Application.ViewModels;

namespace LinkUpPro.Application.Interfaces
{
    public interface IPostService
    {
        Task CreateAsync( SavePostViewModel vm, string userId);

        Task<List<PostViewModel>>
            GetAllByUserAsync(string userId);
    }
}