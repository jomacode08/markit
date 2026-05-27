using System.Security.Cryptography;
using System.Text;

namespace markit.Application.Common.Helpers
{
    public class EmailGenerator
    {
        private static readonly char[] chars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        public static string GenerateDummyEmail(int usernameLength = 10, string domain = "@markit.com")
        {
            var data = new byte[usernameLength];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }

            StringBuilder result = new(usernameLength);
            foreach (byte b in data)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.Append(domain).ToString();
        }
    }
}
