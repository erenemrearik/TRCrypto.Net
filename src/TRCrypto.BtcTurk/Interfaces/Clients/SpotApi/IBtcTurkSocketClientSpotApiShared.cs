using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>
/// BtcTurk spot WebSocket API'sinin borsadan bagimsiz (shared) yuzeyi.
/// </summary>
/// <remarks>
/// Bu akislar kimlik dogrulama gerektirmez. Kullanici akislari (emir/bakiye guncellemeleri)
/// henuz uygulanmamistir.
/// </remarks>
public interface IBtcTurkSocketClientSpotApiShared :
    ISharedClient,
    ITickerSocketClient,
    ITickersSocketClient,
    ITradeSocketClient,
    IOrderBookSocketClient
{
}
