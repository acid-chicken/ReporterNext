using System;
using System.Collections;
using System.Collections.Generic;

namespace ReporterNext.Components
{
    public class UndisposingObjectCollection : ICollection<IDisposable>
    {
        private IList<IDisposable> _list = new List<IDisposable>();

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public void Add(IDisposable item) =>
            _list.Add(item);

        public void Clear() =>
            _list.Clear();

        public bool Contains(IDisposable item) =>
            _list.Contains(item);

        public void CopyTo(IDisposable[] array, int arrayIndex) =>
            _list.CopyTo(array, arrayIndex);

        public bool Remove(IDisposable item) =>
            _list.Remove(item);

        public IEnumerator<IDisposable> GetEnumerator() =>
            _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _list.GetEnumerator();
    }
}
