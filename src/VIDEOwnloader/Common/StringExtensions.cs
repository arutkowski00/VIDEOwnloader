using System.Text.RegularExpressions;

namespace VIDEOwnloader.Common
{
    public static class StringExtensions
    {
        public static readonly Regex UrlRegex = new Regex(@"^(https?|ftp):\/\/[^\s/$.?#].[^\s]*$", RegexOptions.Compiled);

        public static bool IsValidUrl(this string value)
        {
            return UrlRegex.IsMatch(value);
        }
    }
}