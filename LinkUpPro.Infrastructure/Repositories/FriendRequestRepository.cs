using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Repositories
{
    public class FriendRequestRepository
        : GenericRepository<FriendRequest>, IFriendRequestRepository
    {
        public FriendRequestRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<FriendRequest?> GetPendingBetweenAsync(string userIdA, string userIdB)
        {
            return await _context.FriendRequests
                .FirstOrDefaultAsync(r =>
                    r.Status == "Pending" &&
                    ((r.SenderId == userIdA && r.ReceiverId == userIdB) ||
                     (r.SenderId == userIdB && r.ReceiverId == userIdA)));
        }

        public async Task<List<FriendRequest>> GetPendingReceivedAsync(string userId)
        {
            return await _context.FriendRequests
                .Include(r => r.Sender)
                .Where(r =>
                    r.ReceiverId == userId &&
                    r.Status == "Pending")
                .OrderByDescending(r => r.SentDate)
                .ToListAsync();
        }

        public async Task<List<FriendRequest>> GetVisibleSentAsync(string userId)
        {
            return await _context.FriendRequests
                .Include(r => r.Receiver)
                .Where(r =>
                    r.SenderId == userId &&
                    r.Status != "Cancelled" &&
                    !(r.Status != "Pending" && r.IsHiddenFromSender))
                .OrderByDescending(r => r.SentDate)
                .ToListAsync();
        }

        public async Task<List<FriendRequest>> GetVisibleReceivedHistoryAsync(string userId)
        {
            return await _context.FriendRequests
                .Include(r => r.Sender)
                .Where(r =>
                    r.ReceiverId == userId &&
                    (r.Status == "Accepted" || r.Status == "Rejected") &&
                    !r.IsHiddenFromReceiver)
                .OrderByDescending(r => r.RespondedDate ?? r.SentDate)
                .ToListAsync();
        }
    }
}
