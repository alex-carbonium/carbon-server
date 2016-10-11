using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Framework.Util
{
    public static class LinqUtil
    {
        public static void RemoveAll<T>(this IList<T> collection, Func<T, bool> predicate)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (predicate(collection[i]))
                {
                    collection.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
