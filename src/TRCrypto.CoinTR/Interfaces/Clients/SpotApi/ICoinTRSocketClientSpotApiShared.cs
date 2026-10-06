using CryptoExchange.Net.SharedApis;

namespace TRCrypto.CoinTR.Interfaces.Clients.SpotApi;

/// <summary>
/// CoinTR spot WebSocket API'sinin borsadan bagimsiz (shared) yuzeyi.
/// </summary>
/// <remarks>
/// Bu yuzeyde yalnizca herkese acik akislar bulunur. Kullaniciya ozel akislar borsanin
/// ozel WebSocket adresini ve imzali bir oturum acmayi gerektirir; bu surumde
/// uygulanmamistir.
/// </remarks>
public interface ICoinTRSocketClientSpotApiShared :
    ISharedClient,
    ITickerSocketClient,
    ITradeSocketClient,
    IOrderBookSocketClient
{
}
