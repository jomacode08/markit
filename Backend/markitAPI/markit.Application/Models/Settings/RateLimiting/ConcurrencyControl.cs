namespace markit.Application.Models.Settings.RateLimiting
{
    public class ConcurrencyControl
    {
        public string SectionName = "Concurrency";
        public int PermitLimit { get; set; }
        public int QueueLimit { get; set; }
    }
}
