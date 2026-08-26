namespace TRCrypto.BinanceTR.Objects.Options;

/// <summary>
/// <see cref="BinanceTRServiceCollectionExtensions.AddTRCryptoBinanceTR"/> icin secenekler.
/// </summary>
/// <remarks>
/// REST ve WebSocket istemcilerinin secenekleri ayridir; ikisi de gerektiginde tek tek
/// yapilandirilabilir.
/// </remarks>
public class BinanceTROptions
{
    /// <summary>REST istemcisi secenekleri.</summary>
    public BinanceTRRestOptions Rest { get; } = new();

    /// <summary>WebSocket istemcisi secenekleri.</summary>
    public BinanceTRSocketOptions Socket { get; } = new();

    /// <summary>Her iki istemciye uygulanacak kimlik bilgisi.</summary>
    /// <remarks>
    /// Bu surumde kimlik dogrulama devre disidir; public piyasa verisi uclari ve tum
    /// WebSocket akislari anahtarsiz calisir.
    /// </remarks>
    public BinanceTRCredentials? ApiCredentials { get; set; }
}
