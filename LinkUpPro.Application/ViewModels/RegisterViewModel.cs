using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [RegularExpression(@"^(809|829|849)-\d{3}-\d{4}$",ErrorMessage = "Formato inválido")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La imagen es requerida")]
        public IFormFile ProfilePicture { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Debe tener mayúscula, minúscula, número y carácter especial")]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "La contraseña y confirmación no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}
