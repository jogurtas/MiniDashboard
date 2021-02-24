using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MiniDashboard.Counters
{
    public class GlobalCountersCollection
    {
        private static readonly object SyncRoot = new ();
        private readonly ConcurrentDictionary<string, CircularBufferCounter> _counters = new ();

        public IReadOnlyCollection<KeyValuePair<string, long>> CountersList
        {
            get { return _counters.Select(c => new KeyValuePair<string, long>(c.Key, c.Value.Total)).ToArray(); }
        }

        private GlobalCountersCollection()
        {
        }

        private static GlobalCountersCollection _instance;

        public static GlobalCountersCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        _instance ??= new GlobalCountersCollection();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Creates counter and adds to global collection
        /// </summary>
        /// <param name="granularity"></param>
        /// <param name="capacity"></param>
        /// <param name="name">Unique name of the counter</param>
        /// <returns></returns>
        public CircularBufferCounter Create(TimeSpan granularity, TimeSpan capacity, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            var counter = _counters.GetOrAdd(name, (key) => new CircularBufferCounter(granularity, capacity) {Name = name});
            return counter;
        }

        public void Remove(CircularBufferCounter counter) => _counters.TryRemove(counter.Name, out _);

        public void Remove(IEnumerable<CircularBufferCounter> counters)
        {
            foreach (var c in counters)
            {
                Remove(c);
            }
        }

        public bool ContainsKey(string key) => _counters.ContainsKey(key);

        public bool TryGetValue(string counterName, out CircularBufferCounter value) => _counters.TryGetValue(counterName, out value);

        public IReadOnlyCollection<CircularBufferCounter> GetAllCounters() => _counters.Select(c => c.Value).ToList();
    }
}