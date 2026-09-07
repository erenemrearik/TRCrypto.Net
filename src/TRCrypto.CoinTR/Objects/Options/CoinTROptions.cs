namespace TRCrypto.CoinTR.Objects.Options;

/// <summary>
/// <see cref="CoinTRServiceCollectionExtensions.AddTRCryptoCoinTR"/> icin secenekler.
/// </summary>
/// <remarks>
/// REST ve WebSocket istemcilerinin secenekleri ayridir; ikisi de gerektiginde tek tek
/// yapilandirilabilir.
/// </remarks>
public class CoinTROptions
{
    /// <summary>REST istemcisi secenekleri.</summary>
    public CoinTRRestOptions Rest { get; } = new();

    /// <summary>WebSocket istemcisi secenekleri.</summary>
    public CoinTRSocketOptions Socket { get; } = new();

    /// <summary>Her iki istemciye uygulanacak kimlik bilgisi.</summary>
    /// <remarks>
    /// CoinTR kimlik bilgisi uc parcalidir: anahtar, secret ve parola.
    /// </remarks>
    public CoinTRCredentials? ApiCredentials { get; set; }
}
