using System.Collections.Generic;

namespace MiniDashboard.Data
{
    public record Error(string Message, ushort Code)
    {
        public static Error NotFound() => new("Not found", 400);
    };

    public record ResponseModel
    {
        public IEnumerable<Card> Cards { get; init; }
        public IEnumerable<Chart> Charts { get; init; }
        public IEnumerable<Table> Tables { get; set; }
    }
}