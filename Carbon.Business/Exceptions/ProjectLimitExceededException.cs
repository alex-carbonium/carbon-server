using System;
using Carbon.Business.Domain;

namespace Carbon.Business.Exceptions
{
    public class ProjectLimitExceededException : Exception
    {
        public ProjectLimitExceededException(int limit) : 
            base(string.Format("You cannot create more than {0} projects having Personal account", limit))
        {            
        }     
    }
}