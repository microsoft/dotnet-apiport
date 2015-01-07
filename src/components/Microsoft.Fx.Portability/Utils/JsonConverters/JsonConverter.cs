using Newtonsoft.Json;
using System;

namespace Microsoft.Fx.Portability.Utils.JsonConverters
{
    internal abstract class JsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }
    }
}
