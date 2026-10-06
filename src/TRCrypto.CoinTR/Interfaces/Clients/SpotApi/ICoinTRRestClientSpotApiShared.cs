using CryptoExchange.Net.SharedApis;

namespace TRCrypto.CoinTR.Interfaces.Clients.SpotApi;

/// <summary>
/// CoinTR spot REST API'sinin borsadan bagimsiz (shared) yuzeyi.
/// </summary>
/// <remarks>
/// Bu surumde yalnizca herkese acik veri sunulur; bakiye ve emir arayuzleri kimlik
/// dogrulama canli olarak dogrulandiginda eklenecektir. Uygulanmamis bir arayuzu
/// bildirmek <c>Discover()</c> ciktisini yaniltici hale getirirdi.
/// </remarks>
public interface ICoinTRRestClientSpotApiShared :
    ISharedClient,
    ISpotSymbolRestClient,
    ISpotTickerRestClient,
    IOrderBookRestClient,
    IRecentTradeRestClient,
    IKlineRestClient
{
}
