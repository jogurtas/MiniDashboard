using System;
using System.Collections.Generic;
using MiniDashboard.Data;
using MiniDashboard.Utils;

namespace MiniDashboard.Cfg
{
    public class MiniDashboardConfiguration
    {
        private string _route = "/dash/data";

        public string Route
        {
            get => _route;
            set
            {
                ThrowIf.IsNull(value, "route");
                _route = $"/{value.Trim('/')}";
            }
        }

        public string BasicAuthUserName { get; set; }

        public string BasicAuthPassword { get; set; }

        private readonly List<Card> _cards = new();
        public IList<Card> Cards => _cards.AsReadOnly();

        private readonly List<Chart> _charts = new();
        public IList<Chart> Charts => _charts.AsReadOnly();

        private readonly List<Table> _tables = new();
        public IList<Table> Tables => _tables.AsReadOnly();

        public MiniDashboardConfiguration EnrichWithStaticCard(string title, string value, string data = null)
        {
            ThrowIf.IsNullOrWhiteSpace(title);
            ThrowIf.IsNull(value, nameof(value));

            var type = Uri.IsWellFormedUriString(data, UriKind.Absolute) ? CardType.Link : CardType.Static;

            _cards.Add(new Card(title, value, type, () => data));
            return this;
        }

        public MiniDashboardConfiguration EnrichWithDynamicCard(string title, string value, Func<string> onUpdate)
        {
            ThrowIf.IsNullOrWhiteSpace(title);
            ThrowIf.IsNull(value, nameof(value));
            ThrowIf.IsNull(onUpdate, nameof(onUpdate));

            _cards.Add(new Card(title, value, CardType.Dynamic, onUpdate));
            return this;
        }

        public MiniDashboardConfiguration EnrichWithChart(string title, ChartType type, Func<ICollection<IChartData>> onUpdate)
        {
            ThrowIf.IsNullOrWhiteSpace(title);
            ThrowIf.IsNull(onUpdate, nameof(onUpdate));

            _charts.Add(new Chart(title, type, onUpdate));
            return this;
        }

        public MiniDashboardConfiguration EnrichWithTable(string title, Func<ICollection<ITableRowData>> onUpdate)
        {
            ThrowIf.IsNullOrWhiteSpace(title);
            ThrowIf.IsNull(onUpdate, nameof(onUpdate));

            _tables.Add(new Table(title, onUpdate));
            return this;
        }
    }
}