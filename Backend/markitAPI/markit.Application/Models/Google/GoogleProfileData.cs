namespace markit.Application.Models.Google
{
    public class GoogleProfileData
    {
        public string Sub {  get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Given_Name { get; set; } = string.Empty;
        public string Family_Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Email_Verified { get; set; }
    }
}
