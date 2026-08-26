using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Binance TR spot REST API'sinin borsadan bagimsiz (shared) yuzeyi.
/// </summary>
/// <remarks>
/// <para>
/// <c>ISpotTickerRestClient</c> bilincli olarak uygulanmamistir: borsa ticker verisini
/// anahtarsiz sunmaz. Gercek zamanli ticker icin socket yuzeyi kullanilmalidir.
/// </para>
/// <para>
/// Islem verisi, borsanin ayrintili islem ucu bos dondugu icin toplulastirilmis
/// islemlerden turetilir.
/// </para>
/// </remarks>
public interface IBinanceTRRestClientSpotApiShared :
    ISharedClient,
    ISpotSymbolRestClient,
    IOrderBookRestClient,
    IRecentTradeRestClient
{
}
