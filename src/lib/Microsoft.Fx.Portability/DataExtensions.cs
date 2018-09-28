// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Utils.JsonConverters;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public static class DataExtensions
    {
        private const int DefaultBufferSize = 1024;
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public static JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[]
            {
                new JsonMultiDictionaryConverter<MemberInfo, AssemblyInfo>(),
                new JsonToStringConverter<FrameworkName>(s => new FrameworkName(s)),
                new JsonToStringConverter<Version>(s => new Version(s)),
            }
        };

        public static JsonSerializer Serializer { get; } = JsonSerializer.Create(JsonSettings);

        public static byte[] Serialize<T>(this T data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(outputStream))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        var serializer = JsonSerializer.Create(JsonSettings);
                        serializer.Formatting = Formatting.None;
                        serializer.Serialize(jsonWriter, data);
                    }
                }

                return outputStream.ToArray();
            }
        }

        public static byte[] SerializeAndCompress<T>(this T data)
        {
            using (var jsonSerializedStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(jsonSerializedStream))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        Serializer.Serialize(jsonWriter, data);
                        jsonWriter.Flush();

                        using (var outputStream = new MemoryStream())
                        {
                            using (var compressStream = new GZipStream(outputStream, CompressionMode.Compress))
                            {
                                jsonSerializedStream.WriteTo(compressStream);
                            }

                            return outputStream.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Serializes an object to Json and writes the output to the given stream.
        /// </summary>
        /// <param name="data">object to serialize</param>
        /// <param name="outputStream">Stream to write Json to</param>
        /// <param name="leaveOpen">true to leave the stream open; false otherwise</param>
        public static void Serialize<T>(this T data, Stream outputStream, bool leaveOpen)
        {
            using (var writer = new StreamWriter(outputStream, DefaultEncoding, DefaultBufferSize, leaveOpen))
            {
                Serializer.Serialize(writer, data);
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

        /// <summary>
        /// Given the input stream, will take its contents and compress them into the output stream.
        /// </summary>
        /// <param name="inputStream">Input stream to read contents from</param>
        /// <param name="outputStream">Stream to write contents to</param>
        /// <param name="leaveOpen">Whether to leave the input and output streams open after reading/writing to/from them.</param>
        /// <returns></returns>
        public static async Task CompressAsync(this Stream inputStream, Stream outputStream, bool leaveOpen)
        {
            using (var reader = new BinaryReader(inputStream, DefaultEncoding, leaveOpen))
            {
                using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress, leaveOpen))
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var buffer = reader.ReadBytes(DefaultBufferSize);

                        await compressionStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
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
