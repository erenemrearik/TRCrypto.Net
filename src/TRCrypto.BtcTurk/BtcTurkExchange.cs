using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BtcTurk;

/// <summary>BtcTurk borsasina ait sabit bilgiler ve sembol bicimlendirme kurallari.</summary>
public static class BtcTurkExchange
{
    /// <summary>Borsa adi.</summary>
    public const string ExchangeName = "BtcTurk";

    /// <summary>Kullaniciya gosterilecek ad.</summary>
    public const string DisplayName = "BtcTurk";

    /// <summary>Borsanin ana web sitesi.</summary>
    public static string Url { get; } = "https://www.btcturk.com";

    /// <summary>Resmi API dokumantasyonu.</summary>
    public static string[] ApiDocsUrl { get; } = ["https://docs.btcturk.com/"];

    /// <summary>Platform ustverisi.</summary>
    public static PlatformInfo Metadata { get; } = new(
        ExchangeName,
        DisplayName,
        string.Empty,
        "https://www.btcturk.com",
        ["https://docs.btcturk.com/"],
        PlatformType.CryptoCurrencyExchange,
        CentralizationType.Centralized,
        BtcTurkEnvironment.All);

    /// <summary>Istek limiti yapilandirmasi.</summary>
    public static BtcTurkRateLimiters RateLimiter { get; set; } = new();

    /// <summary>Istek parametrelerinin nasil serilestirilecegini belirler.</summary>
    /// <remarks>Ondalik degerler metin olarak gonderilir; boylece hassasiyet kaybi olmaz.</remarks>
    internal static ParameterSerializationSettings ParameterSettings { get; } = new()
    {
        Decimal = DecimalSerialization.String
    };

    /// <summary>
    /// Varlik adi takma adlari.
    /// </summary>
    /// <remarks>
    /// Turkiye kaynaklarinda Turk Lirasi icin zaman zaman <c>TL</c> kullanilir; BtcTurk ise
    /// her zaman <c>TRY</c> bekler. Bu esleme tek bir yerde tutulur. Sembol adlari kod icinde
    /// dagitik <c>Replace</c> cagrilariyla duzeltilmez (spesifikasyon Bolum 7.2, RISK-05).
    /// </remarks>
    internal static IReadOnlyDictionary<string, string> AssetAliases { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["TL"] = "TRY"
        };

    /// <summary>
    /// Bir varlik adini BtcTurk'un kullandigi kanonik ada cevirir.
    /// </summary>
    /// <param name="asset">Varlik adi; buyuk/kucuk harf duyarsizdir.</param>
    /// <returns>Kanonik varlik adi, ornegin <c>TL</c> icin <c>TRY</c>.</returns>
    public static string NormalizeAsset(string asset)
    {
        if (string.IsNullOrEmpty(asset))
            return string.Empty;

        var upper = asset.ToUpperInvariant();
        return AssetAliases.TryGetValue(upper, out var canonical) ? canonical : upper;
    }

    /// <summary>
    /// Base ve quote varligi BtcTurk'un bekledigi native sembol adina cevirir.
    /// </summary>
    /// <param name="baseAsset">Base varlik, ornegin <c>BTC</c>.</param>
    /// <param name="quoteAsset">Quote varlik, ornegin <c>TRY</c>.</param>
    /// <param name="tradingMode">Islem turu. BtcTurk yalnizca spot destekler.</param>
    /// <param name="deliverTime">Vadeli islemler icin teslim tarihi; BtcTurk'te kullanilmaz.</param>
    /// <returns>Native sembol adi, ornegin <c>BTCTRY</c>.</returns>
    public static string FormatSymbol(
        string baseAsset,
        string quoteAsset,
        TradingMode tradingMode,
        DateTime? deliverTime = null)
        => NormalizeAsset(baseAsset) + NormalizeAsset(quoteAsset);
}
