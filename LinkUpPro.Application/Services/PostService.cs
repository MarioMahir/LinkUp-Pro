using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository
            _repository;

        public PostService(
            IPostRepository repository)
        {
            _repository =
                repository;
        }

        public async Task CreateAsync(
            SavePostViewModel vm,
            string userId)
        {
            string? imagePath = null;

            // Validaciones

            if (string.IsNullOrWhiteSpace(
                vm.Content))
            {
                throw new Exception(
                    "Debe ingresar el contenido de la publicación.");
            }

            if (vm.ContentType == "Image")
            {
                if (vm.ImageFile == null)
                {
                    throw new Exception(
                        "Debe seleccionar una imagen para crear la publicación.");
                }

                var allowedExtensions =
                    new[]
                    {
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".webp"
                    };

                var extension =
                    Path.GetExtension(
                        vm.ImageFile.FileName)
                    .ToLower();

                if (!allowedExtensions
                    .Contains(extension))
                {
                    throw new Exception(
                        "El archivo seleccionado no tiene un formato de imagen válido.");
                }

                if (vm.ImageFile.Length >
                    5242880)
                {
                    throw new Exception(
                        "La imagen no puede superar los 5 MB.");
                }

                var folder =
                    Path.Combine(
                        Directory
                        .GetCurrentDirectory(),
                        "wwwroot",
                        "images",
                        "posts");

                Directory
                    .CreateDirectory(
                        folder);

                var fileName =
                    Guid.NewGuid()
                    + extension;

                var path =
                    Path.Combine(
                        folder,
                        fileName);

                using (var stream =
                    new FileStream(
                        path,
                        FileMode.Create))
                {
                    await vm.ImageFile
                        .CopyToAsync(
                            stream);
                }

                imagePath =
                    "/images/posts/"
                    + fileName;
            }

            if (vm.ContentType ==
                "YouTube")
            {
                if (string
                    .IsNullOrWhiteSpace(
                        vm.YoutubeUrl))
                {
                    throw new Exception(
                        "Debe ingresar un enlace válido de YouTube.");
                }

                if (!vm.YoutubeUrl
                    .Contains(
                        "youtube.com")
                    &&
                    !vm.YoutubeUrl
                    .Contains(
                        "youtu.be"))
                {
                    throw new Exception(
                        "Debe ingresar un enlace válido de YouTube.");
                }
            }

            Post post =
                new()
                {
                    Content =
                        vm.Content,

                    ContentType =
                        vm.ContentType,

                    ImageUrl =
                        imagePath,

                    YoutubeUrl =
                        vm.YoutubeUrl,

                    Privacy =
                        vm.Privacy,

                    AllowComments =
                        vm.AllowComments,

                    UserId =
                        userId
                };

            await _repository
                .AddAsync(post);

            await _repository
                .SaveChangesAsync();
        }

        public async Task<List<PostViewModel>>
            GetAllByUserAsync(
                string userId)
        {
            var posts =
                await _repository
                .GetAllByUserAsync(
                    userId);

            return posts
                .Select(x =>
                new PostViewModel
                {
                    Id =
                        x.Id,

                    Content =
                        x.Content,

                    ImageUrl =
                        x.ImageUrl,

                    YoutubeUrl =
                        x.YoutubeUrl,

                    Privacy =
                        x.Privacy,

                    AllowComments =
                        x.AllowComments,

                    IsEdited =
                        x.IsEdited,

                    CreatedDate =
                        x.CreatedDate,

                    UserName =
                        x.User.UserName,

                    ProfilePicture =
                        x.User.ProfilePictureUrl,

                    LikesCount =
                        x.Reactions
                        .Count(
                            r => r.IsLike),

                    DislikesCount =
                        x.Reactions
                        .Count(
                            r => !r.IsLike)

                })
                .ToList();
        }
    }
}