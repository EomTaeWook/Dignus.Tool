using Dignus.Collections;
using System.Collections.Generic;

namespace Shared.Buffer
{
    public class DataStream<T>
    {
        public bool CanRead { get => _itemContainer.Count > _position; }
        public int Length { get => _itemContainer.Count; }
        public int Position { get => _position; }

        private readonly ArrayQueue<T> _itemContainer = new ArrayQueue<T>();

        private int _position;
        public DataStream(T[] items)
        {
            _itemContainer.AddRange(items);
        }
        public DataStream(ICollection<T> items)
        {
            _itemContainer.AddRange(items);
        }
        public T Read()
        {
            if (CanRead == false)
            {
                return default;
            }
            var item = _itemContainer[_position++];
            return item;
        }
        public T Peek()
        {
            if (CanRead == false)
            {
                return default;
            }
            return _itemContainer[_position];
        }
        public void SetPosition(int position)
        {
            _position = position;
        }
        internal void Add(T item)
        {
            _itemContainer.Add(item);
        }
        internal void Clear()
        {
            _itemContainer.Clear();
        }
    }
}
