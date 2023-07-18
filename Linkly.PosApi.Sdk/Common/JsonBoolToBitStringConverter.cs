using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Linkly.PosApi.Sdk.Common
{
    /// <summary>Convert between <see cref="bool" /> and bit-string. true - "1", false - "0".</summary>
    internal class JsonBoolToBitStringConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.GetString() == "1";

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value ? "1" : "0");
    }
}