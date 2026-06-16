using System.ComponentModel.DataAnnotations;
using markit.Domain.Common;

namespace markit.Domain.Entities
{
    public class Collection : BaseModel
    {
        [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
        [Required] public bool IsMain { get; set; }
        [Required] public string UserId { get; set; } = string.Empty;
        [Required] public string PathNames {  get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public int? ParentId { get; set; }
        public string? Path { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        [MaxLength(1500)] public string? Description {  get; set; }

        public virtual Collection? Parent { get; set; }
        public virtual ICollection<Collection>? SubCollections { get; set; }
        public virtual ICollection<Notebook>? Notebooks { get; set; }
    }
}
