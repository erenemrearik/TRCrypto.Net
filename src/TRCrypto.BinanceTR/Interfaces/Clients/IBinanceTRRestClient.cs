using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.BinanceTR.Interfaces.Clients;

/// <summary>
/// Binance TR REST API istemcisi.
/// </summary>
/// <remarks>Istemci uzun omurludur; her istek icin yeni bir ornek olusturmayin.</remarks>
public interface IBinanceTRRestClient : IRestClient
{
    /// <summary>Spot API.</summary>
    IBinanceTRRestClientSpotApi SpotApi { get; }
}
