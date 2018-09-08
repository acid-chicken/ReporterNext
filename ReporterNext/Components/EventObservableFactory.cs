using System;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class EventObservableFactory
    {
        private IDictionary<Type, IDictionary<long, EventObservable>> _observables =
            new Dictionary<Type, IDictionary<long, EventObservable>>();

        private IDictionary<long, EventObservable> GetOrCreateDictionary<T>()
            where T : Event, new() =>
            _observables.TryGetValue(typeof(T), out var observables) ?
                observables :
                CreateOrGetDictionary<T>();

        private IDictionary<long, EventObservable> CreateOrGetDictionary<T>()
            where T : Event, new()
        {
            var observables = new Dictionary<long, EventObservable>();
            return _observables.TryAdd(typeof(T), observables) ?
                observables :
                GetOrCreateDictionary<T>();
        }

        private EventObservable GetOrCreate<T>(long forUserId)
            where T : Event, new() =>
            GetOrCreateDictionary<T>().TryGetValue(forUserId, out var observable) ?
                observable :
                CreateOrGet<T>(forUserId);

        private EventObservable CreateOrGet<T>(long forUserId)
            where T : Event, new()
        {
            var observeable = new EventObservable();
            return GetOrCreateDictionary<T>().TryAdd(forUserId, observeable) ?
                observeable :
                GetOrCreate<T>(forUserId);
        }

        public EventObservable Create<T>(long forUserId)
            where T : Event, new() =>
            GetOrCreate<T>(forUserId);

        public EventObservable Create(long forUserId) =>
            Create<Event>(forUserId);
    }
}
