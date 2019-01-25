// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Utils.JsonConverters
{
    /// <summary>
    /// Json.NET does not handle <see cref="IDictionary{TKey, TValue}"/> when TKey is a type other than string.  This provides
    /// a wrapper for these types to serialize in a Key/Value system (like DCJS).
    /// </summary>
    internal class JsonMultiDictionaryConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, ICollection<TValue>>>
    {
        private class KeyValueHelper
        {
            public TKey Key { get; set; }

            public ICollection<TValue> Value { get; set; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer
                .Deserialize<IEnumerable<KeyValueHelper>>(reader)
                .ToDictionary(t => t.Key, t => t.Value);
        }

        public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
        {
            var data = (obj as IDictionary<TKey, ICollection<TValue>>)
                .Select(d => new KeyValueHelper { Key = d.Key, Value = d.Value });

            serializer.Serialize(writer, data);
        }
    }
}
