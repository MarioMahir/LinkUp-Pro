using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}
