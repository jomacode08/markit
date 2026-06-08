using System.Text.RegularExpressions;

namespace markit.Application.Common.Helpers
{
    public static partial class ValidationRegexes
    {
        public static readonly Regex InternationalNameRegex = InternationalNameRegexGenerator();
        public static readonly Regex EmailRegex = EmailRegexGenerator();


        [GeneratedRegex(@"^[\p{L}\p{N}]+([\s\-'][\p{L}\p{N}]+)*$")]
        private static partial Regex InternationalNameRegexGenerator();

        [GeneratedRegex(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$")]
        private static partial Regex EmailRegexGenerator();
    }
}
