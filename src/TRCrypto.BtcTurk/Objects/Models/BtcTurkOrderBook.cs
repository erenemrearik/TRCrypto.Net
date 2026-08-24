using System.Globalization;
using System.Text.Json;

namespace TRCrypto.BtcTurk.Objects.Models;

/// <summary>Bir parite icin acik emirlerin anlik goruntusu.</summary>
[SerializationModel]
public record BtcTurkOrderBook
{
    /// <summary>["<c>timestamp</c>"] Goruntunun alindigi an (UTC).</summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>bids</c>"] Alis kademeleri; fiyata gore azalan sirada.</summary>
    [JsonPropertyName("bids")]
    public IReadOnlyList<BtcTurkOrderBookEntry> Bids { get; init; } = [];

    /// <summary>["<c>asks</c>"] Satis kademeleri; fiyata gore artan sirada.</summary>
    [JsonPropertyName("asks")]
    public IReadOnlyList<BtcTurkOrderBookEntry> Asks { get; init; } = [];
}

/// <summary>Emir defterinde tek bir fiyat kademesi.</summary>
/// <remarks>Kaynakta iki elemanli bir dizi olarak gelir: once fiyat, sonra miktar.</remarks>
[JsonConverter(typeof(BtcTurkOrderBookEntryConverter))]
public record BtcTurkOrderBookEntry : ISymbolOrderBookEntry
{
    /// <summary>Kademe fiyati.</summary>
    public decimal Price { get; init; }

    /// <summary>Bu fiyattaki toplam miktar.</summary>
    public decimal Quantity { get; init; }

    decimal ISymbolOrderBookEntry.Price
    {
        get => Price;
        set => throw new NotSupportedException("Emir defteri kademesi degistirilemez.");
    }

    decimal ISymbolOrderBookEntry.Quantity
    {
        get => Quantity;
        set => throw new NotSupportedException("Emir defteri kademesi degistirilemez.");
    }
}

/// <summary>
/// Iki elemanli fiyat/miktar dizisini <see cref="BtcTurkOrderBookEntry"/> nesnesine cevirir.
/// </summary>
internal class BtcTurkOrderBookEntryConverter : JsonConverter<BtcTurkOrderBookEntry>
{
    /// <inheritdoc />
    public override BtcTurkOrderBookEntry Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Emir defteri kademesi bir dizi olmalidir.");

        reader.Read();
        var price = ReadDecimal(ref reader);

        reader.Read();
        var quantity = ReadDecimal(ref reader);

        // Ileride dizinin sonuna eleman eklenirse geri kalani atlanir.
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
        }

        return new BtcTurkOrderBookEntry { Price = price, Quantity = quantity };
    }

    private static decimal ReadDecimal(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => decimal.Parse(
                reader.GetString()!, NumberStyles.Any, CultureInfo.InvariantCulture),
            JsonTokenType.Number => reader.GetDecimal(),
            _ => throw new JsonException("Emir defteri degeri sayi ya da metin olmalidir.")
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        BtcTurkOrderBookEntry value,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Price.ToString(CultureInfo.InvariantCulture));
        writer.WriteStringValue(value.Quantity.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndArray();
    }
}
