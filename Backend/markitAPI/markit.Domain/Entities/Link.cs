using markit.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace markit.Domain.Entities
{
    public class Link : BaseModel
    {
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Description {  get; set; } = string.Empty;

        [Required]
        public string Url { get; set; } = string.Empty;

        [Required]
        public LinkType LinkType { get; set; }

        [Required]
        public int MarkId { get; set; }

        public virtual Mark? Mark { get; set; }
    }

    public enum LinkType
    {
        x = 1,
        Youtube = 2,
        Facebook = 3,
        Instagram = 4,
        Other = 5
    }
}
