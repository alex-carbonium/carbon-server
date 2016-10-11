using System.Text;

namespace Carbon.Framework
{
    public class Defs
    {
        public const string DEBUG = "DEBUG";
        public const string TRACE = "TRACE";

        public const string CULTURE = "en-US";
        public static readonly Encoding Encoding = Encoding.UTF8;

        public class Web
        {
            public const string ERROR_URL_FORMAT = "/error/{0}";
            public const int HTTP_AJAX_ERROR_CODE = 418;        
        }

        public class ClaimTypes
        {
            public const string Subject = "sub";
        }
    }

    public static class DllDeployer
    {
        public static void ReferenceMethod()
        {
            new Castle.Core.Logging.NullLogger();
        }
    }
}