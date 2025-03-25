using markit.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace markit.Domain.Entities
{
    public class Block : BaseModel
    {
        [Required]
        public int Cols { get; set; }
        [Required]
        public BackColors Color { get; set; }
        [MaxLength(255)]
        public string? Title { get; set; }
        public string? Content { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public int MarkId { get; set; }

        public virtual Mark? Mark { get; set; }
    }

    public enum BackColors
    {
        Neutral = 1,
        Purple = 2,
        Red = 3,
        Transparent = 4
    }
}
