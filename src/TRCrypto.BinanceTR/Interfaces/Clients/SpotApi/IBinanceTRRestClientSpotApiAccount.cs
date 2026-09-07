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

    /// <summary>
    /// Tek bir varligin bakiyesini getirir.
    /// </summary>
    /// <remarks>
    /// Hesap ucu zaten tum varliklari dondurur; bu uc yalniz tek varlik izlenirken
    /// aktarilan veriyi kucultur.
    /// </remarks>
    /// <param name="asset">Varlik adi, ornegin <c>TRY</c>.</param>
    /// <param name="receiveWindow">Istegin gecerli sayilacagi sure (ms).</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Varligin bakiyesi.</returns>
    /// <exception cref="ArgumentException">Varlik adi bos ise firlatilir.</exception>
    Task<HttpResult<BinanceTRAccountAsset>> GetAssetAsync(
        string asset,
        long? receiveWindow = null,
        CancellationToken ct = default);

    /// <summary>
    /// Kullanici akisina abone olmak icin dinleme tokeni uretir.
    /// </summary>
    /// <remarks>
    /// Token kendiliginden yenilenmez. Suresi dolmadan yenisi alinip yeniden abone
    /// olunmalidir; aksi halde akis hata vermeden durur.
    /// </remarks>
    /// <param name="validity">Tokenin gecerli kalacagi sure (ms); en fazla 24 saat.</param>
    /// <param name="receiveWindow">Istegin gecerli sayilacagi sure (ms).</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Token ve gecerlilik bitis ani.</returns>
    Task<HttpResult<BinanceTRListenToken>> GetListenTokenAsync(
        long? validity = null,
        long? receiveWindow = null,
        CancellationToken ct = default);
}
