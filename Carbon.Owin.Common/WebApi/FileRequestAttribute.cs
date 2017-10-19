using System;

namespace Carbon.Owin.Common.WebApi
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FileRequestAttribute : Attribute
    {
        public FileRequestAttribute(string mimeType)
        {
            MimeType = mimeType;
        }

        public string MimeType { get; }
    }
}
