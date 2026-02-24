namespace markit.Application.Models.Settings.RateLimiting
{
    public class DemoLoginQuota
    {
        public const string SectionName = "DemoLoginQuota";
        public int PermitLimit { get; set; }
        public int WindowHours { get; set; }
    }
}
