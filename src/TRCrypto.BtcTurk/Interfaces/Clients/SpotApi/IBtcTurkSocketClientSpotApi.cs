using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using TRCrypto.BtcTurk.Objects.Models.Socket;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>
/// BtcTurk spot WebSocket API'si.
/// </summary>
/// <remarks>
/// Bu akislar kimlik dogrulama gerektirmez. Baglanti koptugunda kutuphane yeniden
/// baglanir ve acik abonelikleri kendiliginden yeniden kurar.
/// </remarks>
public interface IBtcTurkSocketClientSpotApi : ISocketApiClient<BtcTurkCredentials>, IDisposable
{
    /// <summary>
    /// Borsadan bagimsiz (shared) yuzey.
    /// </summary>
    IBtcTurkSocketClientSpotApiShared SharedClient { get; }

    /// <summary>
    /// Borsadaki tum paritelerin ozet fiyat bilgisini tek abonelikle dinler.
    /// </summary>
    /// <remarks>
    /// Her guncelleme borsadaki paritelerin tamamini tasir. Tek bir pariteyi izlemek
    /// icin <see cref="SubscribeToTickerUpdatesAsync"/> daha az veri aktarir.
    /// </remarks>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik; kapatmak icin <c>CloseAsync</c> cagrilir.</returns>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToAllTickerUpdatesAsync(
        Action<DataEvent<BtcTurkSocketTickerList>> onMessage,
        CancellationToken ct = default);

    /// <summary>
    /// Bir paritenin ozet fiyat bilgisindeki degisiklikleri dinler.
    /// </summary>
    /// <param name="symbol">Native sembol adi, ornegin <c>BTCTRY</c>.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik; kapatmak icin <c>CloseAsync</c> cagrilir.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(
        string symbol,
        Action<DataEvent<BtcTurkSocketTicker>> onMessage,
        CancellationToken ct = default);

    /// <summary>
    /// Bir paritede gerceklesen islemleri dinler.
    /// </summary>
    /// <remarks>Her mesaj bir ya da daha fazla islem tasiyabilir.</remarks>
    /// <param name="symbol">Native sembol adi.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(
        string symbol,
        Action<DataEvent<BtcTurkSocketTradeUpdate>> onMessage,
        CancellationToken ct = default);

    /// <summary>
    /// Bir paritenin emir defterini dinler.
    /// </summary>
    /// <remarks>
    /// Gelen mesajlar bir sira numarasi tasir. Numarada atlama gorulurse defter
    /// gecersiz sayilmali ve yeni bir tam goruntu alinmalidir.
    /// </remarks>
    /// <param name="symbol">Native sembol adi.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(
        string symbol,
        Action<DataEvent<BtcTurkSocketOrderBook>> onMessage,
        CancellationToken ct = default);
}
