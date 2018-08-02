using System;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class EventObservableFactory
    {
        private IDictionary<Type, IDictionary<long, EventObservable<IEvent>>> _observables =
            new Dictionary<Type, IDictionary<long, EventObservable<IEvent>>>();

        private IDictionary<long, EventObservable<T>> GetOrCreateDictionary<T>()
            where T : class, IEvent, new() =>
            _observables.TryGetValue(typeof(T), out var observables) ?
                observables
                    .ToDictionary(x => x.Key, x => x.Value as EventObservable<T>) :
                CreateOrGetDictionary<T>();

        private IDictionary<long, EventObservable<T>> CreateOrGetDictionary<T>()
            where T : class, IEvent, new()
        {
            var observables = new Dictionary<long, EventObservable<IEvent>>();
            return _observables.TryAdd(typeof(T), observables) ?
                observables
                    .ToDictionary(x => x.Key, x => x.Value as EventObservable<T>) :
                GetOrCreateDictionary<T>();
        }

        private EventObservable<T> GetOrCreate<T>(long forUserId)
            where T : class, IEvent, new() =>
            GetOrCreateDictionary<T>().TryGetValue(forUserId, out var observable) ?
                observable :
                CreateOrGet<T>(forUserId);

        private EventObservable<T> CreateOrGet<T>(long forUserId)
            where T : class, IEvent, new()
        {
            var observeable = new EventObservable<T>();
            return GetOrCreateDictionary<T>().TryAdd(forUserId, observeable) ?
                observeable :
                GetOrCreate<T>(forUserId);
        }

        public EventObservable<T> Create<T>(long forUserId)
            where T : class, IEvent, new() =>
            GetOrCreate<T>(forUserId);
    }
}
