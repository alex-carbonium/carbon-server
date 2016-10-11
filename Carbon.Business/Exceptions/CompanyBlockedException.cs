using System;

namespace Carbon.Business.Exceptions
{
    public class CompanyBlockedException : Exception
    {
        public CompanyBlockedException()
            : base("You tried to access the project which belongs to an account which is locked.")
        {            
        }
    }
}