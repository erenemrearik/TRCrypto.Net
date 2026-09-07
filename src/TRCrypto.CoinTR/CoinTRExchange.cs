using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.SharedApis;

namespace TRCrypto.CoinTR;

/// <summary>CoinTR borsasina ait sabit bilgiler ve sembol bicimlendirme kurallari.</summary>
public static class CoinTRExchange
{
    /// <summary>Borsa adi.</summary>
    public const string ExchangeName = "CoinTR";

    /// <summary>Kullaniciya gosterilecek ad.</summary>
    public const string DisplayName = "CoinTR";

    /// <summary>Borsanin ana web sitesi.</summary>
    public static string Url { get; } = "https://www.cointr.com";

    /// <summary>Resmi API dokumantasyonu.</summary>
    public static string[] ApiDocsUrl { get; } = ["https://cointr-ex.github.io/openapis/"];

    /// <summary>Platform ustverisi.</summary>
    public static PlatformInfo Metadata { get; } = new(
        ExchangeName,
        DisplayName,
        string.Empty,
        Url,
        ApiDocsUrl,
        PlatformType.CryptoCurrencyExchange,
        CentralizationType.Centralized,
        CoinTREnvironment.All);

    /// <summary>Istek limiti yapilandirmasi.</summary>
    public static CoinTRRateLimiters RateLimiter { get; set; } = new();

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
    /// Base ve quote varligi CoinTR'nin bekledigi native sembol adina cevirir.
    /// </summary>
    /// <remarks>
    /// CoinTR sembolleri <b>birlesik ve buyuk harf</b> yazar (<c>BTCTRY</c>), BtcTurk ile
    /// ayni bicimde. Binance TR alt cizgi kullanir (<c>BTC_TRY</c>), Paribu ise kucuk harf
    /// ve alt cizgi (<c>btc_tl</c>). Dort borsa icin tek bir bicimlendirme kullanilamaz.
    /// </remarks>
    /// <param name="baseAsset">Base varlik, ornegin <c>BTC</c>.</param>
    /// <param name="quoteAsset">Quote varlik, ornegin <c>TRY</c>.</param>
    /// <param name="tradingMode">Islem turu. CoinTR yalnizca spot destekler.</param>
    /// <param name="deliverTime">Vadeli islemler icin teslim tarihi; kullanilmaz.</param>
    /// <returns>Native sembol adi, ornegin <c>BTCTRY</c>.</returns>
    public static string FormatSymbol(
        string baseAsset,
        string quoteAsset,
        TradingMode tradingMode,
        DateTime? deliverTime = null)
        => NormalizeAsset(baseAsset) + NormalizeAsset(quoteAsset);
}

/// <summary>
/// CoinTR API'sinin istek limiti yapilandirmasi.
/// </summary>
/// <remarks>
/// Resmi dokumantasyon weight tabanli limitlerden soz eder ancak sayisal degerleri bu
/// envanterde dogrulanmamistir. Burada muhafazakar bir tavan uygulanir; degerler
/// dogrulandikca uc bazinda ayarlanacaktir.
/// </remarks>
public class CoinTRRateLimiters
{
    internal IRateLimitGate Rest { get; private set; }

    /// <summary>Bir istek limitine takildiginda tetiklenir.</summary>
    public event Action<RateLimitEvent>? RateLimitTriggered;

    /// <summary>Istek limiti kullanimi guncellendiginde tetiklenir.</summary>
    public event Action<RateLimitUpdateEvent>? RateLimitUpdated;

#pragma warning disable CS8618
    /// <summary>Yeni bir yapilandirma olusturur.</summary>
    public CoinTRRateLimiters()
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
