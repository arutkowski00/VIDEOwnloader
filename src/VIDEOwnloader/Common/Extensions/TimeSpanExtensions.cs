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

using System;
using System.Diagnostics.CodeAnalysis;

namespace VIDEOwnloader.Common.Extensions
{
    public static class TimeSpanExtensions
    {
        [SuppressMessage("ReSharper", "UseStringInterpolation")]
        public static string ToReadableString(this TimeSpan span)
        {
            var formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0
                    ? string.Format("{0:0} day{1} ", span.Days, span.Days == 1 ? string.Empty : "s")
                    : string.Empty,
                span.Duration().Hours > 0
                    ? string.Format("{0:0} hour{1} ", span.Hours, span.Hours == 1 ? string.Empty : "s")
                    : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} min ", span.Minutes) : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} s", span.Seconds) : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 s";

            return formatted;
        }
    }
}