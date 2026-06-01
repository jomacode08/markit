using Ganss.Xss;
using Markdig;
using static markit.Application.Helpers.GeneralConstant;


namespace markit.Application.Common.Helpers
{
    public static class Utilities
    {
        private static readonly HtmlSanitizer _htmlSanitizer = new();
        private static readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public static string? GetSystemConfigDescription(string key)
        {
            return key switch
            {
                SystemConfigKeys.IS_DEMO_ENABLED_KEY => "Configuration that toggles the demo features of the app.",
                SystemConfigKeys.DEMO_SESSION_DURATION_IN_MINUTES => "The validity period (in minutes) for the demo session.",
                SystemConfigKeys.DEMO_USER_TEMPLATE_ID => "User ID to use as a template for creating demo session directories.",
                SystemConfigKeys.AUTH_IS_GOOGLE_ENABLED_KEY => "Configuration that enables or disables Google authentication for the app.",
                SystemConfigKeys.AUTH_IS_GITHUB_ENABLED_KEY => "Configuration that enables or disables GitHub authentication for the app.",
                _ => null
            };
        }

        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            return _htmlSanitizer.Sanitize(html);
        }

        public static string ConvertMarkdownToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return string.Empty;
            return Markdown.ToHtml(markdown, _markdownPipeline);
        }
    }
}
