using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MiniDashboard.Data;

namespace MiniDashboard.Counters
{
    /*
    Implementation of circular buffer counter for counting events per time.
    It is thead safe and has high increment/add performance and good enough avg/count performance. 
    Usage is as simple as:
        var counter = new CircularBufferCounter(TimeSpan.FromSeconds(1), TimeSpan.FromHours(2));
        counter.Increment();
        var avgPerMinuteLastHour = counter.Avg(TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));
    */

    /// <summary>
    /// ~900KB for 24h capacity and 1s granularity
    /// </summary>
    public class CircularBufferCounter
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SecondCount
        {
            public SecondCount(long ticks, int count)
            {
                Ticks = ticks;
                Count = count;
            }

            public int Count;
            public readonly long Ticks; //possibility to make this int32 and compress ticks

            public override string ToString()
            {
                return new DateTime(Ticks).ToString(CultureInfo.InvariantCulture) + ": " + Count;
            }
        }

        private readonly object _syncRoot = new object();
        private readonly SecondCount[] _buffer;
        private int _head;
        private int _tail;
        private int _elementCount;
        private int _total;

        public DateTime StartTime { get; private set; }
        public string Name { get; set; }
        public TimeSpan Capacity { get; private set; }
        public TimeSpan Granularity { get; private set; }
        public long CountPerCapacity => Count(Capacity);
        public long CountPerLastMin => Count(TimeSpan.FromMinutes(1));

        public long Total => _total;

        public CircularBufferCounter(TimeSpan granularity, TimeSpan capacity)
        {
            if (capacity.TotalSeconds < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be more than a second");
            if (granularity.TotalMilliseconds < 100)
                throw new ArgumentOutOfRangeException(nameof(granularity), "Granularity must be more than a 100ms");
            if (granularity >= capacity)
                throw new ArgumentOutOfRangeException(nameof(granularity), "Granularity must be less then Capacity");

            Granularity = granularity;
            _total = 0;
            Capacity = capacity;
            StartTime = DateTime.UtcNow;
            var count = (int) Math.DivRem(capacity.Ticks, granularity.Ticks, out var remainder);
            if (remainder > 0)
                count++;
            _buffer = new SecondCount[count];
            _head = count - 1;
        }


        public void Increment(int count = 1)
        {
            Interlocked.Add(ref _total, count);
            var ticks = DateTime.UtcNow.Ticks;
            if (_buffer[_head].Ticks / Granularity.Ticks == ticks / Granularity.Ticks)
            {
                _buffer[_head].Count += count;
            }
            else
            {
                lock (_syncRoot)
                {
                    if (_buffer[_head].Ticks / Granularity.Ticks != ticks / Granularity.Ticks) //double check if head must advance
                    {
                        _head = (_head + 1) % _buffer.Length;
                        _buffer[_head] = new SecondCount(ticks, count);
                        if (_elementCount == _buffer.Length)
                            _tail = (_tail + 1) % _buffer.Length;
                        else
                            ++_elementCount;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates average increments per time in given interval
        /// </summary>
        public double Avg(TimeSpan perTime, TimeSpan interval)
        {
            if (perTime >= interval)
                throw new ArgumentException("perTime must be lass than lastInterval", "perTime");
            var cnt = Count(interval);
            var avg = cnt / ((double) interval.Ticks / perTime.Ticks);
            return avg;
        }

        /// <summary>
        /// Calculates average increments per time in using all capacity (non locked)
        /// </summary>
        public double Avg(TimeSpan interval)
        {
            var cnt = Count();
            var avg = cnt / ((double) interval.Ticks / Capacity.Ticks);
            return avg;
        }

        public long Count(DateTime from, TimeSpan span)
        {
            var to = from.Ticks + span.Ticks;
            if (to <= 0)
                to = TimeSpan.MaxValue.Ticks;
            var sum = 0;

            lock (_syncRoot)
            {
                for (long i = 0; i < _elementCount; i++)
                {
                    var sCount = this[i];
                    if (sCount.Ticks >= from.Ticks && sCount.Ticks < to)
                    {
                        sum += sCount.Count;
                    }
                    else if (sCount.Ticks > to)
                    {
                        break; //quit after we reach max value
                    }
                }

                return sum;
            }
        }

        /// <summary>
        /// Counts all events in buffer (non locked)
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            var sum = 0;
            for (long i = 0; i < _buffer.Length; i++)
            {
                sum += _buffer[i].Count;
            }

            return sum;
        }

        public long Count(TimeSpan interval) => Count(DateTime.UtcNow.Subtract(interval), TimeSpan.MaxValue);

        public long Count(DateTime from, DateTime to) => Count(from, to - from);

        private Dictionary<string, decimal> GetSimpleStats()
        {
            var stats = new Dictionary<string, decimal>();
            var now = DateTime.UtcNow;
            var intervals = new Dictionary<TimeSpan, int>
            {
                {now.TimeOfDay, 0},
                {TimeSpan.FromHours(24), 0},
                {TimeSpan.FromHours(12), 0},
                {TimeSpan.FromHours(6), 0},
                {TimeSpan.FromHours(3), 0},
                {TimeSpan.FromHours(1), 0},
                {TimeSpan.FromMinutes(30), 0},
                {TimeSpan.FromMinutes(10), 0},
                {TimeSpan.FromMinutes(5), 0},
                {TimeSpan.FromMinutes(1), 0},
                {TimeSpan.FromSeconds(30), 0},
                {TimeSpan.FromSeconds(10), 0},
                {TimeSpan.FromSeconds(5), 0},
                {TimeSpan.FromSeconds(1), 0},
            };
            lock (_syncRoot)
            {
                for (long i = 0; i < _elementCount; i++)
                {
                    var c = this[i];
                    foreach (var interval in intervals.Keys.ToArray())
                    {
                        if (c.Ticks >= now.Subtract(interval).Ticks)
                            intervals[interval] = intervals[interval] + c.Count;
                    }
                }
            }

            foreach (var (key, value) in intervals)
            {
                if (key == now.TimeOfDay)
                    stats["SinceDayStart"] = value;
                else if ((int) key.TotalDays == 1)
                    stats["Last24h"] = value;
                else if (key.Hours > 0)
                    stats["Last" + key.Hours + "h"] = value;
                else if (key.Minutes > 0)
                    stats["Last" + key.Minutes + "m"] = value;
                else if (key.Seconds > 0)
                    stats["Last" + key.Seconds + "s"] = value;
            }

            stats["AvgSecPerLast1h"] = Math.Round(intervals[TimeSpan.FromHours(1)] / 3600m, 2);
            stats["AvgSecPerLast3h"] = Math.Round(intervals[TimeSpan.FromHours(3)] / (3600m * 3), 2);
            stats["AvgSecPerLast1m"] = Math.Round(intervals[TimeSpan.FromMinutes(1)] / 60m, 2);
            stats["AvgSecPerLast24h"] = Math.Round(intervals[TimeSpan.FromHours(24)] / (3600m * 24), 2);
            stats["AvgMinPerLast24h"] = Math.Round(intervals[TimeSpan.FromHours(24)] / (24m * 60), 2);
            return stats;
        }


        public void Clear()
        {
            lock (_syncRoot)
            {
                _buffer[0] = new SecondCount();
                _head = _buffer.Length - 1;
                _tail = 0;
                _elementCount = 0;
            }
        }

        public object GetSummary()
        {
            return new
            {
                Name = Name,
                StartTime = StartTime,
                Total = Total,
                Stats = GetSimpleStats()
            };
        }

        public ITableRowData GetRow()
        {
            var stats = GetSimpleStats();
            return new CountersSummary(
                Name,
                Total.ToString(),
                stats["Last1m"].ToString(CultureInfo.InvariantCulture),
                stats["Last1h"].ToString(CultureInfo.InvariantCulture),
                stats["Last6h"].ToString(CultureInfo.InvariantCulture),
                stats["Last12h"].ToString(CultureInfo.InvariantCulture),
                stats["Last24h"].ToString(CultureInfo.InvariantCulture)
            );
        }

        public SortedList<DateTime, int> GetStats() => GetStats(Granularity);

        public SortedList<DateTime, int> GetStats(TimeSpan granularity)
        {
            lock (_syncRoot)
            {
                var stats = new SortedList<DateTime, int>();
                var now = DateTime.UtcNow.Ticks;
                var sum = 0;
                long ticks = 0, prevTicks = 0;
                for (long i = 0; i < _elementCount; i++)
                {
                    ticks = this[i].Ticks;
                    if (prevTicks == 0)
                        prevTicks = ticks;
                    if (ticks >= now - Capacity.Ticks)
                    {
                        sum += this[i].Count;
                        if (ticks - prevTicks >= granularity.Ticks)
                        {
                            stats.Add(new DateTime(ticks), sum);
                            prevTicks = ticks;
                            sum = 0;
                        }
                    }
                }

                if (sum > 0)
                    stats.Add(new DateTime(ticks), sum);
                return stats;
            }
        }

        private SecondCount this[long index]
        {
            get
            {
                if (index < 0 || index >= _elementCount)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _buffer[(_tail + index) % _buffer.Length];
            }
            set
            {
                if (index < 0 || index >= _elementCount)
                    throw new ArgumentOutOfRangeException(nameof(index));

                _buffer[(_tail + index) % _buffer.Length] = value;
            }
        }
    }
}