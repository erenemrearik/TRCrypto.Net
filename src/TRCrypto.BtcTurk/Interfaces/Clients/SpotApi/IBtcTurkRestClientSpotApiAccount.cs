using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>
/// BtcTurk hesap uclari. Kimlik dogrulama gerektirir.
/// </summary>
/// <remarks>
/// Bu uclar icin API anahtarinizda <c>Toplam Varlik</c> izni acik olmalidir.
/// Anahtar alma rehberi: <c>docs/credentials/btcturk.md</c>.
/// </remarks>
public interface IBtcTurkRestClientSpotApiAccount
{
    /// <summary>
    /// Hesaptaki tum varlik bakiyelerini dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/private-endpoints/account-balance/" /></remarks>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Varlik bakiyeleri.</returns>
    Task<HttpResult<IReadOnlyList<BtcTurkBalance>>> GetBalancesAsync(CancellationToken ct = default);
}
