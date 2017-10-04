using System;
using System.Runtime.Serialization;
using Carbon.Business.Domain;

namespace Carbon.Business.Exceptions
{
    [Serializable]
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

        public InsufficientPermissionsException(Permission requested, Permission granted, string userId, string companyId, string resourceId)
            : base($"Requested {requested}, granted {granted}, user {userId}, company {companyId}, resource {resourceId}")
        {
        }

        // Constructor needed for serialization
        protected InsufficientPermissionsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}