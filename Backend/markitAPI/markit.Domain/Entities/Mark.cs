using markit.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace markit.Domain.Entities
{
    public class Mark : BaseModel
    {
        [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
        [Required] public int CollectionId { get; set; }
        public bool IsFavorite { get; set; }
        public string? DocumentId { get; set; }
        public DateTime? LastSync { get; set; }
        public string? Emoji { get; set; }
        public bool? NameLess { get; set; }

        public virtual Collection? Collection { get; set; }
        public virtual ICollection<Link>? Links { get; set; }
        public virtual ICollection<Block>? Blocks { get; set; }
    }
}