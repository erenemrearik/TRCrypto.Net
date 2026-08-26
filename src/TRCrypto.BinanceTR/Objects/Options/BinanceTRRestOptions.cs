using CryptoExchange.Net.Objects.Options;

namespace TRCrypto.BinanceTR.Objects.Options;

/// <summary><see cref="Clients.BinanceTRRestClient"/> icin secenekler.</summary>
public class BinanceTRRestOptions : RestExchangeOptions<BinanceTREnvironment, BinanceTRCredentials>
{
    /// <summary>Yeni istemciler icin varsayilan secenekler.</summary>
    internal static BinanceTRRestOptions Default { get; set; } = new()
    {
        Environment = BinanceTREnvironment.Live
    };

    /// <summary>Yeni bir secenek nesnesi olusturur.</summary>
    public BinanceTRRestOptions()
    {
        Default?.Set(this);
    }

    /// <summary>Spot API secenekleri.</summary>
    public RestApiOptions SpotOptions { get; private set; } = new();

    internal BinanceTRRestOptions Set(BinanceTRRestOptions targetOptions)
    {
        targetOptions = base.Set(targetOptions);
        targetOptions.SpotOptions = SpotOptions.Set(targetOptions.SpotOptions);
        return targetOptions;
    }
}
