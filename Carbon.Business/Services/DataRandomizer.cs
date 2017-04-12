using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;

namespace Carbon.Business.Services
{
    public class DataRandomizer : Randomizer
    {
        private static readonly object _syncRoot = new object();
        private readonly Random _seed;

        public DataRandomizer(Random seed)
        {
            _seed = seed;
        }

        new public int Number(int min = 0, int max = 1)
        {
            lock (_syncRoot)
            {
                max = max == int.MaxValue ? max : max + 1;
                return _seed.Next(min, max);
            }
        }

        new public double Double(double min = 0.0d, double max = 1.0d)
        {
            lock (_syncRoot)
            {
                if (min == 0.0d && max == 1.0d)
                {
                    //use default implementation
                    return _seed.NextDouble();
                }

                return _seed.NextDouble() * (max - min) + min;
            }
        }

        new public byte[] Bytes(int count)
        {
            var arr = new byte[count];
            lock (_syncRoot)
            {
                _seed.NextBytes(arr);
            }
            return arr;
        }

        new public IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
        {
            List<T> buffer = source.ToList();
            for (var i = 0; i < buffer.Count; i++)
            {
                int j;
                //lock any seed access, for thread safety.
                lock (_syncRoot)
                {
                    j = _seed.Next(i, buffer.Count);
                }
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}
