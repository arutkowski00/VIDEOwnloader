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