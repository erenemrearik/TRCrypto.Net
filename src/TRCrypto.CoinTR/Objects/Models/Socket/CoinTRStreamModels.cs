namespace TRCrypto.CoinTR.Objects.Models.Socket;

/// <summary>
/// Akistan gelen bir mesajin zarfi.
/// </summary>
/// <remarks>
/// Ilk mesajin <c>action</c> alani <c>snapshot</c>, sonrakiler <c>update</c> olur.
/// Veri her zaman bir dizidir, tek ogeli olsa bile.
/// </remarks>
/// <typeparam name="T">Govde tipi.</typeparam>
[SerializationModel]
public record CoinTRStreamUpdate<T>
{
    /// <summary>["<c>action</c>"] Mesajin turu; <c>snapshot</c> ya da <c>update</c>.</summary>
    [JsonPropertyName("action")]
    public string Action { get; init; } = string.Empty;

    /// <summary>["<c>arg</c>"] Mesajin ait oldugu abonelik.</summary>
    [JsonPropertyName("arg")]
    public CoinTRStreamArgument Argument { get; init; } = new();

    /// <summary>["<c>data</c>"] Mesaj govdesi.</summary>
    [JsonPropertyName("data")]
    public List<T> Data { get; init; } = [];

    /// <summary>["<c>ts</c>"] Mesajin uretildigi an.</summary>
    [JsonPropertyName("ts")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }
}

/// <summary>Bir aboneligi tanimlayan bilgiler.</summary>
[SerializationModel]
public record CoinTRStreamArgument
{
    /// <summary>["<c>instType</c>"] Enstruman turu; spot icin <c>SPOT</c>.</summary>
    [JsonPropertyName("instType")]
    public string InstrumentType { get; init; } = string.Empty;

    /// <summary>["<c>channel</c>"] Kanal adi.</summary>
    [JsonPropertyName("channel")]
    public string Channel { get; init; } = string.Empty;

    /// <summary>["<c>instId</c>"] Native parite adi.</summary>
    [JsonPropertyName("instId")]
    public string InstrumentId { get; init; } = string.Empty;
}

/// <summary>Akistan gelen ticker guncellemesi.</summary>
[SerializationModel]
public record CoinTRStreamTicker
{
    /// <summary>["<c>instId</c>"] Native parite adi.</summary>
    [JsonPropertyName("instId")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>["<c>lastPr</c>"] Son islem fiyati.</summary>
    [JsonPropertyName("lastPr")]
    public decimal LastPrice { get; init; }

    /// <summary>["<c>open24h</c>"] 24 saat oncesindeki fiyat.</summary>
    [JsonPropertyName("open24h")]
    public decimal OpenPrice { get; init; }

    /// <summary>["<c>high24h</c>"] Son 24 saatteki en yuksek fiyat.</summary>
    [JsonPropertyName("high24h")]
    public decimal HighPrice { get; init; }

    /// <summary>["<c>low24h</c>"] Son 24 saatteki en dusuk fiyat.</summary>
    [JsonPropertyName("low24h")]
    public decimal LowPrice { get; init; }

    /// <summary>["<c>bidPr</c>"] En iyi alis fiyati.</summary>
    [JsonPropertyName("bidPr")]
    public decimal BestBidPrice { get; init; }

    /// <summary>["<c>askPr</c>"] En iyi satis fiyati.</summary>
    [JsonPropertyName("askPr")]
    public decimal BestAskPrice { get; init; }

    /// <summary>["<c>baseVolume</c>"] Base varlik cinsinden hacim.</summary>
    [JsonPropertyName("baseVolume")]
    public decimal Volume { get; init; }

    /// <summary>["<c>quoteVolume</c>"] Quote varlik cinsinden hacim.</summary>
    [JsonPropertyName("quoteVolume")]
    public decimal QuoteVolume { get; init; }

    /// <summary>
    /// ["<c>change24h</c>"] Son 24 saatteki degisim orani.
    /// </summary>
    /// <remarks>
    /// Deger <b>kesirdir</b>, yuzde degil. REST ticker ucu da ayni bicimi kullanir.
    /// </remarks>
    [JsonPropertyName("change24h")]
    public decimal ChangeRatio { get; init; }

    /// <summary>Son 24 saatteki degisim yuzdesi.</summary>
    [JsonIgnore]
    public decimal ChangePercentage => ChangeRatio * 100m;
}

/// <summary>Akistan gelen emir defteri goruntusu.</summary>
[SerializationModel]
public record CoinTRStreamOrderBook
{
    /// <summary>["<c>asks</c>"] Satis kademeleri.</summary>
    [JsonPropertyName("asks")]
    public List<CoinTROrderBookEntry> Asks { get; init; } = [];

    /// <summary>["<c>bids</c>"] Alis kademeleri.</summary>
    [JsonPropertyName("bids")]
    public List<CoinTROrderBookEntry> Bids { get; init; } = [];

    /// <summary>["<c>ts</c>"] Goruntunun uretildigi an.</summary>
    [JsonPropertyName("ts")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }
}

/// <summary>Akistan gelen islem.</summary>
[SerializationModel]
public record CoinTRStreamTrade
{
    /// <summary>["<c>tradeId</c>"] Islem kimligi.</summary>
    [JsonPropertyName("tradeId")]
    public string TradeId { get; init; } = string.Empty;

    /// <summary>["<c>price</c>"] Gerceklesme fiyati.</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    /// <summary>["<c>size</c>"] Gerceklesme miktari.</summary>
    [JsonPropertyName("size")]
    public decimal Quantity { get; init; }

    /// <summary>["<c>side</c>"] Islemin yonu; metin olarak gelir.</summary>
    [JsonPropertyName("side")]
    public OrderSide Side { get; init; }

    /// <summary>["<c>ts</c>"] Islemin gerceklestigi an.</summary>
    [JsonPropertyName("ts")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }
}
