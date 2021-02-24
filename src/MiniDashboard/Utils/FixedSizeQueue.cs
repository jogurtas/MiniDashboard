using System.Collections.Concurrent;

namespace MiniDashboard.Utils
{
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        public int Size { get; }
        private readonly object _syncObject = new ();

        public FixedSizedQueue(int size) => Size = size;

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (_syncObject)
            {
                while (Count > Size)
                {
                    TryDequeue(out _);
                }
            }
        }
    }
}