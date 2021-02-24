using System;
using MiniDashboard.Data;

namespace MiniDashboard.Workers
{
    public record TimeChartData : IChartData
    {
        public IConvertible Label { get; init; }
        public float Value { get; init; }

        public TimeChartData(IConvertible label, float value)
        {
            Label = label;
            Value = value;
        }
    }
}