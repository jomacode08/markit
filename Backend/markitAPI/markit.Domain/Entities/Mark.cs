using markit.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace markit.Domain.Entities
{
    public class Mark : BaseModel
    {
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public int CreatorId { get; set; }

        public virtual Creator? Creator { get; set; }
        public virtual ICollection<Link>? Links { get; set; }
    }
}