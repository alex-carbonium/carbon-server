using System;

namespace Carbon.Business.Exceptions
{
    public class ProjectNotSharedException : Exception
    {
        public ProjectNotSharedException()
            : base("The project is not currently shared")
        {            
        }
    }
}
