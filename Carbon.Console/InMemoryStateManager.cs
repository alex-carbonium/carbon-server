using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;

namespace Carbon.Console
{
    public class InMemoryStateManager : IActorStateManager
    {
        public Dictionary<string, object> Dictionary = new Dictionary<string, object>();

        public Task AddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = new CancellationToken())
        {
            Dictionary.Add(stateName, value);
            return Task.FromResult(0);
        }

        public Task<T> GetStateAsync<T>(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            var state = Dictionary[stateName];
            return Task.FromResult((T)state);
        }

        public Task SetStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = new CancellationToken())
        {
            Dictionary[stateName] = value;
            return Task.FromResult(0);
        }

        public Task RemoveStateAsync(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            Dictionary.Remove(stateName);
            return Task.FromResult(0);
        }

        public Task<bool> TryAddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!Dictionary.ContainsKey(stateName))
            {
                Dictionary.Add(stateName, value);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<ConditionalValue<T>> TryGetStateAsync<T>(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            object value;
            var exists = Dictionary.TryGetValue(stateName, out value);
            return Task.FromResult(new ConditionalValue<T>(exists, (T) value));
        }

        public Task<bool> TryRemoveStateAsync(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            if (Dictionary.ContainsKey(stateName))
            {
                Dictionary.Remove(stateName);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> ContainsStateAsync(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(Dictionary.ContainsKey(stateName));
        }

        public Task<T> GetOrAddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = new CancellationToken())
        {
            object existing;
            if (Dictionary.TryGetValue(stateName, out existing))
            {
                return Task.FromResult((T)existing);
            }
            Dictionary.Add(stateName, value);
            return Task.FromResult(value);
        }

        public Task<T> AddOrUpdateStateAsync<T>(string stateName, T addValue, Func<string, T, T> updateValueFactory,
            CancellationToken cancellationToken = new CancellationToken())
        {
            object existing;
            if (Dictionary.TryGetValue(stateName, out existing))
            {
                var newValue = updateValueFactory(stateName, (T)existing);
                Dictionary[stateName] = newValue;
                return Task.FromResult(newValue);
            }
            Dictionary.Add(stateName, addValue);
            return Task.FromResult(addValue);
        }

        public Task<IEnumerable<string>> GetStateNamesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(Dictionary.Keys.AsEnumerable());
        }

        public Task ClearCacheAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            Dictionary.Clear();
            return Task.FromResult(0);
        }

        public Task SaveStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(true);
        }
    }
}
