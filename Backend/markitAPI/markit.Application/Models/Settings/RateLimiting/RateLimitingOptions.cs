namespace markit.Application.Models.Settings.RateLimiting
{
    public class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";
        public VolumeControl VolumeControl { get; set; } = new VolumeControl();
        public ConcurrencyControl ConcurrencyControl { get; set; } = new ConcurrencyControl();
        public DemoLoginQuota DemoLoginQuota { get; set; } = new DemoLoginQuota();
    }
}
