using System.Text.RegularExpressions;

namespace VIDEOwnloader.Common.Extensions
{
    public static class StringExtensions
    {
        public static readonly Regex UrlRegex = new Regex(@"^(https?|ftp):\/\/[^\s/$.?#].[^\s]*$", RegexOptions.Compiled);

        public static string Decapitalize(this string value)
        {
            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }

        public static bool IsValidUrl(this string value)
        {
            return UrlRegex.IsMatch(value);
        }
    }
}