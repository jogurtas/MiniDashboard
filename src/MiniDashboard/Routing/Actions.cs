using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using MiniDashboard.Cfg;
using MiniDashboard.Counters;
using MiniDashboard.Data;
using MiniDashboard.Workers;

namespace MiniDashboard.Routing
{
    public class Actions
    {
        private readonly IEnumerable<Card> _cards;
        private readonly IEnumerable<Chart> _charts;
        private readonly IEnumerable<Table> _tables;

        public Actions(MiniDashboardConfiguration cfg)
        {
            ResourcesUsageCollector.Start();

            _cards = new[]
            {
                new Card("Start time", Process.GetCurrentProcess().StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CardType.Static, null),
            }.Concat(cfg.Cards);

            _charts = new[]
            {
                new Chart("Cpu", ChartType.Time, ResourcesUsageCollector.GetCpuUsage),
                new Chart("Memory", ChartType.Time, ResourcesUsageCollector.GetMemoryUsage),
            }.Concat(cfg.Charts);

            _tables = new[]
            {
                new Table("Counters", () => GlobalCountersCollection.Instance.GetAllCounters().Select(x => x.GetRow()).ToList()),
            }.Concat(cfg.Tables);
        }

        public ResponseModel Get()
        {
            return new()
            {
                Cards = _cards.Select(x => x.Update()),
                Charts = _charts.Select(x => x.Update()),
                Tables = _tables.Select(x => x.Update()),
            };
        }

        public IEnumerable<Card> GetCards()
        {
            return _cards.Select(x => x.Update());
        }

        public object GetCardById(string id)
        {
            var card = _cards.FirstOrDefault(x => x.Title.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            return card == null ? Error.NotFound() : card.Update();
        }

        public IEnumerable<Chart> GetCharts()
        {
            return _charts.Select(x => x.Update());
        }

        public object GetChartById(string id)
        {
            var chart = _charts.FirstOrDefault(x => x.Title.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            return chart == null ? Error.NotFound() : chart.Update();
        }

        public IEnumerable<Table> GetTables()
        {
            return _tables.Select(x => x.Update());
        }

        public object GetTableById(string id)
        {
            var table = _tables.FirstOrDefault(x => x.Title.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            return table == null ? Error.NotFound() : table.Update();
        }

        public object GetTableRowById(string id, string elemId)
        {
            var table = _tables.FirstOrDefault(x => x.Title.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            var row = table?.Rows.FirstOrDefault(x =>
                x.RowId.ToString(CultureInfo.InvariantCulture).Equals(elemId, StringComparison.InvariantCultureIgnoreCase));
            return row == null ? Error.NotFound() : row;
        }
    }
}