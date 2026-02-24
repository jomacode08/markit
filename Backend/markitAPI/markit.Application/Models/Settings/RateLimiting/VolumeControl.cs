namespace markit.Application.Models.Settings.RateLimiting
{
    public class VolumeControl
    {
        public string SectionName = "VolumeControl";
        public int PermitLimit { get; set; }
        public int WindowMinutes { get; set; }
        public int WindowSegments { get; set; }
    }
}
