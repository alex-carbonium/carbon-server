using System;

namespace Carbon.Business.Exceptions
{
    public class UpdateConflictException : Exception
    {
        public UpdateConflictException(string message)
            : base(message)
        {
        }

        public UpdateConflictException(Exception exception)
            : base("The record was updated by another transaction", exception)
        {            
        }
    }
}