using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MiniDashboard.Counters
{
    public class CountersCollection : IReadOnlyDictionary<string, CircularBufferCounter>, IDisposable
    {
        private readonly TimeSpan _granularity;
        private readonly TimeSpan _capacity;

        private readonly Dictionary<string, CircularBufferCounter> _counters;

        public string Name { get; set; }

        public CountersCollection(TimeSpan granularity, TimeSpan capacity, string name)
        {
            Name = name;
            _granularity = granularity;
            _capacity = capacity;
            _counters = new Dictionary<string, CircularBufferCounter>();
        }

        public void IncrementCounter(string key, int count = 1)
        {
            if (_counters.TryGetValue(key, out var counter))
            {
                counter.Increment(count);
            }
            else
            {
                counter = GlobalCountersCollection.Instance.Create(_granularity, _capacity, $"C-{Name}-{key}");
                counter.Increment(count);
                _counters[key] = counter;
            }
        }

        public IDictionary<string, object> GetAllCountersForDisplay()
        {
            return _counters.Where(c => c.Value != null).ToDictionary(c => c.Key, c => c.Value.GetSummary());
        }

        public bool ContainsKey(string key) => _counters.ContainsKey(key);

        public bool TryGetValue(string key, out CircularBufferCounter value) => _counters.TryGetValue(key, out value);

        public CircularBufferCounter this[string key] => _counters[key];

        public IEnumerable<string> Keys => _counters.Keys;

        public IEnumerable<CircularBufferCounter> Values => _counters.Values;

        public IEnumerator<KeyValuePair<string, CircularBufferCounter>> GetEnumerator() => _counters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _counters.Count;

        public void Dispose() => GlobalCountersCollection.Instance.Remove(_counters.Values);
    }
}