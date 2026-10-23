using System.Text.Json;
using System.Text.Json.Serialization;

namespace AcademicApi.Converters;

/// <summary>
/// Memangkas spasi di awal/akhir string saat JSON dibaca, sehingga validasi
/// dan data yang disimpan selalu memakai nilai yang sudah bersih.
/// </summary>
public sealed class TrimmedStringJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return reader.GetString()?.Trim();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
