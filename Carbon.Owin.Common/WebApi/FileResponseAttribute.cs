using System;

namespace Carbon.Owin.Common.WebApi
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FileResponseAttribute : Attribute
    {
        public FileResponseAttribute(string mimeType)
        {
            MimeType = mimeType;
        }

        public string MimeType { get; }
    }
}
