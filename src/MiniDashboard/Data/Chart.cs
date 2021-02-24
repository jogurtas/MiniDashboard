using System;
using System.Collections.Generic;

namespace MiniDashboard.Data
{
    public enum ChartType
    {
        Line,
        Area,
        Bar,
        Pie,
        Doughnut,
        Time
    }
    
    public interface IChartData
    {
        IConvertible Label { get; init; }
    }

    public class Chart
    {
        public string Title { get; init; }
        public ChartType Type { get; set; }
        public ICollection<IChartData> Data { get; private set; }

        private Func<ICollection<IChartData>> OnUpdate { get; }

        public Chart(string title, ChartType type, Func<ICollection<IChartData>> onUpdate)
        {
            Title = title;
            Type = type;
            OnUpdate = onUpdate;
        }

        public Chart Update()
        {
            Data = OnUpdate?.Invoke();
            return this;
        }
    }
}