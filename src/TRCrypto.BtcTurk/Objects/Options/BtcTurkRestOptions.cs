using CryptoExchange.Net.Objects.Options;

namespace TRCrypto.BtcTurk.Objects.Options;

/// <summary><see cref="Clients.BtcTurkRestClient"/> icin secenekler.</summary>
public class BtcTurkRestOptions : RestExchangeOptions<BtcTurkEnvironment, BtcTurkCredentials>
{
    /// <summary>Yeni istemciler icin varsayilan secenekler.</summary>
    internal static BtcTurkRestOptions Default { get; set; } = new()
    {
        Environment = BtcTurkEnvironment.Live
    };

    /// <summary>Yeni bir secenek nesnesi olusturur.</summary>
    public BtcTurkRestOptions()
    {
        Default?.Set(this);
    }

    /// <summary>Spot API secenekleri.</summary>
    public RestApiOptions SpotOptions { get; private set; } = new();

    internal BtcTurkRestOptions Set(BtcTurkRestOptions targetOptions)
    {
        targetOptions = base.Set(targetOptions);
        targetOptions.SpotOptions = SpotOptions.Set(targetOptions.SpotOptions);
        return targetOptions;
    }
}
