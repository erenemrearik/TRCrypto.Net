namespace TRCrypto.BtcTurk.Objects.Models;

/// <summary>Gerceklesmis bir piyasa islemi.</summary>
[SerializationModel]
public record BtcTurkTrade
{
    /// <summary>["<c>pair</c>"] Native sembol adi, ornegin <c>BTCTRY</c>.</summary>
    [JsonPropertyName("pair")]
    public string Pair { get; init; } = string.Empty;

    /// <summary>["<c>pairNormalized</c>"] Normalize edilmis ad, ornegin <c>BTC_TRY</c>.</summary>
    [JsonPropertyName("pairNormalized")]
    public string PairNormalized { get; init; } = string.Empty;

    /// <summary>["<c>numerator</c>"] Base varlik, ornegin <c>BTC</c>.</summary>
    [JsonPropertyName("numerator")]
    public string Numerator { get; init; } = string.Empty;

    /// <summary>["<c>denominator</c>"] Quote varlik, ornegin <c>TRY</c>.</summary>
    [JsonPropertyName("denominator")]
    public string Denominator { get; init; } = string.Empty;

    /// <summary>["<c>date</c>"] Islemin gerceklestigi an (UTC).</summary>
    [JsonPropertyName("date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>tid</c>"] Islem kimligi.</summary>
    [JsonPropertyName("tid")]
    public string Id { get; init; } = string.Empty;

    /// <summary>["<c>price</c>"] Islem fiyati.</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    /// <summary>["<c>amount</c>"] Islem miktari (base varlik cinsinden).</summary>
    [JsonPropertyName("amount")]
    public decimal Quantity { get; init; }

    /// <summary>["<c>side</c>"] Islemin yonu.</summary>
    /// <remarks>Resmi ornek yanitta yer almaz ancak canli API bu alani dondurur.</remarks>
    [JsonPropertyName("side")]
    public OrderSide Side { get; init; }
}
