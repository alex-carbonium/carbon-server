using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Framework.Extensions
{
    public static class UriExtensions
    {
        //uri constructor cannot combine two parts of the path
        public static Uri AddPath(this Uri uri, string path)
        {
            var url = uri.ToString();
            if (!url.EndsWith("/"))
            {
                url += "/";
            }
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }
            return new Uri(url + path, UriKind.RelativeOrAbsolute);
        }

        public static IDictionary<string, string> GetQuery(this Uri uri)
        {
            var res = new Dictionary<string, string>();
            var q = uri.Query;
            if (string.IsNullOrEmpty(q))
            {
                return res;
            }
            if (q.StartsWith("?"))
            {
                q = q.Substring(1);
            }
            foreach (var s in q.Split(new []{'&'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = s.Split('=');
                if (kv.Length == 2)
                {
                    res[Uri.UnescapeDataString(kv[0])] = Uri.UnescapeDataString(kv[1]);
                }
            }
            return res;
        }

        public static Uri AppendQuery(this Uri uri, IDictionary<string, string> query)
        {
            var q = new StringBuilder();
            var url = uri.ToString();
            var addSeparator = false;
            var questionIndex = url.IndexOf("?");
            if (questionIndex == -1)
            {
                q.Append("?");
            }
            else
            {
                addSeparator = questionIndex != url.Length - 1;
            }
            foreach (var kv in query)
            {
                if (addSeparator)
                {
                    q.Append("&");
                }
                q.AppendFormat("{0}={1}", Uri.EscapeUriString(kv.Key), Uri.EscapeUriString(kv.Value));
                addSeparator = true;
            }
            return new Uri(url + q, UriKind.RelativeOrAbsolute);
        }

        public static Uri TrimQuery(this Uri uri, Dictionary<string, string> filter = null)
        {
            var s = uri.ToString();
            var q = s.IndexOf("?");
            if (q != -1)
            {
                var baseUrl = new Uri(s.Substring(0, q), UriKind.RelativeOrAbsolute);
                if (filter == null || filter.Count == 0)
                {
                    return baseUrl;
                }
                var query = uri.GetQuery();
                foreach (var key in filter.Keys.Where(key => query.ContainsKey(key)))
                {
                    query[key] = filter[key];
                }
                return baseUrl.AppendQuery(query);
            }
            return uri;
        }

        public static Uri ToHttps(this Uri uri)
        {
            var partial = uri.GetComponents(UriComponents.Host | UriComponents.PathAndQuery, UriFormat.Unescaped);
            return new Uri("https://" + partial);
        }
    }
}