using TRCrypto.BtcTurk.Converters;

namespace TRCrypto.BtcTurk.Objects.Models;

/// <summary>Bir emrin durumu ve kosullari.</summary>
/// <remarks>
/// Alan adlari uclar arasinda farkli yazilir (<c>pairSymbol</c> / <c>pairsymbol</c>);
/// ayristirma buyuk-kucuk harf duyarsizdir.
/// </remarks>
[SerializationModel]
public record BtcTurkOrder
{
    /// <summary>["<c>id</c>"] Emir kimligi.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>["<c>price</c>"] Emir fiyati.</summary>
    [JsonPropertyName("price")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Price { get; init; }

    /// <summary>["<c>stopPrice</c>"] Stop emirlerinde tetikleme fiyati.</summary>
    [JsonPropertyName("stopPrice")]
    [JsonConverter(typeof(BtcTurkNullableDecimalConverter))]
    public decimal? StopPrice { get; init; }

    /// <summary>["<c>quantity</c>"] Emir miktari (base varlik cinsinden).</summary>
    [JsonPropertyName("quantity")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Quantity { get; init; }

    /// <summary>["<c>amount</c>"] Emir tutari.</summary>
    [JsonPropertyName("amount")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Amount { get; init; }

    /// <summary>["<c>leftAmount</c>"] Henuz gerceklesmemis miktar.</summary>
    [JsonPropertyName("leftAmount")]
    [JsonConverter(typeof(BtcTurkNullableDecimalConverter))]
    public decimal? RemainingQuantity { get; init; }

    /// <summary>["<c>pairSymbol</c>"] Native sembol adi, ornegin <c>BTCTRY</c>.</summary>
    [JsonPropertyName("pairSymbol")]
    public string PairSymbol { get; init; } = string.Empty;

    /// <summary>["<c>pairSymbolNormalized</c>"] Normalize edilmis ad, ornegin <c>BTC_TRY</c>.</summary>
    [JsonPropertyName("pairSymbolNormalized")]
    public string? PairSymbolNormalized { get; init; }

    /// <summary>["<c>type</c>"] Emrin yonu.</summary>
    [JsonPropertyName("type")]
    public OrderSide Side { get; init; }

    /// <summary>["<c>method</c>"] Emrin yontemi.</summary>
    [JsonPropertyName("method")]
    public OrderMethod Method { get; init; }

    /// <summary>["<c>status</c>"] Emrin durumu.</summary>
    [JsonPropertyName("status")]
    public OrderStatus Status { get; init; }

    /// <summary>["<c>orderClientId</c>"] Cagiran tarafin verdigi emir kimligi.</summary>
    [JsonPropertyName("orderClientId")]
    public string? ClientOrderId { get; init; }

    /// <summary>["<c>time</c>"] Emrin olusturuldugu an (UTC).</summary>
    [JsonPropertyName("time")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreateTime { get; init; }

    /// <summary>["<c>updateTime</c>"] Emrin son guncellendigi an (UTC).</summary>
    [JsonPropertyName("updateTime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdateTime { get; init; }
}

/// <summary>Bir paritedeki acik emirler; alis ve satis olarak ayrilmistir.</summary>
[SerializationModel]
public record BtcTurkOpenOrders
{
    /// <summary>["<c>asks</c>"] Acik satis emirleri.</summary>
    [JsonPropertyName("asks")]
    public IReadOnlyList<BtcTurkOrder> Asks { get; init; } = [];

    /// <summary>["<c>bids</c>"] Acik alis emirleri.</summary>
    [JsonPropertyName("bids")]
    public IReadOnlyList<BtcTurkOrder> Bids { get; init; } = [];
}

/// <summary>
/// Yeni olusturulan bir emrin borsadan donen bilgisi.
/// </summary>
/// <remarks>
/// Bu uc, emir sorgulama uclarindan farkli alan adlari kullanir: olusturulma zamani
/// <c>datetime</c>, cagiran tarafin kimligi <c>newOrderClientId</c> olarak gelir.
/// </remarks>
[SerializationModel]
public record BtcTurkOrderPlacement
{
    /// <summary>["<c>id</c>"] Emir kimligi.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>["<c>datetime</c>"] Emrin olusturuldugu an (UTC).</summary>
    [JsonPropertyName("datetime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreateTime { get; init; }

    /// <summary>["<c>type</c>"] Emrin yonu.</summary>
    [JsonPropertyName("type")]
    public OrderSide Side { get; init; }

    /// <summary>["<c>method</c>"] Emrin yontemi.</summary>
    [JsonPropertyName("method")]
    public OrderMethod Method { get; init; }

    /// <summary>["<c>price</c>"] Emir fiyati.</summary>
    [JsonPropertyName("price")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Price { get; init; }

    /// <summary>["<c>stopPrice</c>"] Stop emirlerinde tetikleme fiyati.</summary>
    [JsonPropertyName("stopPrice")]
    [JsonConverter(typeof(BtcTurkNullableDecimalConverter))]
    public decimal? StopPrice { get; init; }

    /// <summary>["<c>quantity</c>"] Emir miktari.</summary>
    [JsonPropertyName("quantity")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Quantity { get; init; }

    /// <summary>["<c>pairSymbol</c>"] Native sembol adi.</summary>
    [JsonPropertyName("pairSymbol")]
    public string PairSymbol { get; init; } = string.Empty;

    /// <summary>["<c>pairSymbolNormalized</c>"] Normalize edilmis ad.</summary>
    [JsonPropertyName("pairSymbolNormalized")]
    public string? PairSymbolNormalized { get; init; }

    /// <summary>["<c>newOrderClientId</c>"] Cagiran tarafin verdigi emir kimligi.</summary>
    [JsonPropertyName("newOrderClientId")]
    public string? ClientOrderId { get; init; }
}
