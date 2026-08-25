using TRCrypto.BtcTurk.Converters;

namespace TRCrypto.BtcTurk.Objects.Models.Socket;

/// <summary>
/// WebSocket ticker guncellemesi.
/// </summary>
/// <remarks>
/// Alan adlari kaynakta tek/iki harfe kisaltilmistir ve degerler metin olarak gelir;
/// ayni veriyi REST ticker ucu acik adlarla ve sayi olarak dondurur.
/// </remarks>
[SerializationModel]
public record BtcTurkSocketTicker
{
    /// <summary>["<c>PS</c>"] Native sembol adi, ornegin <c>BTCTRY</c>.</summary>
    [JsonPropertyName("PS")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>["<c>NS</c>"] Base varlik.</summary>
    [JsonPropertyName("NS")]
    public string NumeratorSymbol { get; init; } = string.Empty;

    /// <summary>["<c>DS</c>"] Quote varlik.</summary>
    [JsonPropertyName("DS")]
    public string DenominatorSymbol { get; init; } = string.Empty;

    /// <summary>["<c>PId</c>"] Parite kimligi.</summary>
    [JsonPropertyName("PId")]
    public int PairId { get; init; }

    /// <summary>["<c>B</c>"] En iyi alis fiyati.</summary>
    [JsonPropertyName("B")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal BestBidPrice { get; init; }

    /// <summary>["<c>A</c>"] En iyi satis fiyati.</summary>
    [JsonPropertyName("A")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal BestAskPrice { get; init; }

    /// <summary>["<c>BA</c>"] En iyi alis miktari.</summary>
    [JsonPropertyName("BA")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal BestBidQuantity { get; init; }

    /// <summary>["<c>AA</c>"] En iyi satis miktari.</summary>
    [JsonPropertyName("AA")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal BestAskQuantity { get; init; }

    /// <summary>["<c>LA</c>"] Son islem fiyati.</summary>
    [JsonPropertyName("LA")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal LastPrice { get; init; }

    /// <summary>["<c>H</c>"] Son 24 saatteki en yuksek fiyat.</summary>
    [JsonPropertyName("H")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal HighPrice { get; init; }

    /// <summary>["<c>L</c>"] Son 24 saatteki en dusuk fiyat.</summary>
    [JsonPropertyName("L")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal LowPrice { get; init; }

    /// <summary>["<c>O</c>"] 24 saat oncesindeki fiyat.</summary>
    [JsonPropertyName("O")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal OpenPrice { get; init; }

    /// <summary>["<c>V</c>"] Son 24 saatteki islem hacmi.</summary>
    [JsonPropertyName("V")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Volume { get; init; }

    /// <summary>["<c>AV</c>"] Son 24 saatteki ortalama fiyat.</summary>
    [JsonPropertyName("AV")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal AveragePrice { get; init; }

    /// <summary>["<c>D</c>"] Son 24 saatteki fiyat degisimi (mutlak).</summary>
    [JsonPropertyName("D")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal DailyChange { get; init; }

    /// <summary>["<c>DP</c>"] Son 24 saatteki fiyat degisimi (yuzde).</summary>
    [JsonPropertyName("DP")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal DailyChangePercentage { get; init; }
}

/// <summary>WebSocket islem guncellemesi; bir ya da daha fazla islem tasir.</summary>
[SerializationModel]
public record BtcTurkSocketTradeUpdate
{
    /// <summary>["<c>symbol</c>"] Native sembol adi.</summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>["<c>items</c>"] Gerceklesmis islemler.</summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<BtcTurkSocketTrade> Trades { get; init; } = [];
}

/// <summary>WebSocket akisindan gelen tek bir islem.</summary>
[SerializationModel]
public record BtcTurkSocketTrade
{
    /// <summary>["<c>I</c>"] Islem kimligi.</summary>
    [JsonPropertyName("I")]
    public string Id { get; init; } = string.Empty;

    /// <summary>["<c>D</c>"] Islemin gerceklestigi an (UTC).</summary>
    /// <remarks>Kaynakta metin icinde milisaniye olarak gelir.</remarks>
    [JsonPropertyName("D")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>A</c>"] Islem miktari.</summary>
    [JsonPropertyName("A")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Quantity { get; init; }

    /// <summary>["<c>P</c>"] Islem fiyati.</summary>
    [JsonPropertyName("P")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Price { get; init; }

    /// <summary>["<c>S</c>"] Islemin yonu.</summary>
    /// <remarks>
    /// Kaynakta sayisal koddur (<c>0</c> satis, <c>1</c> alis). Bu esleme resmi
    /// dokumantasyonda yer almaz; canli akistaki islem kimlikleri REST yanitiyla
    /// karsilastirilarak belirlenmistir.
    /// </remarks>
    [JsonPropertyName("S")]
    [JsonConverter(typeof(BtcTurkSocketOrderSideConverter))]
    public OrderSide Side { get; init; }
}

/// <summary>WebSocket emir defteri goruntusu.</summary>
[SerializationModel]
public record BtcTurkSocketOrderBook
{
    /// <summary>["<c>PS</c>"] Native sembol adi.</summary>
    [JsonPropertyName("PS")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// ["<c>CS</c>"] Sira numarasi.
    /// </summary>
    /// <remarks>
    /// Fark mesajlarinin sirasini dogrulamak icin kullanilir. Atlama tespit edilirse
    /// defter gecersiz sayilip yeni bir tam goruntu alinmalidir.
    /// </remarks>
    [JsonPropertyName("CS")]
    public long Sequence { get; init; }

    /// <summary>["<c>AO</c>"] Satis kademeleri.</summary>
    [JsonPropertyName("AO")]
    public IReadOnlyList<BtcTurkSocketOrderBookEntry> Asks { get; init; } = [];

    /// <summary>["<c>BO</c>"] Alis kademeleri.</summary>
    [JsonPropertyName("BO")]
    public IReadOnlyList<BtcTurkSocketOrderBookEntry> Bids { get; init; } = [];
}

/// <summary>WebSocket emir defterinde tek bir fiyat kademesi.</summary>
[SerializationModel]
public record BtcTurkSocketOrderBookEntry : ISymbolOrderBookEntry
{
    /// <summary>["<c>P</c>"] Kademe fiyati.</summary>
    [JsonPropertyName("P")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Price { get; init; }

    /// <summary>["<c>A</c>"] Bu fiyattaki toplam miktar.</summary>
    [JsonPropertyName("A")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
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
