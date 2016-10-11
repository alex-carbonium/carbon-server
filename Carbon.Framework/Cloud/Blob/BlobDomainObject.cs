using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Carbon.Framework.Models;

namespace Carbon.Framework.Cloud.Blob
{
    public class BlobDomainObject : IDomainObject<string>
    {
        private Dictionary<string, string> _metadata; 

        public string Id { get; set; }
        public Uri Uri { get; set; }
        public byte[] Content { get; set; }
        public Stream ContentStream { get; set; }
        public string ContentType { get; set; }
        public bool LazyFetched { get; set; }
        public bool CacheForever { get; set; }

        public Dictionary<string, string> Metadata
        {
            get { return _metadata ?? (_metadata = new Dictionary<string, string>()); }
        }

        public bool HasMetadata()
        {
            return _metadata != null;
        }

        public void SetContent(string content)
        {
            if (ContentStream != null)
            {
                ContentStream.Dispose();
                ContentStream = null;
            }
            Content = Encoding.UTF8.GetBytes(content);
        }
        public void SetContent(byte[] content)
        {
            if (ContentStream != null)
            {
                ContentStream.Dispose();
                ContentStream = null;
            }
            Content = content;
        }

        public async Task<string> GetContentAsString()
        {
            if (ContentStream != null)
            {
                using (var reader = new StreamReader(ContentStream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            return Encoding.UTF8.GetString(Content);
        }

        public string AutoDetectContentType()
        {
            var ext = Path.GetExtension(Id);
            switch (ext.ToLower())
            {
                case ".html": ContentType = "text/html"; break;
                case ".png": ContentType = "image/png"; break;
                case ".jpg": ContentType = "image/jpeg"; break;
                case ".jpeg": ContentType = "image/jpeg"; break;
                case ".gif": ContentType = "image/gif"; break;
                case ".css": ContentType = "text/css"; break;
                case ".js": ContentType = "text/javascript"; break;
                case ".woff": ContentType = "application/x-font-woff"; break;
                case ".eot": ContentType = "application/vnd.ms-fontobject"; break;
                case ".svg": ContentType = "image/svg+xml"; break;
                case ".otf": ContentType = "application/x-font-opentype"; break;
                case ".ttf": ContentType = "application/x-font-ttf"; break;
            }
            return ContentType;
        }
    }
}