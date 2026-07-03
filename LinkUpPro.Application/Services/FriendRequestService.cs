using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Application.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IFriendRequestRepository _friendRequestRepository;

        private readonly IFriendshipRepository _friendshipRepository;

        private readonly IFriendService _friendService;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IMapper _mapper;

        public FriendRequestService(
            IFriendRequestRepository friendRequestRepository,
            IFriendshipRepository friendshipRepository,
            IFriendService friendService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _friendRequestRepository = friendRequestRepository;
            _friendshipRepository = friendshipRepository;
            _friendService = friendService;
            _userManager = userManager;
            _mapper = mapper;
        }

        private static ServiceResult Fail(string message) =>
            new() { Succeeded = false, Message = message };

        private static ServiceResult Ok(string message) =>
            new() { Succeeded = true, Message = message };

        public async Task<List<FriendRequestDto>> GetPendingReceivedAsync(string userId)
        {
            var requests = await _friendRequestRepository.GetPendingReceivedAsync(userId);

            var result = new List<FriendRequestDto>();

            foreach (var r in requests)
            {
                if (r.Sender == null || !r.Sender.EmailConfirmed)
                {
                    continue;
                }

                result.Add(new FriendRequestDto
                {
                    Id = r.Id,
                    OtherUserId = r.SenderId,
                    OtherUserName = r.Sender.UserName ?? string.Empty,
                    OtherFullName = $"{r.Sender.FirstName} {r.Sender.LastName}",
                    OtherProfilePicture = r.Sender.ProfilePictureUrl,
                    MutualFriendsCount = await _friendService.GetMutualFriendsCountAsync(userId, r.SenderId),
                    Status = r.Status,
                    SentDate = r.SentDate,
                    RespondedDate = r.RespondedDate,
                    IsSentByCurrentUser = false
                });
            }

            return result;
        }

        public async Task<List<FriendRequestDto>> GetSentAsync(string userId)
        {
            var requests = await _friendRequestRepository.GetVisibleSentAsync(userId);

            var result = new List<FriendRequestDto>();

            foreach (var r in requests)
            {
                if (r.Receiver == null || !r.Receiver.EmailConfirmed)
                {
                    continue;
                }

                result.Add(new FriendRequestDto
                {
                    Id = r.Id,
                    OtherUserId = r.ReceiverId,
                    OtherUserName = r.Receiver.UserName ?? string.Empty,
                    OtherFullName = $"{r.Receiver.FirstName} {r.Receiver.LastName}",
                    OtherProfilePicture = r.Receiver.ProfilePictureUrl,
                    MutualFriendsCount = await _friendService.GetMutualFriendsCountAsync(userId, r.ReceiverId),
                    Status = r.Status,
                    SentDate = r.SentDate,
                    RespondedDate = r.RespondedDate,
                    IsSentByCurrentUser = true
                });
            }

            return result;
        }

        public async Task<List<FriendRequestDto>> GetReceivedHistoryAsync(string userId)
        {
            var requests = await _friendRequestRepository.GetVisibleReceivedHistoryAsync(userId);

            var result = new List<FriendRequestDto>();

            foreach (var r in requests)
            {
                if (r.Sender == null || !r.Sender.EmailConfirmed)
                {
                    continue;
                }

                result.Add(new FriendRequestDto
                {
                    Id = r.Id,
                    OtherUserId = r.SenderId,
                    OtherUserName = r.Sender.UserName ?? string.Empty,
                    OtherFullName = $"{r.Sender.FirstName} {r.Sender.LastName}",
                    OtherProfilePicture = r.Sender.ProfilePictureUrl,
                    MutualFriendsCount = await _friendService.GetMutualFriendsCountAsync(userId, r.SenderId),
                    Status = r.Status,
                    SentDate = r.SentDate,
                    RespondedDate = r.RespondedDate,
                    IsSentByCurrentUser = false
                });
            }

            return result;
        }

        public async Task<int> GetPendingCountAsync(string userId)
        {
            var requests = await _friendRequestRepository.GetPendingReceivedAsync(userId);
            return requests.Count(r => r.Sender != null && r.Sender.EmailConfirmed);
        }

        public async Task<List<FriendDto>> GetAvailableUsersAsync(string userId, string? search = null)
        {
            var candidates = _userManager.Users
                .Where(u => u.Id != userId && u.EmailConfirmed)
                .ToList();

            var result = new List<FriendDto>();

            foreach (var candidate in candidates)
            {
                var friendship = await _friendshipRepository.GetBetweenAsync(userId, candidate.Id);
                if (friendship is { IsActive: true })
                {
                    continue;
                }

                var pending = await _friendRequestRepository.GetPendingBetweenAsync(userId, candidate.Id);
                if (pending != null)
                {
                    continue;
                }

                var dto = _mapper.Map<FriendDto>(candidate);
                dto.MutualFriendsCount = await _friendService.GetMutualFriendsCountAsync(userId, candidate.Id);
                result.Add(dto);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                result = result.Where(x => x.UserName.ToLowerInvariant().Contains(term)).ToList();
            }

            return result.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
        }

        public async Task<ServiceResult> SendRequestAsync(string senderId, string receiverId)
        {
            if (senderId == receiverId)
            {
                return Fail("No puede enviarse una solicitud de amistad a sí mismo.");
            }

            var sender = await _userManager.FindByIdAsync(senderId);
            var receiver = await _userManager.FindByIdAsync(receiverId);

            if (sender == null || receiver == null || !sender.EmailConfirmed || !receiver.EmailConfirmed)
            {
                return Fail("No se puede enviar la solicitud porque uno de los usuarios se encuentra inactivo.");
            }

            var friendship = await _friendshipRepository.GetBetweenAsync(senderId, receiverId);
            if (friendship is { IsActive: true })
            {
                return Fail("Este usuario ya forma parte de su lista de amigos.");
            }

            var existing = await _friendRequestRepository.GetPendingBetweenAsync(senderId, receiverId);
            if (existing != null)
            {
                return existing.SenderId == senderId
                    ? Fail("Ya envió una solicitud de amistad a este usuario y se encuentra pendiente de respuesta.")
                    : Fail("Este usuario ya le envió una solicitud de amistad. Debe aceptarla o rechazarla desde sus solicitudes pendientes.");
            }

            try
            {
                await _friendRequestRepository.AddAsync(new FriendRequest
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Status = "Pending",
                    PairKey = FriendRequest.MakePairKey(senderId, receiverId),
                    SentDate = DateTime.UtcNow
                });

                await _friendRequestRepository.SaveChangesAsync();
            }
            catch (LinkUpPro.Application.Exceptions.ConcurrencyConflictException)
            {
                // Otra solicitud concurrente ya insertó el mismo par en espera de respuesta.
                return Fail("Ya existe una solicitud de amistad pendiente entre ambos usuarios.");
            }

            return Ok("La solicitud de amistad fue enviada correctamente.");
        }

        public async Task<ServiceResult> AcceptRequestAsync(int requestId, string currentUserId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);

            if (request == null || request.Status != "Pending")
            {
                return Fail("Esta solicitud ya no se encuentra disponible para ser aceptada.");
            }

            if (request.ReceiverId != currentUserId)
            {
                return Fail("No posee permisos para realizar esta acción sobre la solicitud.");
            }

            var sender = await _userManager.FindByIdAsync(request.SenderId);
            var receiver = await _userManager.FindByIdAsync(request.ReceiverId);

            if (sender == null || receiver == null || !sender.EmailConfirmed || !receiver.EmailConfirmed)
            {
                return Fail("No se puede aceptar la solicitud porque uno de los usuarios se encuentra inactivo.");
            }

            request.Status = "Accepted";
            request.RespondedDate = DateTime.UtcNow;
            _friendRequestRepository.Update(request);

            var friendship = await _friendshipRepository.GetBetweenAsync(request.SenderId, request.ReceiverId);

            if (friendship == null)
            {
                var orderedIds = string.CompareOrdinal(request.SenderId, request.ReceiverId) < 0
                    ? (request.SenderId, request.ReceiverId)
                    : (request.ReceiverId, request.SenderId);

                await _friendshipRepository.AddAsync(new Friendship
                {
                    UserOneId = orderedIds.Item1,
                    UserTwoId = orderedIds.Item2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                });
            }
            else
            {
                friendship.IsActive = true;
                friendship.RemovedDate = null;
                _friendshipRepository.Update(friendship);
            }

            await _friendRequestRepository.SaveChangesAsync();

            return Ok("La solicitud fue aceptada correctamente.");
        }

        public async Task<ServiceResult> RejectRequestAsync(int requestId, string currentUserId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);

            if (request == null || request.Status != "Pending")
            {
                return Fail("Esta solicitud ya no se encuentra disponible.");
            }

            if (request.ReceiverId != currentUserId)
            {
                return Fail("No posee permisos para realizar esta acción sobre la solicitud.");
            }

            request.Status = "Rejected";
            request.RespondedDate = DateTime.UtcNow;
            _friendRequestRepository.Update(request);
            await _friendRequestRepository.SaveChangesAsync();

            return Ok("La solicitud fue rechazada correctamente.");
        }

        public async Task<ServiceResult> CancelRequestAsync(int requestId, string currentUserId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);

            if (request == null || request.Status != "Pending")
            {
                return Fail("Esta solicitud ya no se encuentra disponible.");
            }

            if (request.SenderId != currentUserId)
            {
                return Fail("No posee permisos para realizar esta acción sobre la solicitud.");
            }

            request.Status = "Cancelled";
            request.RespondedDate = DateTime.UtcNow;
            _friendRequestRepository.Update(request);
            await _friendRequestRepository.SaveChangesAsync();

            return Ok("La solicitud fue cancelada correctamente.");
        }

        public async Task<ServiceResult> HideFromHistoryAsync(int requestId, string currentUserId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);

            if (request == null || (request.Status != "Accepted" && request.Status != "Rejected"))
            {
                return Fail("Esta solicitud ya no se encuentra disponible.");
            }

            if (request.SenderId != currentUserId)
            {
                return Fail("No posee permisos para realizar esta acción sobre la solicitud.");
            }

            request.IsHiddenFromSender = true;
            _friendRequestRepository.Update(request);
            await _friendRequestRepository.SaveChangesAsync();

            return Ok("La solicitud fue eliminada de su historial.");
        }

        public async Task<ServiceResult> HideFromReceiverHistoryAsync(int requestId, string currentUserId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);

            if (request == null || (request.Status != "Accepted" && request.Status != "Rejected"))
            {
                return Fail("Esta solicitud ya no se encuentra disponible.");
            }

            if (request.ReceiverId != currentUserId)
            {
                return Fail("No posee permisos para realizar esta acción sobre la solicitud.");
            }

            request.IsHiddenFromReceiver = true;
            _friendRequestRepository.Update(request);
            await _friendRequestRepository.SaveChangesAsync();

            return Ok("La solicitud fue eliminada de su historial.");
        }
    }
}
