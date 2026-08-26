using CryptoExchange.Net.Interfaces;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

namespace TRCrypto.BtcTurk.Interfaces.Clients;

/// <summary>
/// BtcTurk WebSocket istemcisi.
/// </summary>
/// <remarks>
/// Istemci uzun omurludur ve birden fazla abonelik ayni baglanti uzerinde paylasilir;
/// her abonelik icin yeni bir istemci olusturmayin.
/// </remarks>
public interface IBtcTurkSocketClient : ISocketClient
{
    /// <summary>Spot WebSocket API.</summary>
    IBtcTurkSocketClientSpotApi SpotApi { get; }
}
