using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace TRCrypto.BtcTurk;

/// <summary>
/// BtcTurk API'sinin istek limiti yapilandirmasi.
/// </summary>
/// <remarks>
/// Degerler resmi dokumantasyondan alinmistir
/// (<see href="https://docs.btcturk.com/docs/private-endpoints/rate-limits/" />, 24 Agustos 2026).
/// Limitler IP bazlidir ve uc bazinda degisir; burada en kisitlayici public limit
/// (order book: 60 saniyede 180 istek) genel bir tavan olarak uygulanir.
/// Uc bazli ince ayar, ilgili uclar eklendikce yapilacaktir.
/// </remarks>
public class BtcTurkRateLimiters
{
    internal IRateLimitGate PublicRest { get; private set; }

    /// <summary>Bir istek limitine takildiginda tetiklenir.</summary>
    public event Action<RateLimitEvent>? RateLimitTriggered;

    /// <summary>Istek limiti kullanimi guncellendiginde tetiklenir.</summary>
    public event Action<RateLimitUpdateEvent>? RateLimitUpdated;

#pragma warning disable CS8618
    /// <summary>Yeni bir yapilandirma olusturur.</summary>
    public BtcTurkRateLimiters()
#pragma warning restore CS8618
    {
        Initialize();
    }

    private void Initialize()
    {
        PublicRest = new RateLimitGate("Public Rest")
            .AddGuard(new RateLimitGuard(
                RateLimitGuard.PerHost,
                new IGuardFilter[] { new AuthenticatedEndpointFilter(false) },
                180,
                TimeSpan.FromSeconds(60),
                RateLimitWindowType.Sliding));

        PublicRest.RateLimitTriggered += x => RateLimitTriggered?.Invoke(x);
        PublicRest.RateLimitUpdated += x => RateLimitUpdated?.Invoke(x);
    }
}
