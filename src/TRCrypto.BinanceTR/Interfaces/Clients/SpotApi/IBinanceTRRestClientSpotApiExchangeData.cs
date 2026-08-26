using TRCrypto.BinanceTR.Objects.Models;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Binance TR spot piyasa verisi uclari.
/// </summary>
/// <remarks>
/// Bu uclar kimlik dogrulama gerektirmez. Ancak borsanin <c>/api/v3/*</c> yollari
/// (global Binance'te herkese acik olanlar) burada anahtar ister; bu nedenle ticker
/// verisi anahtarsiz alinamaz.
/// </remarks>
public interface IBinanceTRRestClientSpotApiExchangeData
{
    /// <summary>
    /// Sunucu saatini dondurur.
    /// </summary>
    /// <remarks>Deger yanit zarfindaki zaman damgasindan okunur.</remarks>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Sunucu saati (UTC).</returns>
    Task<HttpResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default);

    /// <summary>
    /// Borsanin destekledigi pariteleri dondurur.
    /// </summary>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Parite listesi.</returns>
    Task<HttpResult<BinanceTRExchangeInfo>> GetSymbolsAsync(CancellationToken ct = default);

    /// <summary>
    /// Bir parite icin emir defteri goruntusunu dondurur.
    /// </summary>
    /// <param name="symbol">Native sembol adi, ornegin <c>BTC_TRY</c>.</param>
    /// <param name="limit">
    /// Kademe sayisi. Borsa yalnizca 5, 10, 20, 50, 100, 500 ve 1000 degerlerini kabul eder;
    /// diger degerler yaniltici bir "Incorrect Page number" hatasiyla reddedilir.
    /// </param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Emir defteri.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="limit"/> desteklenen degerlerden biri degilse firlatilir.
    /// </exception>
    Task<HttpResult<BinanceTROrderBook>> GetOrderBookAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default);

    /// <summary>
    /// Bir parite icin toplulastirilmis islemleri dondurur.
    /// </summary>
    /// <remarks>
    /// Borsanin ayrintili islem ucu (<c>market/trades</c>) su an bos liste dondurmektedir;
    /// islem verisi icin bu uc kullanilmalidir.
    /// </remarks>
    /// <param name="symbol">Native sembol adi.</param>
    /// <param name="limit">Dondurulecek kayit sayisi.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Toplulastirilmis islemler.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<HttpResult<BinanceTRAggregatedTradeList>> GetAggregatedTradesAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default);
}
