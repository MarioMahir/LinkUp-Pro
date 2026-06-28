using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Application.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido" )]
        public string UserName { get; set; }
    }
}