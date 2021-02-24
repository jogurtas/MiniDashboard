using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MiniDashboard.Data;
using MiniDashboard.Utils;

namespace MiniDashboard.Workers
{
    public static class ResourcesUsageCollector
    {
        private static FixedSizedQueue<IChartData> Cpu { get; } = new(300);
        private static FixedSizedQueue<IChartData> Memory { get; } = new(300);
        private static Timer _timer;

        public static void Start()
        {
            _timer = new Timer(GetCurrentResourcesUsage, null, 1000, 1000 * 5);
        }

        public static void Stop() => _timer.Dispose();

        private static async void GetCurrentResourcesUsage(object state)
        {
            var timestamp = DateTime.UtcNow;
            Cpu.Enqueue(new TimeChartData(timestamp, await CalcCpuUsage()));
            Memory.Enqueue(new TimeChartData(timestamp, CalcMemoryUsage()));
        }

        private static float CalcMemoryUsage()
        {
            return Process.GetCurrentProcess().WorkingSet64 / 1024f / 1024f;
        }

        private static async Task<float> CalcCpuUsage()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return (float) cpuUsageTotal * 100;
        }

        public static ICollection<IChartData> GetMemoryUsage() => Memory.ToList();

        public static ICollection<IChartData> GetCpuUsage() => Cpu.ToList();
    }
}