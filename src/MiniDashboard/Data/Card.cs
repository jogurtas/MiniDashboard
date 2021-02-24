using System;

namespace MiniDashboard.Data
{
    public enum CardType
    {
        Static,
        Dynamic,
        Link
    }

    public class Card
    {
        public string Title { get; }
        public string Value { get; }
        public CardType Type { get; }
        public string Data { get; private set; }
        
        private Func<string> OnUpdate { get; set; }

        public Card(string title, string value, CardType type, Func<string> onUpdate)
        {
            Title = title;
            Value = value;
            Type = type;
            OnUpdate = onUpdate;
        }

        public Card Update()
        {
            Data = OnUpdate?.Invoke();
            return this;
        }
    }
}