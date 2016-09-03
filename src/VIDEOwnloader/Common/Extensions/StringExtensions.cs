#region License

// VIDEOwnloader
// Copyright (C) 2016 Adam Rutkowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

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