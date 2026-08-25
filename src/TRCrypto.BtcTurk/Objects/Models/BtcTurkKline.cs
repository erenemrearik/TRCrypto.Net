namespace TRCrypto.BtcTurk.Objects.Models;

/// <summary>Belirli bir zaman araligindaki fiyat hareketi (mum).</summary>
public record BtcTurkKline
{
    /// <summary>Araligin baslangic ani (UTC).</summary>
    public DateTime OpenTime { get; init; }

    /// <summary>Aralik acilis fiyati.</summary>
    public decimal OpenPrice { get; init; }

    /// <summary>Aralik icindeki en yuksek fiyat.</summary>
    public decimal HighPrice { get; init; }

    /// <summary>Aralik icindeki en dusuk fiyat.</summary>
    public decimal LowPrice { get; init; }

    /// <summary>Aralik kapanis fiyati.</summary>
    public decimal ClosePrice { get; init; }

    /// <summary>Aralik icindeki islem hacmi (base varlik cinsinden).</summary>
    public decimal Volume { get; init; }
}

/// <summary>
/// Kline ucunun ham yaniti.
/// </summary>
/// <remarks>
/// <para>
/// Bu uc <c>graph-api.btcturk.com</c> uzerinde calisir ve diger uclarin standart
/// zarfini (<c>success</c> / <c>code</c> / <c>data</c>) tasimaz.
/// </para>
/// <para>
/// Veriler paralel diziler halinde gelir: <c>t</c> dizisindeki her indeks, ayni
/// indeksteki <c>o</c>, <c>h</c>, <c>l</c>, <c>c</c> ve <c>v</c> degerlerine karsilik gelir.
/// Kullanilabilir mum listesi icin <see cref="ToKlines"/> cagrilir.
/// </para>
/// </remarks>
[SerializationModel]
public record BtcTurkKlineResponse
{
    /// <summary>["<c>s</c>"] Durum gostergesi; basarili yanitlarda <c>ok</c>.</summary>
    /// <remarks>Resmi dokumantasyonda gecmez ancak canli yanitta bulunur.</remarks>
    [JsonPropertyName("s")]
    public string? Status { get; init; }

    /// <summary>["<c>t</c>"] Zaman damgalari; <b>saniye</b> cinsinden.</summary>
    [JsonPropertyName("t")]
    public IReadOnlyList<long> Timestamps { get; init; } = [];

    /// <summary>["<c>o</c>"] Acilis fiyatlari.</summary>
    [JsonPropertyName("o")]
    public IReadOnlyList<decimal> OpenPrices { get; init; } = [];

    /// <summary>["<c>h</c>"] En yuksek fiyatlar.</summary>
    [JsonPropertyName("h")]
    public IReadOnlyList<decimal> HighPrices { get; init; } = [];

    /// <summary>["<c>l</c>"] En dusuk fiyatlar.</summary>
    [JsonPropertyName("l")]
    public IReadOnlyList<decimal> LowPrices { get; init; } = [];

    /// <summary>["<c>c</c>"] Kapanis fiyatlari.</summary>
    [JsonPropertyName("c")]
    public IReadOnlyList<decimal> ClosePrices { get; init; } = [];

    /// <summary>["<c>v</c>"] Hacimler.</summary>
    [JsonPropertyName("v")]
    public IReadOnlyList<decimal> Volumes { get; init; } = [];

    /// <summary>
    /// Paralel dizileri mum listesine donusturur.
    /// </summary>
    /// <returns>Zaman sirasini koruyan mum listesi.</returns>
    /// <exception cref="InvalidOperationException">
    /// Diziler ayni uzunlukta degilse firlatilir. Bu durumda hangi degerin hangi muma
    /// ait oldugu belirsizdir; eksik veriyi tahmin etmek yerine hata verilir.
    /// </exception>
    public IReadOnlyList<BtcTurkKline> ToKlines()
    {
        var count = Timestamps.Count;

        if (OpenPrices.Count != count
            || HighPrices.Count != count
            || LowPrices.Count != count
            || ClosePrices.Count != count
            || Volumes.Count != count)
        {
            throw new InvalidOperationException(
                "Kline yaniti bozuk: paralel dizilerin uzunluklari birbirini tutmuyor " +
                $"(t={count}, o={OpenPrices.Count}, h={HighPrices.Count}, " +
                $"l={LowPrices.Count}, c={ClosePrices.Count}, v={Volumes.Count}).");
        }

        var klines = new List<BtcTurkKline>(count);
        for (var i = 0; i < count; i++)
        {
            klines.Add(new BtcTurkKline
            {
                // Bu uc saniye kullanir; diger tum uclar milisaniye kullanir.
                OpenTime = DateTimeOffset.FromUnixTimeSeconds(Timestamps[i]).UtcDateTime,
                OpenPrice = OpenPrices[i],
                HighPrice = HighPrices[i],
                LowPrice = LowPrices[i],
                ClosePrice = ClosePrices[i],
                Volume = Volumes[i]
            });
        }

        return klines;
    }
}
