using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

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
internal partial class BinanceTRSocketClientUserApi : IBinanceTRSocketClientUserSharedApi
{
    #region ISubscribeBalancesSocket

    SubscribeBalanceOptions ISubscribeBalancesSocket.SubscribeBalanceOptions => ((IBalanceSocketClient)this).SubscribeBalanceOptions;

    Task<WebSocketResult<UpdateSubscription>> ISubscribeBalancesSocket.SubscribeToBalanceUpdatesAsync(
        SubscribeBalancesRequest request,
        Action<DataEvent<SharedBalance[]>> handler,
        CancellationToken ct)
        => ((IBalanceSocketClient)this).SubscribeToBalanceUpdatesAsync(request, handler, ct);

    #endregion
}
