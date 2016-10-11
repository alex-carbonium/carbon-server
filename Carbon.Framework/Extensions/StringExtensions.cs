using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;

namespace Carbon.Framework.Extensions
{
    public static class StringExtensions
    {
        public static bool IsBase64String(this string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        public static string ToCamelCase(this string s)
        {
            var i = 0;
            while (char.IsUpper(s[i++]));
            return s.Substring(0, i).ToLower() + s.Substring(i);
        }

        public static string ToPlainText(this SecureString secureString)
        {
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
        public static string CleanUrlPath(this string url)
        {
            if (string.IsNullOrWhiteSpace(url)) url = "/";

            if (url != "/" && url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        public static string Unquote(this string s)
        {
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                return s.Substring(1, s.Length - 2);
            }
            if (s.StartsWith("\""))
            {
                return s.Substring(1, s.Length - 1);
            }
            if (s.EndsWith("\""))
            {
                return s.Substring(0, s.Length - 1);
            }
            return s;
        }
    }
}