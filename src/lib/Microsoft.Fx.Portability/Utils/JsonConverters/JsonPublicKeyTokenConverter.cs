// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
            string publicKeyTokenString = serializer
                .Deserialize<string>(reader);
            return PublicKeyToken.Parse(publicKeyTokenString);
        }

        public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
        {
            PublicKeyToken pkToken = (PublicKeyToken)obj;
            serializer.Serialize(writer, pkToken.ToString());
        }
    }
}
