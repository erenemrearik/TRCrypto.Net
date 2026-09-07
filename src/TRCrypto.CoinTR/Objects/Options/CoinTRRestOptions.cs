using CryptoExchange.Net.Objects.Options;

namespace TRCrypto.CoinTR.Objects.Options;

/// <summary><see cref="Clients.CoinTRRestClient"/> icin secenekler.</summary>
public class CoinTRRestOptions : RestExchangeOptions<CoinTREnvironment, CoinTRCredentials>
{
    /// <summary>Yeni istemciler icin varsayilan secenekler.</summary>
    internal static CoinTRRestOptions Default { get; set; } = new()
    {
        Environment = CoinTREnvironment.Live
    };

    /// <summary>Yeni bir secenek nesnesi olusturur.</summary>
    public CoinTRRestOptions()
    {
        Default?.Set(this);
    }

    /// <summary>Spot API secenekleri.</summary>
    public RestApiOptions SpotOptions { get; private set; } = new();

    internal CoinTRRestOptions Set(CoinTRRestOptions targetOptions)
    {
        targetOptions = base.Set(targetOptions);
        targetOptions.SpotOptions = SpotOptions.Set(targetOptions.SpotOptions);
        return targetOptions;
    }
}
