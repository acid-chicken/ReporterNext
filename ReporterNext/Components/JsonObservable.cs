using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class JsonObservable : IObservable<EventObject>, IDisposable
    {
        private readonly IList<IObserver<EventObject>> _observers = new List<IObserver<EventObject>>();

        private bool disposedValue = false;

        public void Execute(EventObject content)
        {
            foreach (var observer in _observers)
                BackgroundJob.Enqueue(() => observer.OnNext(content));
        }

        public IDisposable Subscribe(IObserver<EventObject> observer, bool neverUnsubscribe = false)
        {
            _observers.Add(observer);
            return neverUnsubscribe ?
                null :
                new UnsubscribeOnDispose<EventObject>(_observers, observer);
        }

        public IDisposable Subscribe(IObserver<EventObject> observer) =>
            Subscribe(observer, false);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Task.WaitAll(_observers.Select(x => Task.Run(() => x.OnCompleted())).ToArray());
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
