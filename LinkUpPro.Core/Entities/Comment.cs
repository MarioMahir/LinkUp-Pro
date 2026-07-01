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

        public string Content { get; set; }

        public bool IsEdited { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int PostId { get; set; }

        public Post Post { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int? ParentCommentId { get; set; }

        public Comment ParentComment { get; set; }

        public ICollection<Comment>? Replies { get; set; }
    }
}
