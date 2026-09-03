using TRCrypto.BinanceTR.Objects.Models;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Hesap bilgisi uclari.
/// </summary>
/// <remarks>
/// Tum uclar API anahtari ister ve imzalanir. Anahtarin okuma izni yoksa borsa
/// <c>3701</c> koduyla yanit verir; bu kod yanlis anahtar, listede olmayan IP ve
/// eksik izin durumlarinin ucunu birden kapsar.
/// </remarks>
public interface IBinanceTRRestClientSpotApiAccount
{
    /// <summary>
    /// Spot hesabin izinlerini, komisyon oranlarini ve varlik bakiyelerini getirir.
    /// </summary>
    /// <param name="receiveWindow">
    /// Istegin gecerli sayilacagi sure (ms). Verilmezse borsa 5000 kullanir; en fazla 60000.
    /// </param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Hesap bilgisi.</returns>
    Task<HttpResult<BinanceTRAccount>> GetAccountAsync(
        long? receiveWindow = null,
        CancellationToken ct = default);
}
