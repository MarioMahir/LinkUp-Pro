using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Application.ViewModels
{
    public class ResendActivationViewModel
    {
        [Required]
        public string UserName { get; set; }
    }
}
