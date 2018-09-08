using System;
using System.Collections.Generic;

namespace ReporterNext.Components
{
    internal class UnsubscribeOnDispose<T> : IDisposable
    {
        private readonly IList<IObserver<T>> _observers;

        private readonly IObserver<T> _observer;

        private bool _disposed = false;

        internal UnsubscribeOnDispose(IList<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _observers.Remove(_observer);
                _disposed = true;
            }
        }
        public void Dispose() =>
            Dispose(true);
    }
}
