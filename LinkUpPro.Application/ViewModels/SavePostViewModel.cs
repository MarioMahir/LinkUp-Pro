using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels
{
    public class SavePostViewModel
    {
        public int Id { get; set; }

        [Required(
            ErrorMessage =
            "Debe ingresar el contenido de la publicación.")]
        [StringLength(
            1000,
            ErrorMessage =
            "Máximo 1000 caracteres.")]
        public string Content { get; set; }

        [Required]
        public string ContentType { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? CurrentImageUrl { get; set; }

        public string? YoutubeUrl { get; set; }

        [Required]
        public string Privacy { get; set; }

        public bool AllowComments { get; set; } = true;
    }
}