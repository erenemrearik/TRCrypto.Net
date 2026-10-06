using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// REST paylasilan yuzeyinin ince taneli (V2) gorunumu.
/// </summary>
/// <remarks>
/// <para>
/// CryptoExchange.Net 13 her islemi ayri bir arayuze boldu (<c>IGetOrderBookRest</c>,
/// <c>ISubscribeTickerSocket</c> gibi). Bu arayuz o yetenekleri toplar.
/// </para>
/// <para>
/// <see cref="IBinanceTRRestClientSpotApiShared"/> ile ayni nesnedir ama ondan ayri tutulur. V1 ve V2 ayni adli
/// uyeler tasir (<c>GetOrderBookOptions</c>, <c>GetOrderBookAsync</c>); ikisini tek bir
/// arayuzde birlestirmek, istemciye dogrudan erisen kullanici kodunda belirsizlik
/// hatasina yol acar ve mevcut kodu kirar.
/// </para>
/// </remarks>
public interface IBinanceTRRestClientSpotSharedApi :
    ISharedApi,
    IGetSpotSymbolsRest,
    IGetOrderBookRest,
    IGetRecentTradesRest,
    IGetBalancesRest,
    IPlaceSpotOrderRest,
    IGetSpotOrderRest,
    IGetOpenSpotOrdersRest,
    IGetClosedSpotOrdersRest,
    ICancelSpotOrderRest,
    IGetSpotOrderTradesRest,
    IGetSpotUserTradeHistoryRest
{
}
