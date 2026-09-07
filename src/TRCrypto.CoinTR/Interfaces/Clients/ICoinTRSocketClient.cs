using TRCrypto.CoinTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.CoinTR.Interfaces.Clients;

/// <summary>CoinTR WebSocket istemcisi.</summary>
/// <remarks>Bu surumde yalnizca herkese acik akislar sunulur.</remarks>
public interface ICoinTRSocketClient : ISocketClient<CoinTRCredentials>
{
    /// <summary>Spot piyasa akislari.</summary>
    ICoinTRSocketClientSpotApi SpotApi { get; }
}
