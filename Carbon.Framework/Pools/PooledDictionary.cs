using System;
using System.Collections.Generic;

namespace Carbon.Framework.Pools
{
    public class PooledDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
    {
        private static readonly ObjectPool<PooledDictionary<TKey, TValue>> _poolInstance = CreatePool(32);

        public static PooledDictionary<TKey, TValue> GetInstance()
        {
            return _poolInstance.Allocate();
        }

        public void Free()
        {
            Clear();
            _poolInstance.Free(this);
        }

        public void Dispose()
        {
            Free();
        }

        private static ObjectPool<PooledDictionary<TKey, TValue>> CreatePool(int size)
        {
            return new ObjectPool<PooledDictionary<TKey, TValue>>(() => new PooledDictionary<TKey, TValue>(), size);
        }
    }
}
