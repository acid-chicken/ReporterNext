using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using Hangfire;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class EventObservable<T> : IObservable<T>, IDisposable
        where T : IEvent
    {
        private readonly IList<IObserver<T>> _observers = new List<IObserver<T>>();

        private bool disposedValue = false;

        public void Execute(T content)
        {
            BackgroundJob.Enqueue(() => Console.WriteLine(_observers));
            foreach (var observer in _observers)
                BackgroundJob.Enqueue(() => observer.OnNext(content));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new UnsubscribeOnDispose(_observers, observer);
        }

        private class UnsubscribeOnDispose : IDisposable
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Task.WhenAll(_observers.Select(x => Task.Run(() => x.OnCompleted()))).GetAwaiter().GetResult();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
