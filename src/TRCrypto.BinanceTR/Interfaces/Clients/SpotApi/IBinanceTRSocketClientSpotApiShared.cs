using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Binance TR spot WebSocket API'sinin borsadan bagimsiz (shared) yuzeyi.
/// </summary>
/// <remarks>
/// Ticker verisi bu borsada yalnizca burada bulunur; REST tarafinda anahtarsiz karsiligi
/// yoktur. Kullanici akislari henuz uygulanmamistir.
/// </remarks>
public interface IBinanceTRSocketClientSpotApiShared :
    ISharedClient,
    ITickerSocketClient,
    ITradeSocketClient,
    IOrderBookSocketClient
{
}
