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
using System.Text;

namespace VIDEOwnloader.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception exc)
        {
            var sb = new StringBuilder(exc.Message);
            var innerException = exc.InnerException;
            var previousMessage = string.Empty;
            var isColonUsed = false;
            while (innerException != null)
            {
                if (previousMessage != innerException.Message)
                    if (sb[sb.Length - 1] == ':')
                    {
                        sb.Append(" " + innerException.Message.Decapitalize());
                    }
                    else
                    {
                        sb.Append(" " + innerException.Message);
                        if (!innerException.Message.EndsWith(".") && !isColonUsed)
                        {
                            sb.Append(":");
                            isColonUsed = true;
                        }
                    }
                previousMessage = innerException.Message;
                innerException = innerException.InnerException;
            }
            return sb.ToString();
        }
    }
}