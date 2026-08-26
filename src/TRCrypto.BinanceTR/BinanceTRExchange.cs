using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BinanceTR;

/// <summary>Binance TR borsasina ait sabit bilgiler ve sembol bicimlendirme kurallari.</summary>
public static class BinanceTRExchange
{
    /// <summary>Borsa adi.</summary>
    public const string ExchangeName = "BinanceTR";

    /// <summary>Kullaniciya gosterilecek ad.</summary>
    public const string DisplayName = "Binance TR";

    /// <summary>Borsanin ana web sitesi.</summary>
    public static string Url { get; } = "https://www.binance.tr";

    /// <summary>Resmi API dokumantasyonu.</summary>
    public static string[] ApiDocsUrl { get; } = ["https://www.binance.tr/apidocs"];

    /// <summary>Platform ustverisi.</summary>
    public static PlatformInfo Metadata { get; } = new(
        ExchangeName,
        DisplayName,
        string.Empty,
        Url,
        ApiDocsUrl,
        PlatformType.CryptoCurrencyExchange,
        CentralizationType.Centralized,
        BinanceTREnvironment.All);

    /// <summary>Istek limiti yapilandirmasi.</summary>
    public static BinanceTRRateLimiters RateLimiter { get; set; } = new();

    /// <summary>Istek parametrelerinin nasil serilestirilecegini belirler.</summary>
    internal static ParameterSerializationSettings ParameterSettings { get; } = new()
    {
        Decimal = DecimalSerialization.String
    };

    /// <summary>
    /// Varlik adi takma adlari.
    /// </summary>
    /// <remarks>
    /// Turkiye kaynaklarinda Turk Lirasi icin zaman zaman <c>TL</c> kullanilir; borsa ise
    /// her zaman <c>TRY</c> bekler.
    /// </remarks>
    internal static IReadOnlyDictionary<string, string> AssetAliases { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["TL"] = "TRY"
        };

    /// <summary>Bir varlik adini borsanin kullandigi kanonik ada cevirir.</summary>
    /// <param name="asset">Varlik adi; buyuk/kucuk harf duyarsizdir.</param>
    /// <returns>Kanonik varlik adi.</returns>
    public static string NormalizeAsset(string asset)
    {
        if (string.IsNullOrEmpty(asset))
            return string.Empty;

        var upper = asset.ToUpperInvariant();
        return AssetAliases.TryGetValue(upper, out var canonical) ? canonical : upper;
    }

    /// <summary>
    /// Base ve quote varligi Binance TR'nin bekledigi native sembol adina cevirir.
    /// </summary>
    /// <remarks>
    /// Binance TR sembollerinde <b>alt cizgi</b> kullanir (<c>BTC_TRY</c>); BtcTurk ise
    /// birlesik yazar (<c>BTCTRY</c>). Iki borsa icin tek bir bicimlendirme kullanilamaz.
    /// </remarks>
    /// <param name="baseAsset">Base varlik, ornegin <c>BTC</c>.</param>
    /// <param name="quoteAsset">Quote varlik, ornegin <c>TRY</c>.</param>
    /// <param name="tradingMode">Islem turu. Binance TR yalnizca spot destekler.</param>
    /// <param name="deliverTime">Vadeli islemler icin teslim tarihi; kullanilmaz.</param>
    /// <returns>Native sembol adi, ornegin <c>BTC_TRY</c>.</returns>
    public static string FormatSymbol(
        string baseAsset,
        string quoteAsset,
        TradingMode tradingMode,
        DateTime? deliverTime = null)
        => NormalizeAsset(baseAsset) + "_" + NormalizeAsset(quoteAsset);
}

/// <summary>
/// Binance TR API'sinin istek limiti yapilandirmasi.
/// </summary>
/// <remarks>
/// Resmi dokumantasyon weight tabanli limitlerden soz eder ancak sayisal degerleri bu
/// envanterde dogrulanmamistir. Burada muhafazakar bir tavan uygulanir; degerler
/// dogrulandikca uc bazinda ayarlanacaktir.
/// </remarks>
public class BinanceTRRateLimiters
{
    internal IRateLimitGate Rest { get; private set; }

    /// <summary>Bir istek limitine takildiginda tetiklenir.</summary>
    public event Action<RateLimitEvent>? RateLimitTriggered;

    /// <summary>Istek limiti kullanimi guncellendiginde tetiklenir.</summary>
    public event Action<RateLimitUpdateEvent>? RateLimitUpdated;

#pragma warning disable CS8618
    /// <summary>Yeni bir yapilandirma olusturur.</summary>
    public BinanceTRRateLimiters()
#pragma warning restore CS8618
    {
        Initialize();
    }

    private void Initialize()
    {
        Rest = new RateLimitGate("Rest")
            .AddGuard(new RateLimitGuard(
                RateLimitGuard.PerHost,
                new IGuardFilter[] { new AuthenticatedEndpointFilter(false) },
                1200,
                TimeSpan.FromMinutes(1),
                RateLimitWindowType.Sliding));

        Rest.RateLimitTriggered += x => RateLimitTriggered?.Invoke(x);
        Rest.RateLimitUpdated += x => RateLimitUpdated?.Invoke(x);
    }
}
