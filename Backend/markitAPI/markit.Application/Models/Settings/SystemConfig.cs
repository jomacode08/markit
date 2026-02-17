using System.ComponentModel.DataAnnotations;

namespace markit.Application.Models.Settings
{
    public class SystemConfig
    {
        [Key] public required string Id { get; set; }
        [Required] public required string Value { get; set; }
        public string? Description { get; set; }
    }
}
