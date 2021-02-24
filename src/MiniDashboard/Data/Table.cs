using System;
using System.Collections.Generic;

namespace MiniDashboard.Data
{
    public interface ITableRowData
    {
        IConvertible RowId { get; init; }
    }
    
    public class Table
    {
        public string Title { get; }
        public ICollection<ITableRowData> Rows { get; private set; }

        private Func<ICollection<ITableRowData>> OnUpdate { get; }

        public Table(string title, Func<ICollection<ITableRowData>> onUpdate)
        {
            Title = title;
            OnUpdate = onUpdate;
        }

        public Table Update()
        {
            Rows = OnUpdate?.Invoke();
            return this;
        }
    }
}