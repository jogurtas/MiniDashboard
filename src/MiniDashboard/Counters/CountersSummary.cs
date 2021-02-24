using System;
using MiniDashboard.Data;

namespace MiniDashboard.Counters
{
    public record CountersSummary(IConvertible RowId, string Total, string Last1M, string Last1H, string Last6H, string Last12H, string Last24H) : ITableRowData;
}