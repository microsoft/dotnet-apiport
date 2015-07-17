// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Utils.JsonConverters;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    public static class DataExtensions
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[]
            {
                new JsonMultiDictionaryConverter<MemberInfo, AssemblyInfo>(),
                new JsonToStringConverter<FrameworkName>(s => new FrameworkName(s)),
                new JsonToStringConverter<Version>(s => new Version(s)),
            }
        };

        public static readonly JsonSerializer Serializer = JsonSerializer.Create(JsonSettings);

        public static byte[] Serialize<T>(this T data)
        {
            var str = JsonConvert.SerializeObject(data, Formatting.Indented, JsonSettings);

            using (var outputStream = new MemoryStream())
            using (var writer = new StreamWriter(outputStream))
            {
                writer.Write(str);
                writer.Flush();

                return outputStream.ToArray();
            }
        }

        public static T Deserialize<T>(this Stream stream)
        {
            var reader = new StreamReader(stream);

            return (T)Serializer.Deserialize(reader, typeof(T));
        }

        public static T Deserialize<T>(this byte[] data)
        {
            using (MemoryStream dataStream = new MemoryStream(data))
            {
                return Deserialize<T>(dataStream);
            }
        }

        public static byte[] Compress(this byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    compressStream.Write(data, 0, data.Length);
                }

                return outputStream.ToArray();
            }
        }

        public static T DecompressToObject<T>(this Stream stream)
        {
            using (var decompressStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                return decompressStream.Deserialize<T>();
            }
        }

        public static T DecompressToObject<T>(this byte[] data)
        {
            using (var dataStream = new MemoryStream(data))
            {
                return dataStream.DecompressToObject<T>();
            }
        }
    }
}
