using System.Text.Json;

namespace TRCrypto.BtcTurk.Objects.Internal;

/// <summary>
/// BtcTurk WebSocket mesaj zarfi.
/// </summary>
/// <remarks>
/// Kaynak bicimi iki elemanli bir dizidir: <c>[tip, govde]</c>. Tip numarasi govdenin
/// icinde de tekrar edilir, ancak yonlendirme dizinin ilk elemanina gore yapilir.
/// </remarks>
/// <typeparam name="T">Govde tipi.</typeparam>
[JsonConverter(typeof(BtcTurkSocketUpdateConverterFactory))]
internal record BtcTurkSocketUpdate<T>
{
    /// <summary>Mesaj kodu.</summary>
    public int Type { get; init; }

    /// <summary>Mesaj govdesi.</summary>
    public T Data { get; init; } = default!;
}

/// <summary>
/// <c>[tip, govde]</c> dizisini <see cref="BtcTurkSocketUpdate{T}"/> nesnesine cevirir.
/// </summary>
internal class BtcTurkSocketUpdateConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType
           && typeToConvert.GetGenericTypeDefinition() == typeof(BtcTurkSocketUpdate<>);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var bodyType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(BtcTurkSocketUpdateConverter<>).MakeGenericType(bodyType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <inheritdoc cref="BtcTurkSocketUpdateConverterFactory" />
/// <typeparam name="T">Govde tipi.</typeparam>
internal class BtcTurkSocketUpdateConverter<T> : JsonConverter<BtcTurkSocketUpdate<T>>
{
    /// <inheritdoc />
    public override BtcTurkSocketUpdate<T> Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("WebSocket mesaji iki elemanli bir dizi olmalidir.");

        reader.Read();
        var type = reader.GetInt32();

        reader.Read();
        var data = JsonSerializer.Deserialize<T>(ref reader, options)!;

        // Ileride diziye eleman eklenirse geri kalani atlanir.
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
        }

        return new BtcTurkSocketUpdate<T> { Type = type, Data = data };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, BtcTurkSocketUpdate<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Type);
        JsonSerializer.Serialize(writer, value.Data, options);
        writer.WriteEndArray();
    }
}
