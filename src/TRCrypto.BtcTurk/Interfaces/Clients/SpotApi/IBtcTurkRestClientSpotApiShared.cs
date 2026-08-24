using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>
/// BtcTurk spot API'sinin borsadan bagimsiz (shared) yuzeyi.
/// </summary>
/// <remarks>
/// Bu arayuzler sayesinde ayni kod farkli borsalarla calisabilir. Borsaya ozgu alanlara
/// ihtiyac duyuldugunda native API kullanilmalidir.
/// <para>
/// Bu surumde yalnizca kimlik dogrulama gerektirmeyen piyasa verisi arayuzleri uygulanmistir.
/// Bakiye ve emir arayuzleri imzalama eklendiginde gelecektir.
/// </para>
/// </remarks>
public interface IBtcTurkRestClientSpotApiShared :
    ISharedClient,
    ISpotSymbolRestClient,
    ISpotTickerRestClient,
    IOrderBookRestClient,
    IRecentTradeRestClient
{
}
