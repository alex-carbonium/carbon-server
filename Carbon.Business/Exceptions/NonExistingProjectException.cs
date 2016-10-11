using System;

namespace Carbon.Business.Exceptions
{
    public class NonExistingProjectException : Exception
    {
        public NonExistingProjectException(string message)
            : base(message)
        {            
        }
    }
}