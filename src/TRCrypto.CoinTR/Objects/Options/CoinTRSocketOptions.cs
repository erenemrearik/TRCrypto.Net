using CryptoExchange.Net.Objects.Options;

namespace TRCrypto.CoinTR.Objects.Options;

/// <summary><see cref="Clients.CoinTRSocketClient"/> icin secenekler.</summary>
public class CoinTRSocketOptions : SocketExchangeOptions<CoinTREnvironment, CoinTRCredentials>
{
    /// <summary>Yeni istemciler icin varsayilan secenekler.</summary>
    internal static CoinTRSocketOptions Default { get; set; } = new()
    {
        Environment = CoinTREnvironment.Live
    };

    /// <summary>Yeni bir secenek nesnesi olusturur.</summary>
    public CoinTRSocketOptions()
    {
        Default?.Set(this);
    }

    /// <summary>Spot API secenekleri.</summary>
    public SocketApiOptions SpotOptions { get; private set; } = new();


    internal CoinTRSocketOptions Set(CoinTRSocketOptions targetOptions)
    {
        targetOptions = base.Set(targetOptions);
        targetOptions.SpotOptions = SpotOptions.Set(targetOptions.SpotOptions);
        return targetOptions;
    }
}
