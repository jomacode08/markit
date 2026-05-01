namespace markit.Application.Models.Authentication.Google
{
    public class GoogleAuthSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
        }
    }
}
