namespace TRCrypto.BinanceTR.Objects.Models.Socket;

/// <summary>Tum akis mesajlarinin ortak alanlari.</summary>
public abstract record BinanceTRStreamEvent
{
    /// <summary>["<c>e</c>"] Olay turu, ornegin <c>24hrTicker</c>.</summary>
    [JsonPropertyName("e")]
    public string? EventType { get; init; }

    /// <summary>["<c>E</c>"] Olayin uretildigi an (UTC).</summary>
    [JsonPropertyName("E")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime EventTime { get; init; }

    /// <summary>
    /// ["<c>s</c>"] Native sembol adi.
    /// </summary>
    /// <remarks>
    /// Akis yanitlarinda sembol <b>alt cizgisiz</b> gelir (<c>BTCTRY</c>); REST tarafinda
    /// ise alt cizgilidir (<c>BTC_TRY</c>). Abonelik ise kucuk harfle yapilir.
    /// </remarks>
    [JsonPropertyName("s")]
    public string Symbol { get; init; } = string.Empty;
}

/// <summary>24 saatlik ozet fiyat guncellemesi.</summary>
[SerializationModel]
public record BinanceTRStreamTicker : BinanceTRStreamEvent
{
    /// <summary>["<c>c</c>"] Son islem fiyati.</summary>
    [JsonPropertyName("c")]
    public decimal LastPrice { get; init; }

    /// <summary>["<c>o</c>"] 24 saat oncesindeki fiyat.</summary>
    [JsonPropertyName("o")]
    public decimal OpenPrice { get; init; }

    /// <summary>["<c>h</c>"] Son 24 saatteki en yuksek fiyat.</summary>
    [JsonPropertyName("h")]
    public decimal HighPrice { get; init; }

    /// <summary>["<c>l</c>"] Son 24 saatteki en dusuk fiyat.</summary>
    [JsonPropertyName("l")]
    public decimal LowPrice { get; init; }

    /// <summary>["<c>p</c>"] Son 24 saatteki fiyat degisimi (mutlak).</summary>
    [JsonPropertyName("p")]
    public decimal Change { get; init; }

    /// <summary>["<c>P</c>"] Son 24 saatteki fiyat degisimi (yuzde).</summary>
    [JsonPropertyName("P")]
    public decimal ChangePercentage { get; init; }

    /// <summary>["<c>w</c>"] Agirlikli ortalama fiyat.</summary>
    [JsonPropertyName("w")]
    public decimal WeightedAveragePrice { get; init; }

    /// <summary>["<c>b</c>"] En iyi alis fiyati.</summary>
    [JsonPropertyName("b")]
    public decimal BestBidPrice { get; init; }

    /// <summary>["<c>B</c>"] En iyi alis miktari.</summary>
    [JsonPropertyName("B")]
    public decimal BestBidQuantity { get; init; }

    /// <summary>["<c>a</c>"] En iyi satis fiyati.</summary>
    [JsonPropertyName("a")]
    public decimal BestAskPrice { get; init; }

    /// <summary>["<c>A</c>"] En iyi satis miktari.</summary>
    [JsonPropertyName("A")]
    public decimal BestAskQuantity { get; init; }

    /// <summary>["<c>v</c>"] Islem hacmi (base varlik cinsinden).</summary>
    [JsonPropertyName("v")]
    public decimal Volume { get; init; }

    /// <summary>["<c>q</c>"] Islem hacmi (quote varlik cinsinden).</summary>
    [JsonPropertyName("q")]
    public decimal QuoteVolume { get; init; }

    /// <summary>["<c>n</c>"] Islem sayisi.</summary>
    [JsonPropertyName("n")]
    public long TradeCount { get; init; }
}

/// <summary>Gerceklesmis tek bir islem.</summary>
[SerializationModel]
public record BinanceTRStreamTrade : BinanceTRStreamEvent
{
    /// <summary>["<c>t</c>"] Islem kimligi.</summary>
    [JsonPropertyName("t")]
    public long Id { get; init; }

    /// <summary>["<c>p</c>"] Islem fiyati.</summary>
    [JsonPropertyName("p")]
    public decimal Price { get; init; }

    /// <summary>["<c>q</c>"] Islem miktari.</summary>
    [JsonPropertyName("q")]
    public decimal Quantity { get; init; }

    /// <summary>["<c>T</c>"] Islemin gerceklestigi an (UTC).</summary>
    [JsonPropertyName("T")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>m</c>"] Alici tarafin piyasa yapici olup olmadigi.</summary>
    [JsonPropertyName("m")]
    public bool BuyerIsMaker { get; init; }

    /// <summary>Islemin yonu.</summary>
    /// <remarks>
    /// Akis yonu dogrudan vermez. Alici piyasa yapiciysa emri veren taraf saticidir.
    /// </remarks>
    [JsonIgnore]
    public OrderSide Side => BuyerIsMaker ? OrderSide.Sell : OrderSide.Buy;
}

/// <summary>Toplulastirilmis islem.</summary>
[SerializationModel]
public record BinanceTRStreamAggregatedTrade : BinanceTRStreamEvent
{
    /// <summary>["<c>a</c>"] Toplulastirilmis islem kimligi.</summary>
    [JsonPropertyName("a")]
    public long Id { get; init; }

    /// <summary>["<c>p</c>"] Islem fiyati.</summary>
    [JsonPropertyName("p")]
    public decimal Price { get; init; }

    /// <summary>["<c>q</c>"] Islem miktari.</summary>
    [JsonPropertyName("q")]
    public decimal Quantity { get; init; }

    /// <summary>["<c>f</c>"] Toplulastirmadaki ilk islem kimligi.</summary>
    [JsonPropertyName("f")]
    public long FirstTradeId { get; init; }

    /// <summary>["<c>l</c>"] Toplulastirmadaki son islem kimligi.</summary>
    [JsonPropertyName("l")]
    public long LastTradeId { get; init; }

    /// <summary>["<c>T</c>"] Islemin gerceklestigi an (UTC).</summary>
    [JsonPropertyName("T")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>m</c>"] Alici tarafin piyasa yapici olup olmadigi.</summary>
    [JsonPropertyName("m")]
    public bool BuyerIsMaker { get; init; }

    /// <summary>Islemin yonu.</summary>
    [JsonIgnore]
    public OrderSide Side => BuyerIsMaker ? OrderSide.Sell : OrderSide.Buy;
}

/// <summary>Emir defteri farki.</summary>
/// <remarks>
/// Miktari <c>0</c> olan bir kademe, o fiyat seviyesinin defterden kaldirildigini bildirir.
/// </remarks>
[SerializationModel]
public record BinanceTRStreamOrderBookUpdate : BinanceTRStreamEvent
{
    /// <summary>["<c>U</c>"] Bu guncellemenin kapsadigi ilk sira numarasi.</summary>
    [JsonPropertyName("U")]
    public long FirstUpdateId { get; init; }

    /// <summary>["<c>u</c>"] Bu guncellemenin kapsadigi son sira numarasi.</summary>
    /// <remarks>Ardisik guncellemelerde atlama bu alanla tespit edilir.</remarks>
    [JsonPropertyName("u")]
    public long LastUpdateId { get; init; }

    /// <summary>["<c>b</c>"] Degisen alis kademeleri.</summary>
    [JsonPropertyName("b")]
    public IReadOnlyList<BinanceTROrderBookEntry> Bids { get; init; } = [];

    /// <summary>["<c>a</c>"] Degisen satis kademeleri.</summary>
    [JsonPropertyName("a")]
    public IReadOnlyList<BinanceTROrderBookEntry> Asks { get; init; } = [];
}

/// <summary>Emir defterinin tam goruntusu.</summary>
/// <remarks>Bu akis olay turu alani tasimaz.</remarks>
[SerializationModel]
public record BinanceTRStreamOrderBook
{
    /// <summary>["<c>lastUpdateId</c>"] Goruntunun sira numarasi.</summary>
    [JsonPropertyName("lastUpdateId")]
    public long LastUpdateId { get; init; }

    /// <summary>["<c>bids</c>"] Alis kademeleri.</summary>
    [JsonPropertyName("bids")]
    public IReadOnlyList<BinanceTROrderBookEntry> Bids { get; init; } = [];

    /// <summary>["<c>asks</c>"] Satis kademeleri.</summary>
    [JsonPropertyName("asks")]
    public IReadOnlyList<BinanceTROrderBookEntry> Asks { get; init; } = [];
}

/// <summary>Mum guncellemesi.</summary>
[SerializationModel]
public record BinanceTRStreamKlineUpdate : BinanceTRStreamEvent
{
    /// <summary>["<c>k</c>"] Mum verisi.</summary>
    [JsonPropertyName("k")]
    public BinanceTRStreamKline Kline { get; init; } = new();
}

/// <summary>Bir zaman araligindaki fiyat hareketi.</summary>
[SerializationModel]
public record BinanceTRStreamKline
{
    /// <summary>["<c>t</c>"] Aralik baslangici (UTC).</summary>
    [JsonPropertyName("t")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime OpenTime { get; init; }

    /// <summary>["<c>T</c>"] Aralik bitisi (UTC).</summary>
    [JsonPropertyName("T")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CloseTime { get; init; }

    /// <summary>["<c>i</c>"] Aralik, ornegin <c>1m</c>.</summary>
    [JsonPropertyName("i")]
    public string Interval { get; init; } = string.Empty;

    /// <summary>["<c>o</c>"] Acilis fiyati.</summary>
    [JsonPropertyName("o")]
    public decimal OpenPrice { get; init; }

    /// <summary>["<c>h</c>"] En yuksek fiyat.</summary>
    [JsonPropertyName("h")]
    public decimal HighPrice { get; init; }

    /// <summary>["<c>l</c>"] En dusuk fiyat.</summary>
    [JsonPropertyName("l")]
    public decimal LowPrice { get; init; }

    /// <summary>["<c>c</c>"] Kapanis fiyati.</summary>
    [JsonPropertyName("c")]
    public decimal ClosePrice { get; init; }

    /// <summary>["<c>v</c>"] Hacim (base varlik cinsinden).</summary>
    [JsonPropertyName("v")]
    public decimal Volume { get; init; }

    /// <summary>["<c>q</c>"] Hacim (quote varlik cinsinden).</summary>
    [JsonPropertyName("q")]
    public decimal QuoteVolume { get; init; }

    /// <summary>["<c>n</c>"] Islem sayisi.</summary>
    [JsonPropertyName("n")]
    public long TradeCount { get; init; }

    /// <summary>
    /// ["<c>x</c>"] Mumun kapanip kapanmadigi.
    /// </summary>
    /// <remarks>
    /// <c>false</c> ise mum halen olusmaktadir ve degerleri sonraki guncellemelerde
    /// degisecektir; kapanmamis bir mumu nihai veri saymak yaniltici olur.
    /// </remarks>
    [JsonPropertyName("x")]
    public bool Closed { get; init; }
}
