using System.Collections.Generic;
using System.Linq;

namespace Carbon.Framework.Validation
{
    public interface IValidator
    {
        void AddError(string key, string message, params object[] parameters);
        void AddError(string message);
        Dictionary<string, string> Errors { get; }
    }

    public static class Extensions
    {
        public static string GetAllErrors(this IValidator validator)
        {
            if (validator.Errors == null)
            {
                return null;
            }            
            return string.Join(", ", validator.Errors.Select(x => x.Key + ":" + x.Value));
        }
    }
}