using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <summary>
/// CryptoExchange.Net 13 ile gelen ince taneli (V2) paylasilan API arayuzleri.
/// </summary>
/// <remarks>
/// <para>
/// Her V2 uyesi ayni sinifin V1 uygulamasina delege eder; davranis tek yerde durur.
/// Secenek ozellikleri V1 ile ayni nesneyi dondurur, bu yuzden dogrulama kurallari
/// ve yetenek listesi tek bir yapilandirmayi paylasir.
/// </para>
/// <para>
/// V2 arayuzleri istege bagli degildir: 13 surumunde istek dogrulamasi
/// (<c>ValidateRequest</c>) yalnizca V2 yetenek arayuzunu kabul eder. Bunlar
/// uygulanmadan borsaya ozgu sinirlar (kademe sayisi, aralik gibi) denetlenemezdi.
/// </para>
/// </remarks>
internal partial class BtcTurkSocketClientSpotApi : IBtcTurkSocketClientSpotSharedApi
{
    #region ISubscribeTickerSocket

    SubscribeTickerOptions ISubscribeTickerSocket.SubscribeTickerOptions => ((ITickerSocketClient)this).SubscribeTickerOptions;

    Task<WebSocketResult<UpdateSubscription>> ISubscribeTickerSocket.SubscribeToTickerUpdatesAsync(
        SubscribeTickerRequest request,
        Action<DataEvent<SharedTicker>> handler,
        CancellationToken ct)
        => ((ITickerSocketClient)this).SubscribeToTickerUpdatesAsync(request, x => handler(x.ToType<SharedTicker>(x.Data)), ct);

    #endregion

    #region ISubscribeAllTickersSocket

    SubscribeTickersOptions ISubscribeAllTickersSocket.SubscribeAllTickersOptions => ((ITickersSocketClient)this).SubscribeAllTickersOptions;

    Task<WebSocketResult<UpdateSubscription>> ISubscribeAllTickersSocket.SubscribeToAllTickersUpdatesAsync(
        SubscribeAllTickersRequest request,
        Action<DataEvent<SharedTicker[]>> handler,
        CancellationToken ct)
        => ((ITickersSocketClient)this).SubscribeToAllTickersUpdatesAsync(request, x => handler(x.ToType<SharedTicker[]>(x.Data)), ct);

    #endregion

    #region ISubscribeTradesSocket

    SubscribeTradeOptions ISubscribeTradesSocket.SubscribeTradeOptions => ((ITradeSocketClient)this).SubscribeTradeOptions;

    Task<WebSocketResult<UpdateSubscription>> ISubscribeTradesSocket.SubscribeToTradeUpdatesAsync(
        SubscribeTradeRequest request,
        Action<DataEvent<SharedTrade[]>> handler,
        CancellationToken ct)
        => ((ITradeSocketClient)this).SubscribeToTradeUpdatesAsync(request, handler, ct);

    #endregion

    #region ISubscribeOrderBookSocket

    SubscribeOrderBookOptions ISubscribeOrderBookSocket.SubscribeOrderBookOptions => ((IOrderBookSocketClient)this).SubscribeOrderBookOptions;

    Task<WebSocketResult<UpdateSubscription>> ISubscribeOrderBookSocket.SubscribeToOrderBookUpdatesAsync(
        SubscribeOrderBookRequest request,
        Action<DataEvent<SharedOrderBook>> handler,
        CancellationToken ct)
        => ((IOrderBookSocketClient)this).SubscribeToOrderBookUpdatesAsync(request, handler, ct);

    #endregion
}
