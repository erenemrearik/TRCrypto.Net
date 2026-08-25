using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>
/// BtcTurk spot API'sinin borsadan bagimsiz (shared) yuzeyi.
/// </summary>
/// <remarks>
/// Bu arayuzler sayesinde ayni kod farkli borsalarla calisabilir. Borsaya ozgu alanlara
/// ihtiyac duyuldugunda native API kullanilmalidir.
/// <para>
/// Piyasa verisi arayuzleri kimlik dogrulama gerektirmez. Bakiye arayuzu icin API
/// anahtarinizda <c>Toplam Varlik</c> izni acik olmalidir. Emir arayuzleri henuz uygulanmadi.
/// </para>
/// </remarks>
public interface IBtcTurkRestClientSpotApiShared :
    ISharedClient,
    ISpotSymbolRestClient,
    IBalanceRestClient,
    ISpotTickerRestClient,
    IOrderBookRestClient,
    IRecentTradeRestClient
{
}
