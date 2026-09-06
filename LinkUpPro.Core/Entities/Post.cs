using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinkUpPro.Core.Entities
{
    public class Post
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? YoutubeUrl { get; set; }

        public string Privacy { get; set; } = string.Empty;

        public bool AllowComments { get; set; }

        public bool IsEdited { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    }

}

