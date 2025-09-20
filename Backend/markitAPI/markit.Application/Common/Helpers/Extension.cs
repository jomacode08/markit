using markit.Application.Models.Authentication.Enums;

namespace markit.Application.Common.Helpers
{
    public static class Extension
    {
        public static string GetName(this LoginProvider loginProvider)
        {
            return Enum.GetName(typeof(LoginProvider), loginProvider)
                ?? throw new InvalidOperationException();
        }
    }
}
