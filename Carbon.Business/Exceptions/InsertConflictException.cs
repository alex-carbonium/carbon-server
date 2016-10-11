using System;

namespace Carbon.Business.Exceptions
{
    public class InsertConflictException : Exception
    {
        public InsertConflictException(string message)
            : base(message)
        {
        }

        public InsertConflictException(Exception exception)
            : base("The record already exists", exception)
        {
        }
    }
}