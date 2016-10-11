using System.Collections.Generic;

namespace Carbon.Framework.Validation
{
    public class DictionaryValidator : IValidator
    {
        private static readonly Dictionary<string, string> EmptyErrors = new Dictionary<string, string>(); 

        private Dictionary<string, string> _errors = EmptyErrors;

        private void EnsureErrors()
        {
            if (_errors == EmptyErrors)
            {
                _errors = new Dictionary<string, string>();
            }
        }

        public void AddError(string key, string message, params object[] parameters)
        {
            EnsureErrors();
            _errors.Add(key, string.Format(message, parameters));
        }

        public void AddError(string message)
        {
            EnsureErrors();
            _errors.Add(string.Empty, message);
        }

        public Dictionary<string, string> Errors
        {
            get { return _errors; }
        }
    }
}
