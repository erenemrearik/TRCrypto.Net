using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>BtcTurk spot piyasa verisi uclari. Kimlik dogrulama gerektirmez.</summary>
public interface IBtcTurkRestClientSpotApiExchangeData
{
    /// <summary>
    /// Borsanin destekledigi pariteleri, varliklari ve sunucu saatini dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/public-endpoints/exchange-info/" /></remarks>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Borsa bilgisi.</returns>
    Task<HttpResult<BtcTurkExchangeInfo>> GetExchangeInfoAsync(CancellationToken ct = default);

    /// <summary>
    /// Sunucu saatini dondurur.
    /// </summary>
    /// <remarks>
    /// BtcTurk ayri bir sunucu saati ucu sunmaz; deger exchange info yanitindan alinir.
    /// </remarks>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Sunucu saati (UTC).</returns>
    Task<HttpResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default);
}
