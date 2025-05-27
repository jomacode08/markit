using System.ComponentModel.DataAnnotations;
using markit.Domain.Common;

namespace markit.Domain.Entities
{
    public class Collection : BaseModel
    {
        [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
        public string? Path { get; set; } = string.Empty;
        [Required] public bool IsMain { get; set; }
        public int? ParentId { get; set; }
        [Required] public int CreatorId { get; set; }
        [Required] public string PathNames {  get; set; } = string.Empty;

        public virtual Collection? Parent { get; set; }
        public virtual Creator? Creator { get; set; }
        public virtual ICollection<Collection>? SubCollections { get; set; }
        public virtual ICollection<Mark>? Marks { get; set; }
    }
}
