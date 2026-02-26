namespace markit.Application.Features.Settings.Queries.ViewModels
{
    public class DemoSettings
    {
        public bool IsEnabled { get; set; } = false;
        public string? UserId { get; set; }
        public int? TokenDurationInMinutes { get; set; }
    }
}
