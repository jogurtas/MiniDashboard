using System;
using System.Threading;

namespace MiniDashboard.Counters
{
    public static class GlobalStats
    {
        public static DateTime Started { get; private set; } = DateTime.UtcNow;
        private static int _connectionsCounter;
        public static int ConnectionsCounter => _connectionsCounter;

        public static void IncrementConnections() => Interlocked.Increment(ref _connectionsCounter);
        public static void DecrementConnections() => Interlocked.Decrement(ref _connectionsCounter);

        public static CountersCollection Counters { get; } = new (TimeSpan.FromSeconds(10), TimeSpan.FromHours(48), "General");
    }
}
