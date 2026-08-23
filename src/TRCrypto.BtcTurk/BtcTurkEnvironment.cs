using CryptoExchange.Net.Objects;

namespace TRCrypto.BtcTurk;

/// <summary>BtcTurk calisma ortamlari.</summary>
/// <remarks>BtcTurk bir test/sandbox ortami sunmaz; yalnizca canli ortam tanimlidir.</remarks>
public class BtcTurkEnvironment : TradeEnvironment
{
    /// <summary>REST API taban adresi.</summary>
    public string RestBaseAddress { get; }

    /// <summary>WebSocket taban adresi.</summary>
    public string SocketBaseAddress { get; }

    internal BtcTurkEnvironment(string name, string restBaseAddress, string socketBaseAddress)
        : base(name)
    {
        RestBaseAddress = restBaseAddress;
        SocketBaseAddress = socketBaseAddress;
    }

    /// <summary>Bagimlilik enjeksiyonu icin kurucu; ozel ortam icin <see cref="CreateCustom"/> kullanin.</summary>
#pragma warning disable CS8618
    public BtcTurkEnvironment() : base(TradeEnvironmentNames.Live)
#pragma warning restore CS8618
    {
    }

    /// <summary>Canli ortam.</summary>
    public static BtcTurkEnvironment Live { get; } = new(
        TradeEnvironmentNames.Live,
        BtcTurkApiAddresses.Default.RestClientAddress,
        BtcTurkApiAddresses.Default.SocketClientAddress);

    /// <summary>Tanimli ortam adlari.</summary>
    public static string[] All => [Live.Name];

    /// <summary>Adina gore ortam dondurur.</summary>
    /// <param name="name">Ortam adi.</param>
    /// <returns>Bulunan ortam; bulunamazsa <c>null</c>.</returns>
    public static BtcTurkEnvironment? GetEnvironmentByName(string? name)
        => name switch
        {
            TradeEnvironmentNames.Live => Live,
            "" => Live,
            null => Live,
            _ => default
        };

    /// <summary>Ozel bir ortam olusturur.</summary>
    /// <param name="name">Ortam adi.</param>
    /// <param name="restAddress">REST taban adresi.</param>
    /// <param name="socketAddress">WebSocket taban adresi.</param>
    /// <returns>Olusturulan ortam.</returns>
    public static BtcTurkEnvironment CreateCustom(string name, string restAddress, string socketAddress)
        => new(name, restAddress, socketAddress);
}
