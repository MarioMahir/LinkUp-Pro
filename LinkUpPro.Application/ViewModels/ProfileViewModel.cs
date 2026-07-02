using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su nombre.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su apellido.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar un número telefónico válido de República Dominicana.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string? CurrentProfilePictureUrl { get; set; }

        public IFormFile? ProfilePictureFile { get; set; }

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
