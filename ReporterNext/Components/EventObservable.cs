using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using Hangfire;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class EventObservable : IObservable<Event>, IDisposable
    {
        private readonly IList<IObserver<Event>> _observers = new List<IObserver<Event>>();

        private bool disposedValue = false;

        public void Execute<T>(T content, bool fallback = false)
            where T : Event
        {
            foreach (var observer in _observers)
                if (observer is IObserver<T> x)
                    x.OnNext(content);
                else if (fallback)
                    observer.OnNext(content);
        }

        public void Execute(Event content)
        {
            foreach (var observer in _observers)
                observer.OnNext(content);
        }

        public IDisposable Subscribe(IObserver<Event> observer, bool neverUnsubscribe = false)
        {
            _observers.Add(observer);
            return neverUnsubscribe ?
                null :
                new UnsubscribeOnDispose<Event>(_observers, observer);
        }

        public IDisposable Subscribe(IObserver<Event> observer) =>
            Subscribe(observer, false);

        protected virtual void Dispose(bool disposing) =>
            DisposeAsync(disposing).RunSynchronously();

        protected virtual Task DisposeAsync(bool disposing) =>
            !disposedValue &&
                disposing &&
                (disposedValue = true) ?
                    Task.WhenAll(_observers.Select(x => Task.Run(() => x.OnCompleted())).ToArray()) :
                    Task.CompletedTask;

        public void Dispose() =>
            Dispose(true);
    }
}
