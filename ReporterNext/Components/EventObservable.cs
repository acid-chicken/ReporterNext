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
        where T : Event
    {
        private readonly IList<IObserver<T>> _observers = new List<IObserver<T>>();

        private bool disposedValue = false;

        public void Execute(T content)
        {
            foreach (var observer in _observers)
                BackgroundJob.Enqueue(() => observer.OnNext(content));
        }

        public IDisposable Subscribe(IObserver<T> observer, bool neverUnsubscribe = false)
        {
            _observers.Add(observer);
            return neverUnsubscribe ?
                null :
                new UnsubscribeOnDispose<T>(_observers, observer);
        }

        public IDisposable Subscribe(IObserver<T> observer) =>
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
