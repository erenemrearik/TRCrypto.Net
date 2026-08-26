using CryptoExchange.Net.Objects.Sockets;
using TRCrypto.BinanceTR.Enums;
using TRCrypto.BinanceTR.Objects.Models.Socket;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Binance TR spot WebSocket API'si.
/// </summary>
/// <remarks>
/// Bu akislar kimlik dogrulama gerektirmez. REST tarafinda anahtar isteyen ticker verisi
/// ile bos donen tekil islem ve mum verisi burada anahtarsiz alinabilir.
/// </remarks>
public interface IBinanceTRSocketClientSpotApi : ISocketApiClient<BinanceTRCredentials>, IDisposable
{
    /// <summary>Borsadan bagimsiz (shared) yuzey.</summary>
    IBinanceTRSocketClientSpotApiShared SharedClient { get; }

    /// <summary>Bir paritenin 24 saatlik ozet bilgisini dinler.</summary>
    /// <param name="symbol">Sembol; alt cizgili ya da alt cizgisiz verilebilir.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(
        string symbol,
        Action<DataEvent<BinanceTRStreamTicker>> onMessage,
        CancellationToken ct = default);

    /// <summary>Bir paritede gerceklesen islemleri dinler.</summary>
    /// <param name="symbol">Sembol.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(
        string symbol,
        Action<DataEvent<BinanceTRStreamTrade>> onMessage,
        CancellationToken ct = default);

    /// <summary>Bir paritede toplulastirilmis islemleri dinler.</summary>
    /// <param name="symbol">Sembol.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToAggregatedTradeUpdatesAsync(
        string symbol,
        Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage,
        CancellationToken ct = default);

    /// <summary>Bir paritenin emir defteri farklarini dinler.</summary>
    /// <remarks>
    /// Miktari sifir olan kademe silme anlamina gelir. Sira numaralarinda atlama
    /// gorulurse defter gecersiz sayilip yeni bir goruntu alinmalidir.
    /// </remarks>
    /// <param name="symbol">Sembol.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(
        string symbol,
        Action<DataEvent<BinanceTRStreamOrderBookUpdate>> onMessage,
        CancellationToken ct = default);

    /// <summary>Bir paritenin emir defteri goruntusunu dinler.</summary>
    /// <param name="symbol">Sembol.</param>
    /// <param name="levels">Kademe sayisi; borsa 5, 10 ve 20 degerlerini destekler.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(
        string symbol,
        int levels,
        Action<DataEvent<BinanceTRStreamOrderBook>> onMessage,
        CancellationToken ct = default);

    /// <summary>Bir paritenin mum verisini dinler.</summary>
    /// <remarks>
    /// Ilk mesaj icin mum araligina kadar beklemek gerekebilir; kisa sureli bir denemede
    /// akis calismiyor sanilabilir.
    /// </remarks>
    /// <param name="symbol">Sembol.</param>
    /// <param name="interval">Mum araligi.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik.</returns>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(
        string symbol,
        KlineInterval interval,
        Action<DataEvent<BinanceTRStreamKlineUpdate>> onMessage,
        CancellationToken ct = default);
}
