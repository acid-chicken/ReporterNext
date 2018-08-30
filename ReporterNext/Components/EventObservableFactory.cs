using System;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class EventObservableFactory
    {
        private IDictionary<Type, IDictionary<long, EventObservable<Event>>> _observables =
            new Dictionary<Type, IDictionary<long, EventObservable<Event>>>();

        private IDictionary<long, EventObservable<Event>> GetOrCreateDictionary<T>()
            where T : Event, new() =>
            _observables.TryGetValue(typeof(T), out var observables) ?
                observables :
                CreateOrGetDictionary<T>();

        private IDictionary<long, EventObservable<Event>> CreateOrGetDictionary<T>()
            where T : Event, new()
        {
            var observables = new Dictionary<long, EventObservable<Event>>();
            return _observables.TryAdd(typeof(T), observables) ?
                observables :
                GetOrCreateDictionary<T>();
        }

        private EventObservable<Event> GetOrCreate<T>(long forUserId)
            where T : Event, new() =>
            GetOrCreateDictionary<T>().TryGetValue(forUserId, out var observable) ?
                observable :
                CreateOrGet<T>(forUserId);

        private EventObservable<Event> CreateOrGet<T>(long forUserId)
            where T : Event, new()
        {
            var observeable = new EventObservable<Event>();
            return GetOrCreateDictionary<T>().TryAdd(forUserId, observeable) ?
                observeable :
                GetOrCreate<T>(forUserId);
        }

        public EventObservable<Event> Create<T>(long forUserId)
            where T : Event, new() =>
            GetOrCreate<T>(forUserId);

        public EventObservable<Event> Create(long forUserId) =>
            Create<Event>(forUserId);
    }
}
