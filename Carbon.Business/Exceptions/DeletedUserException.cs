using System;

namespace Carbon.Business.Exceptions
{
    public class DeletedUserException : Exception
    {
        public DeletedUserException(string userId)
            : base(string.Format("User {0} is deleted", userId))
        {
            
        }
    }
}
