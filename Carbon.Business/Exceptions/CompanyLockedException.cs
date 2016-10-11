using System;

namespace Carbon.Business.Exceptions
{
    public class CompanyLockedException : Exception
    {
        public CompanyLockedException()
            : base("We are performing maintenance on this account. It usually takes a few minutes. If you have any unsaved changes, don't worry, they will be stored in your browser.")
        {            
        }
    }
}