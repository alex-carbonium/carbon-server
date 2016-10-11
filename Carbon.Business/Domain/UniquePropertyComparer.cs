using System;
using System.Collections.Generic;

namespace Carbon.Business.Domain
{
    public class UniquePropertyComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, string> _accessor;

        public UniquePropertyComparer(Func<T, string> accessor)
        {
            _accessor = accessor;
        }

        public bool Equals(T x, T y)
        {
            return string.Equals(_accessor(x), _accessor(y), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(T obj)
        {
            return _accessor(obj)?.GetHashCode() ?? 0;
        }
    }
}