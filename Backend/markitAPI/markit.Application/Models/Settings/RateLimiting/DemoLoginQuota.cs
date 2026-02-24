namespace markit.Application.Models.Settings.RateLimiting
{
    public class DemoLoginQuota
    {
        public string SectionName = "DemoLoginQuota";
        public int PermitLimit { get; set; }
        public int WindowHours { get; set; }
    }
}
