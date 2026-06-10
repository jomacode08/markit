using markit.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace markit.Domain.Entities
{
    public class Block : BaseModel
    {
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public int NotebookId { get; set; }

        public virtual Notebook? Notebook { get; set; }
    }
}
