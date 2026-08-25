using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Enums;
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

    /// <summary>
    /// Hesaba ait gerceklesmis islemleri dondurur.
    /// </summary>
    /// <remarks>
    /// <para><see href="https://docs.btcturk.com/docs/private-endpoints/user-transactions/" /></para>
    /// <para>Tarih araligi verilmezse borsa son 30 gunu dondurur.</para>
    /// <para>Tutarlar isaretlidir: satista miktar, komisyon ve vergi negatif gelir.</para>
    /// </remarks>
    /// <param name="symbol">Native sembol adi, ornegin <c>BTCTRY</c>.</param>
    /// <param name="side">Islem yonu filtresi.</param>
    /// <param name="startTime">Baslangic zamani.</param>
    /// <param name="endTime">Bitis zamani.</param>
    /// <param name="orderId">Tek bir emre ait islemler. Diger filtrelerle birlikte kullanilamaz.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Gerceklesmis islemler.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="orderId"/> baska bir filtreyle birlikte verilirse firlatilir.
    /// </exception>
    Task<HttpResult<IReadOnlyList<BtcTurkUserTrade>>> GetUserTradesAsync(
        string? symbol = null,
        OrderSide? side = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        long? orderId = null,
        CancellationToken ct = default);
}
