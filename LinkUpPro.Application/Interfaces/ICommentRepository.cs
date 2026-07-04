using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<List<Comment>> GetAllByPostAsync(int postId);
    }
}
