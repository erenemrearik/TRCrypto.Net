using CryptoExchange.Net.Objects.Sockets;
using TRCrypto.CoinTR.Objects.Models.Socket;

namespace TRCrypto.CoinTR.Interfaces.Clients.SpotApi;

/// <summary>
/// CoinTR spot WebSocket API'si.
/// </summary>
/// <remarks>Bu akislarin hicbiri kimlik dogrulama gerektirmez.</remarks>
public interface ICoinTRSocketClientSpotApi : ISocketApiClient<CoinTRCredentials>, IDisposable
{
    /// <summary>Borsadan bagimsiz (shared) yuzey.</summary>
    ICoinTRSocketClientSpotApiShared SharedClient { get; }

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
        Action<DataEvent<CoinTRStreamTicker>> onMessage,
        CancellationToken ct = default);

    /// <summary>
    /// Bir paritenin emir defterini dinler.
    /// </summary>
    /// <remarks>
    /// Kademe sayisi verildiginde borsa tam goruntu gonderir; verilmediginde yalnizca
    /// degisiklikler gelir ve defteri cagiran taraf birlestirmek zorundadir.
    /// </remarks>
    /// <param name="symbol">Native sembol adi.</param>
    /// <param name="levels">Kademe sayisi; <c>1</c>, <c>5</c> ya da <c>15</c>.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    /// <exception cref="ArgumentException">Sembol bos ya da kademe sayisi gecersizse firlatilir.</exception>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(
        string symbol,
        int levels,
        Action<DataEvent<CoinTRStreamOrderBook>> onMessage,
        CancellationToken ct = default);

    /// <summary>
    /// Bir paritedeki islemleri dinler.
    /// </summary>
    /// <param name="symbol">Native sembol adi.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(
        string symbol,
        Action<DataEvent<CoinTRStreamTrade>> onMessage,
        CancellationToken ct = default);
}
