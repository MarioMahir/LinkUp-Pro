using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Entities
{
    public class PostReaction
    {
        public int Id { get; set; }

        public bool IsLike { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public int PostId { get; set; }

        public Post Post { get; set; } = null!;
    }
}
