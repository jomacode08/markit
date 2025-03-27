using markit.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace markit.Domain.Entities
{
    public class Creator : BaseModel
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public Gender? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        public virtual ICollection<Collection>? Collections { get; set; }
    }

    public enum Gender
    {
        Female = 1,
        Male = 2,
        Other = 3
    }
}
