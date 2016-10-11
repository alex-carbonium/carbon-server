using System;
using Carbon.Business.Domain;

namespace Carbon.Business.Exceptions
{
    public class InsufficientPermissionsException : Exception
    {
        public InsufficientPermissionsException(Permission requested)
            : base($"Requested {requested}")
        {            
        }
        public InsufficientPermissionsException(Permission requested, Permission granted)
            : base($"Requested {requested}, granted {granted}")
        {            
        }        
    }
}