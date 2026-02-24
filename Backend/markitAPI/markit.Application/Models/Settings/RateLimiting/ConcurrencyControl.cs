namespace markit.Application.Models.Settings.RateLimiting
{
    public class ConcurrencyControl
    {
        public const string SectionName = "ConcurrencyControl";
        public int PermitLimit { get; set; }
        public int QueueLimit { get; set; }
    }
}
