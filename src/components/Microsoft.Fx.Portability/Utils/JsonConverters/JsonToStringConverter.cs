using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Fx.Portability.Utils.JsonConverters
{
    internal class JsonToStringConverter<T> : JsonConverter<T>
    {
        private readonly Func<string, T> _factory;

        public JsonToStringConverter(Func<string, T> factory)
        {
            _factory = factory;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<JToken>(reader).ToString();

            return String.IsNullOrEmpty(value) ? default(T) : _factory(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }
}
