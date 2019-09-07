using Microsoft.Fx.Portability.ObjectModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Fx.Portability.Utils.JsonConverters
{
    internal class JsonPublicKeyTokenConverter : JsonConverter<PublicKeyToken>
    { 
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            ImmutableArray<byte> bytes = serializer
                .Deserialize<ImmutableArray<byte>>(reader);
            return new PublicKeyToken(bytes);
        }

        public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
        {
            PublicKeyToken pkToken = (PublicKeyToken)obj;
            serializer.Serialize(writer, (ImmutableArray<byte>)pkToken.Token);
        }
    }
}
