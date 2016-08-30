using System;
using System.Text;

namespace VIDEOwnloader.Common
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
                        sb.Append(" " + Decapitalize(innerException.Message));
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

        private static string Decapitalize(string value)
        {
            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }
    }
}