using MiniDashboard.Utils;

namespace MiniDashboard.Cfg
{
    public class MiniDashboardUiConfiguration
    {
        private string _route = "/dash";

        public string Route
        {
            get => _route;
            set
            {
                ThrowIf.IsNullOrWhiteSpace(value);
                _route = $"/{value.Trim('/')}";
            }
        }
    }
}