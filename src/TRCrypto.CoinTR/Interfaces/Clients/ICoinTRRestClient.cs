using TRCrypto.CoinTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.CoinTR.Interfaces.Clients;

/// <summary>
/// CoinTR REST API istemcisi.
/// </summary>
/// <remarks>Istemci uzun omurludur; her istek icin yeni bir ornek olusturmayin.</remarks>
public interface ICoinTRRestClient : IRestClient
{
    /// <summary>Spot API.</summary>
    ICoinTRRestClientSpotApi SpotApi { get; }
}
