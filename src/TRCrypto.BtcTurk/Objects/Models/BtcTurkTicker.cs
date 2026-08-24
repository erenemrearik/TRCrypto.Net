namespace TRCrypto.BtcTurk.Objects.Models;

/// <summary>Bir parite icin 24 saatlik ozet fiyat bilgisi.</summary>
[SerializationModel]
public record BtcTurkTicker
{
    /// <summary>["<c>pair</c>"] Native sembol adi, ornegin <c>BTCTRY</c>.</summary>
    [JsonPropertyName("pair")]
    public string Pair { get; init; } = string.Empty;

    /// <summary>["<c>pairNormalized</c>"] Normalize edilmis ad, ornegin <c>BTC_TRY</c>.</summary>
    [JsonPropertyName("pairNormalized")]
    public string PairNormalized { get; init; } = string.Empty;

    /// <summary>["<c>numeratorSymbol</c>"] Base varlik, ornegin <c>BTC</c>.</summary>
    /// <remarks>
    /// Bu ucta alan adi <c>numeratorSymbol</c> iken islem ucunda <c>numerator</c> olarak gecer;
    /// isimlendirme uclar arasinda tutarli degildir.
    /// </remarks>
    [JsonPropertyName("numeratorSymbol")]
    public string NumeratorSymbol { get; init; } = string.Empty;

    /// <summary>["<c>denominatorSymbol</c>"] Quote varlik, ornegin <c>TRY</c>.</summary>
    [JsonPropertyName("denominatorSymbol")]
    public string DenominatorSymbol { get; init; } = string.Empty;

    /// <summary>["<c>timestamp</c>"] Verinin uretildigi an (UTC).</summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>last</c>"] Son islem fiyati.</summary>
    [JsonPropertyName("last")]
    public decimal LastPrice { get; init; }

    /// <summary>["<c>high</c>"] Son 24 saatteki en yuksek fiyat.</summary>
    [JsonPropertyName("high")]
    public decimal HighPrice { get; init; }

    /// <summary>["<c>low</c>"] Son 24 saatteki en dusuk fiyat.</summary>
    [JsonPropertyName("low")]
    public decimal LowPrice { get; init; }

    /// <summary>["<c>bid</c>"] En iyi alis fiyati.</summary>
    [JsonPropertyName("bid")]
    public decimal BestBidPrice { get; init; }

    /// <summary>["<c>ask</c>"] En iyi satis fiyati.</summary>
    [JsonPropertyName("ask")]
    public decimal BestAskPrice { get; init; }

    /// <summary>["<c>open</c>"] 24 saat oncesindeki fiyat.</summary>
    [JsonPropertyName("open")]
    public decimal OpenPrice { get; init; }

    /// <summary>["<c>volume</c>"] Son 24 saatteki islem hacmi (base varlik cinsinden).</summary>
    [JsonPropertyName("volume")]
    public decimal Volume { get; init; }

    /// <summary>["<c>average</c>"] Son 24 saatteki ortalama fiyat.</summary>
    [JsonPropertyName("average")]
    public decimal AveragePrice { get; init; }

    /// <summary>["<c>daily</c>"] Son 24 saatteki fiyat degisimi (mutlak).</summary>
    [JsonPropertyName("daily")]
    public decimal DailyChange { get; init; }

    /// <summary>["<c>dailyPercent</c>"] Son 24 saatteki fiyat degisimi (yuzde).</summary>
    [JsonPropertyName("dailyPercent")]
    public decimal DailyChangePercentage { get; init; }

    /// <summary>["<c>order</c>"] Goruntuleme sirasi.</summary>
    [JsonPropertyName("order")]
    public int Order { get; init; }
}
