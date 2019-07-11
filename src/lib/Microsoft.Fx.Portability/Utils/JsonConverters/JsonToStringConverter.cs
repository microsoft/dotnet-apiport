// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;

namespace Microsoft.Fx.Portability.Utils.JsonConverters
{
    internal class JsonToStringConverter<T> : JsonConverter<T>
    {
        private readonly ConcurrentDictionary<string, T> _cache;
        private readonly Func<string, T> _factory;

        public JsonToStringConverter(Func<string, T> factory)
        {
            _cache = new ConcurrentDictionary<string, T>(StringComparer.Ordinal);
            _factory = factory;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<JToken>(reader).ToString();

            return string.IsNullOrEmpty(value) ? default : _cache.GetOrAdd(value, _factory);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }
}
