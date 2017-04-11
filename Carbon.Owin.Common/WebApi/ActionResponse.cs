using System.Collections.Generic;

namespace Carbon.Owin.Common.WebApi
{
    public class ActionResponse
    {
        public ActionResponse(bool ok, IDictionary<string, string> errors = null)
        {
            Ok = ok;
            Errors = errors;
        }

        public bool Ok { get; }
        public IDictionary<string, string> Errors { get; }

        public static readonly ActionResponse Success = new ActionResponse(true);
    }
}