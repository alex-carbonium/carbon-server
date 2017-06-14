using System;

namespace Carbon.Business.Exceptions
{
    public class BadUrlException : Exception
    {
        public BadUrlException(string url) : base(url)
        {
        }
    }
}
