namespace markit.Application.Models.Settings.RateLimiting
{
    public class VolumeControl
    {
        public const string SectionName = "VolumeControl";
        public int PermitLimit { get; set; }
        public int WindowMinutes { get; set; }
        public int WindowSegments { get; set; }
    }
}
