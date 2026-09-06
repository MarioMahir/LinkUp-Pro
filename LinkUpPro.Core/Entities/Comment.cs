using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsEdited { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int PostId { get; set; }

        public Post Post { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public int? ParentCommentId { get; set; }

        public Comment ParentComment { get; set; } = null!;

        public ICollection<Comment>? Replies { get; set; }
    }
}
