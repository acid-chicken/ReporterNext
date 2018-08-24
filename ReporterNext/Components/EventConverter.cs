using System;
using Newtonsoft.Json;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class EventConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(IEvent);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            serializer.Deserialize(reader, typeof(Event));

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            serializer.Serialize(writer, value);
    }
}
