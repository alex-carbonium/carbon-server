using System;

namespace Carbon.Framework.Util
{
    public class TypeUtil
    {
        public static bool IsNullable(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}