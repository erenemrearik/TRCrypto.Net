using CryptoExchange.Net.Objects.Options;

namespace TRCrypto.BinanceTR.Objects.Options;

/// <summary><see cref="Clients.BinanceTRSocketClient"/> icin secenekler.</summary>
public class BinanceTRSocketOptions : SocketExchangeOptions<BinanceTREnvironment, BinanceTRCredentials>
{
    /// <summary>Yeni istemciler icin varsayilan secenekler.</summary>
    internal static BinanceTRSocketOptions Default { get; set; } = new()
    {
        Environment = BinanceTREnvironment.Live
    };

    /// <summary>Yeni bir secenek nesnesi olusturur.</summary>
    public BinanceTRSocketOptions()
    {
        Default?.Set(this);
    }

    /// <summary>Spot piyasa akisi secenekleri.</summary>
    public SocketApiOptions SpotOptions { get; private set; } = new();

    /// <summary>Kullanici akisi secenekleri.</summary>
    public SocketApiOptions UserOptions { get; private set; } = new();

    internal BinanceTRSocketOptions Set(BinanceTRSocketOptions targetOptions)
    {
        targetOptions = base.Set(targetOptions);
        targetOptions.SpotOptions = SpotOptions.Set(targetOptions.SpotOptions);
        targetOptions.UserOptions = UserOptions.Set(targetOptions.UserOptions);
        return targetOptions;
    }
}
