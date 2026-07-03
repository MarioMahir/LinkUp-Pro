using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;

        private readonly ICommentRepository _commentRepository;

        private readonly IPostReactionRepository _reactionRepository;

        private readonly IFriendshipRepository _friendshipRepository;

        private readonly INotificationService _notificationService;

        private readonly IFileStorageService _fileStorageService;

        private readonly IMapper _mapper;

        private static readonly string[] AllowedImageExtensions =
            { ".jpg", ".jpeg", ".png", ".webp" };

        public PostService(
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IPostReactionRepository reactionRepository,
            IFriendshipRepository friendshipRepository,
            INotificationService notificationService,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _reactionRepository = reactionRepository;
            _friendshipRepository = friendshipRepository;
            _notificationService = notificationService;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        private static ServiceResult Fail(string message) =>
            new() { Succeeded = false, Message = message };

        private static ServiceResult Ok(string? message = null) =>
            new() { Succeeded = true, Message = message ?? string.Empty };

        private static ServiceResult? ValidateContent(SavePostViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Content))
            {
                return Fail("Debe ingresar el contenido de la publicación.");
            }

            if (vm.Content.Trim().Length > 1000)
            {
                return Fail("El contenido no puede superar los 1000 caracteres.");
            }

            if (vm.ContentType != "Image" && vm.ContentType != "YouTube")
            {
                return Fail("Debe seleccionar un tipo de contenido válido.");
            }

            if (vm.Privacy != "Friends" && vm.Privacy != "OnlyMe")
            {
                return Fail("Debe seleccionar una privacidad válida.");
            }

            if (vm.ContentType == "Image" && vm.ImageFile != null)
            {
                var extension = Path.GetExtension(vm.ImageFile.FileName).ToLowerInvariant();

                if (!AllowedImageExtensions.Contains(extension))
                {
                    return Fail("El archivo seleccionado no tiene un formato de imagen válido.");
                }

                if (vm.ImageFile.Length > 5242880)
                {
                    return Fail("La imagen no puede superar los 5 MB.");
                }

                if (!FileSignatureValidator.HasValidImageSignature(vm.ImageFile))
                {
                    return Fail("El archivo seleccionado no tiene un formato de imagen válido.");
                }
            }

            if (vm.ContentType == "YouTube")
            {
                if (string.IsNullOrWhiteSpace(vm.YoutubeUrl) ||
                    (!vm.YoutubeUrl.Contains("youtube.com") && !vm.YoutubeUrl.Contains("youtu.be")))
                {
                    return Fail("Debe ingresar un enlace válido de YouTube.");
                }
            }

            return null;
        }

        // ----- Create / Edit / Delete -----

        public async Task<ServiceResult> CreateAsync(SavePostViewModel vm, string userId)
        {
            var validation = ValidateContent(vm);
            if (validation != null)
            {
                return validation;
            }

            if (vm.ContentType == "Image" && vm.ImageFile == null)
            {
                return Fail("Debe seleccionar una imagen para crear la publicación.");
            }

            string? imagePath = null;

            if (vm.ContentType == "Image" && vm.ImageFile != null)
            {
                imagePath = await _fileStorageService.SaveAsync(vm.ImageFile, "posts");
            }

            var post = new Post
            {
                Content = vm.Content.Trim(),
                ContentType = vm.ContentType,
                ImageUrl = imagePath,
                YoutubeUrl = vm.ContentType == "YouTube" ? vm.YoutubeUrl : null,
                Privacy = vm.Privacy,
                AllowComments = vm.AllowComments,
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _postRepository.AddAsync(post);
            await _postRepository.SaveChangesAsync();

            return Ok("La publicación fue creada correctamente.");
        }

        public async Task<ServiceResult> EditAsync(SavePostViewModel vm, string userId)
        {
            var post = await _postRepository.GetByIdAsync(vm.Id);

            if (post == null || post.IsDeleted)
            {
                return Fail("Esta publicación ya no se encuentra disponible.");
            }

            if (post.UserId != userId)
            {
                return Fail("No posee permisos para editar esta publicación.");
            }

            var validation = ValidateContent(vm);
            if (validation != null)
            {
                return validation;
            }

            var previousImageUrl = post.ImageUrl;

            if (vm.ContentType == "Image")
            {
                if (vm.ImageFile != null)
                {
                    post.ImageUrl = await _fileStorageService.SaveAsync(vm.ImageFile, "posts");
                }
                else if (post.ContentType != "Image")
                {
                    return Fail("Debe seleccionar una imagen para crear la publicación.");
                }

                post.YoutubeUrl = null;
            }
            else
            {
                post.YoutubeUrl = vm.YoutubeUrl;
                post.ImageUrl = null;
            }

            post.Content = vm.Content.Trim();
            post.ContentType = vm.ContentType;
            post.Privacy = vm.Privacy;
            post.AllowComments = vm.AllowComments;
            post.IsEdited = true;
            post.UpdatedDate = DateTime.UtcNow;

            _postRepository.Update(post);
            await _postRepository.SaveChangesAsync();

            if (previousImageUrl != post.ImageUrl)
            {
                _fileStorageService.DeleteIfExists(previousImageUrl);
            }

            return Ok("La publicación fue actualizada correctamente.");
        }

        public async Task<ServiceResult> DeleteAsync(int postId, string userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);

            if (post == null || post.IsDeleted)
            {
                return Fail("Esta publicación ya no se encuentra disponible.");
            }

            if (post.UserId != userId)
            {
                return Fail("No posee permisos para eliminar esta publicación.");
            }

            post.IsDeleted = true;
            _postRepository.Update(post);
            await _postRepository.SaveChangesAsync();

            return Ok("La publicación fue eliminada correctamente.");
        }

        // ----- Visibility -----

        private async Task<bool> CanViewPostAsync(Post post, string viewerId)
        {
            if (post.UserId == viewerId)
            {
                return true;
            }

            if (post.Privacy != "Friends")
            {
                return false;
            }

            if (post.User != null && !post.User.EmailConfirmed)
            {
                return false;
            }

            var friendship = await _friendshipRepository.GetBetweenAsync(post.UserId, viewerId);
            return friendship is { IsActive: true };
        }

        public async Task<PostDto?> GetByIdAsync(int postId, string viewerId)
        {
            var post = await _postRepository.GetByIdWithDetailsAsync(postId);

            if (post == null || post.IsDeleted)
            {
                return null;
            }

            if (!await CanViewPostAsync(post, viewerId))
            {
                return null;
            }

            return await ToDtoAsync(post, viewerId);
        }

        // ----- Feeds -----

        private static List<Post> ApplyFilter(IEnumerable<Post> posts, PostFilterDto? filter)
        {
            var query = posts.AsEnumerable();

            if (filter == null)
            {
                return query.ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var term = filter.SearchText.Trim().ToLowerInvariant();
                query = query.Where(p => p.Content.ToLowerInvariant().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.ContentType) && filter.ContentType != "All")
            {
                query = query.Where(p => p.ContentType == filter.ContentType);
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(p => p.CreatedDate.Date >= filter.DateFrom.Value.Date);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(p => p.CreatedDate.Date <= filter.DateTo.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.EditedStatus) && filter.EditedStatus != "All")
            {
                query = query.Where(p => filter.EditedStatus == "Edited" ? p.IsEdited : !p.IsEdited);
            }

            return query.ToList();
        }

        public async Task<List<PostDto>> GetOwnFeedAsync(string userId, PostFilterDto? filter = null)
        {
            var posts = await _postRepository.GetAllByUserAsync(userId);
            posts = ApplyFilter(posts, filter);

            var result = new List<PostDto>();
            foreach (var post in posts.OrderByDescending(p => p.CreatedDate))
            {
                result.Add(await ToDtoAsync(post, userId));
            }

            return result;
        }

        public async Task<List<PostDto>> GetFeedByAuthorsAsync(
            List<string> authorIds,
            string viewerId,
            PostFilterDto? filter = null)
        {
            var posts = new List<Post>();

            foreach (var authorId in authorIds)
            {
                posts.AddRange(await _postRepository.GetVisibleByFriendAsync(authorId));
            }

            posts = ApplyFilter(posts, filter);

            var result = new List<PostDto>();
            foreach (var post in posts.OrderByDescending(p => p.CreatedDate))
            {
                result.Add(await ToDtoAsync(post, viewerId));
            }

            return result;
        }

        public async Task<PostDto> ToDtoAsync(Post post, string viewerId)
        {
            var dto = _mapper.Map<PostDto>(post);

            var reaction = await _reactionRepository.GetByUserAndPostAsync(viewerId, post.Id);
            dto.CurrentUserReactionIsLike = reaction?.IsLike;

            var allComments = await _commentRepository.GetAllByPostAsync(post.Id);

            var friendships = await _friendshipRepository.GetActiveByUserAsync(viewerId);
            var friendIds = friendships
                .Select(f => f.UserOneId == viewerId ? f.UserTwoId : f.UserOneId)
                .ToHashSet();

            var tree = BuildCommentTree(allComments, null, viewerId, friendIds);
            dto.Comments = PruneDeletedComments(tree);

            return dto;
        }

        private static List<CommentDto> BuildCommentTree(
            List<Comment> allComments,
            int? parentId,
            string viewerId,
            HashSet<string> viewerFriendIds)
        {
            return allComments
                .Where(c => c.ParentCommentId == parentId)
                .OrderBy(c => c.CreatedDate)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    IsEdited = c.IsEdited,
                    IsDeleted = c.IsDeleted,
                    CreatedDate = c.CreatedDate,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    UserName = c.User?.UserName ?? string.Empty,
                    ProfilePicture = c.User?.ProfilePictureUrl,
                    AuthorIsFriendOfViewer = c.UserId == viewerId || viewerFriendIds.Contains(c.UserId),
                    ParentCommentId = c.ParentCommentId,
                    Replies = BuildCommentTree(allComments, c.Id, viewerId, viewerFriendIds)
                })
                .ToList();
        }

        private static List<CommentDto> PruneDeletedComments(List<CommentDto> nodes)
        {
            var result = new List<CommentDto>();

            foreach (var node in nodes)
            {
                node.Replies = PruneDeletedComments(node.Replies);

                if (node.IsDeleted)
                {
                    if (node.Replies.Count > 0)
                    {
                        node.Content = "Este comentario fue eliminado.";
                        result.Add(node);
                    }
                }
                else
                {
                    result.Add(node);
                }
            }

            return result;
        }

        // ----- Comments -----

        public async Task<ServiceResult> AddCommentAsync(
            int postId,
            string content,
            string userId,
            int? parentCommentId = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Fail("Debe ingresar el contenido del comentario.");
            }

            if (content.Trim().Length > 500)
            {
                return Fail("El comentario no puede superar los 500 caracteres.");
            }

            var post = await _postRepository.GetByIdWithDetailsAsync(postId);

            if (post == null || post.IsDeleted)
            {
                return Fail("Esta publicación ya no se encuentra disponible.");
            }

            if (!await CanViewPostAsync(post, userId))
            {
                return Fail("No posee permisos para visualizar esta publicación.");
            }

            if (!post.AllowComments)
            {
                return Fail("Los comentarios están desactivados para esta publicación.");
            }

            Comment? parent = null;

            if (parentCommentId.HasValue)
            {
                parent = await _commentRepository.GetByIdAsync(parentCommentId.Value);

                if (parent == null || parent.PostId != postId)
                {
                    return Fail("El comentario al que intenta responder ya no está disponible.");
                }
            }

            var comment = new Comment
            {
                Content = content.Trim(),
                PostId = postId,
                UserId = userId,
                ParentCommentId = parentCommentId,
                CreatedDate = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            var recipientId = parent != null ? parent.UserId : post.UserId;
            var type = parent != null ? "Reply" : "Comment";

            await _notificationService.CreateAsync(recipientId, userId, type, postId, comment.Id);

            return Ok("El comentario fue publicado correctamente.");
        }

        public async Task<ServiceResult> EditCommentAsync(int commentId, string content, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);

            if (comment == null || comment.IsDeleted)
            {
                return Fail("Este comentario ya no se encuentra disponible.");
            }

            if (comment.UserId != userId)
            {
                return Fail("No posee permisos para editar este contenido.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Fail("Debe ingresar el contenido del comentario.");
            }

            if (content.Trim().Length > 500)
            {
                return Fail("El comentario no puede superar los 500 caracteres.");
            }

            comment.Content = content.Trim();
            comment.IsEdited = true;

            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();

            return Ok("El comentario fue actualizado correctamente.");
        }

        public async Task<ServiceResult> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);

            if (comment == null || comment.IsDeleted)
            {
                return Fail("Este comentario ya no se encuentra disponible.");
            }

            if (comment.UserId != userId)
            {
                return Fail("No posee permisos para eliminar este contenido.");
            }

            comment.IsDeleted = true;
            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();

            return Ok("El comentario fue eliminado correctamente.");
        }

        // ----- Reactions -----

        public async Task<ServiceResult> SetReactionAsync(int postId, string userId, bool isLike)
        {
            var post = await _postRepository.GetByIdWithDetailsAsync(postId);

            if (post == null || post.IsDeleted)
            {
                return Fail("Esta publicación ya no se encuentra disponible.");
            }

            if (!await CanViewPostAsync(post, userId))
            {
                return Fail("No posee permisos para visualizar esta publicación.");
            }

            var existing = await _reactionRepository.GetByUserAndPostAsync(userId, postId);
            var wasNew = existing == null;
            var previousIsLike = existing?.IsLike;
            var reactionChanged = existing != null && previousIsLike != isLike;

            if (existing != null)
            {
                existing.IsLike = isLike;
                _reactionRepository.Update(existing);
            }
            else
            {
                await _reactionRepository.AddAsync(new PostReaction
                {
                    PostId = postId,
                    UserId = userId,
                    IsLike = isLike
                });
            }

            await _reactionRepository.SaveChangesAsync();

            // Se notifica tanto al registrar una reacción nueva como al
            // cambiarla (Me gusta <-> No me gusta), pero nunca al eliminarla
            // ni al volver a marcar la misma reacción ya existente.
            if (wasNew || reactionChanged)
            {
                await _notificationService.CreateAsync(
                    post.UserId, userId, "Reaction", postId, null, isLike);
            }

            return Ok();
        }

        public async Task<ServiceResult> RemoveReactionAsync(int postId, string userId)
        {
            var existing = await _reactionRepository.GetByUserAndPostAsync(userId, postId);

            if (existing == null)
            {
                return Ok();
            }

            _reactionRepository.Remove(existing);
            await _reactionRepository.SaveChangesAsync();

            return Ok();
        }
    }
}
