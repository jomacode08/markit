namespace markit.Application.Models.Authentication.GitHub
{
    public class GitHubAuthSettings
    {
        public string AppName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(AppName) 
                && !string.IsNullOrWhiteSpace(ClientId)
                && !string.IsNullOrWhiteSpace(ClientSecret);
        }
    }
}
