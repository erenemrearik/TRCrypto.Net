using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.BinanceTR.Interfaces.Clients;

/// <summary>Binance TR WebSocket istemcisi.</summary>
/// <remarks>Istemci uzun omurludur; her abonelik icin yeni bir ornek olusturmayin.</remarks>
public interface IBinanceTRSocketClient : ISocketClient
{
    /// <summary>Spot WebSocket API.</summary>
    IBinanceTRSocketClientSpotApi SpotApi { get; }
}
