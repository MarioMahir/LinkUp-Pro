using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Application.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }

        public string Token { get; set; }

        [Required( ErrorMessage = "La nueva contraseña es requerida" )]

        [DataType(DataType.Password)]

        [RegularExpression(
            @"^(?=.*[a-z])
        (?=.*[A-Z])
        (?=.*\d)
        (?=.*[\W_])
        .{8,}$",
            ErrorMessage = "Debe tener mayúscula, minúscula, número y carácter especial")]

        public string NewPassword { get; set; }

        [Required]

        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden" )]

        [DataType(DataType.Password)]

        public string ConfirmPassword { get; set; }
    }
}
