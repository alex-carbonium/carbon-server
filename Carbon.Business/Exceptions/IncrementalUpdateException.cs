using System;

namespace Carbon.Business.Exceptions
{
    public class IncrementalUpdateException : Exception
    {
        public IncrementalUpdateException(string fromVersion, string toVersion)
            : base("Primitives tail could not be applied on top of latest snapshot")
        {
            Data["fromVersion"] = fromVersion;
            Data["toVersion"] = toVersion;
        }
    }
}