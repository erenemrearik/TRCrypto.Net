using CryptoExchange.Net.Objects.Options;

namespace TRCrypto.BtcTurk.Objects.Options;

/// <summary><see cref="Clients.BtcTurkSocketClient"/> icin secenekler.</summary>
public class BtcTurkSocketOptions : SocketExchangeOptions<BtcTurkEnvironment, BtcTurkCredentials>
{
    /// <summary>Yeni istemciler icin varsayilan secenekler.</summary>
    internal static BtcTurkSocketOptions Default { get; set; } = new()
    {
        Environment = BtcTurkEnvironment.Live,
        SocketSubscriptionsCombineTarget = 10
    };

    /// <summary>Yeni bir secenek nesnesi olusturur.</summary>
    public BtcTurkSocketOptions()
    {
        Default?.Set(this);
    }

    /// <summary>Spot API secenekleri.</summary>
    public SocketApiOptions SpotOptions { get; private set; } = new();

    internal BtcTurkSocketOptions Set(BtcTurkSocketOptions targetOptions)
    {
        targetOptions = base.Set(targetOptions);
        targetOptions.SpotOptions = SpotOptions.Set(targetOptions.SpotOptions);
        return targetOptions;
    }
}
