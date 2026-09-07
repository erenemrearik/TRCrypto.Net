namespace TRCrypto.CoinTR;

/// <summary>CoinTR calisma ortamlari.</summary>
/// <remarks>CoinTR bir test/sandbox ortami sunmaz; yalnizca canli ortam tanimlidir.</remarks>
public class CoinTREnvironment : TradeEnvironment
{
    /// <summary>REST API taban adresi.</summary>
    public string RestBaseAddress { get; }

    /// <summary>WebSocket adresi.</summary>
    public string SocketBaseAddress { get; }


    internal CoinTREnvironment(string name, string restBaseAddress, string socketBaseAddress)
        : base(name)
    {
        RestBaseAddress = restBaseAddress;
        SocketBaseAddress = socketBaseAddress;
    }

    /// <summary>Bagimlilik enjeksiyonu icin kurucu.</summary>
#pragma warning disable CS8618
    public CoinTREnvironment() : base(TradeEnvironmentNames.Live)
#pragma warning restore CS8618
    {
    }

    /// <summary>Canli ortam.</summary>
    public static CoinTREnvironment Live { get; } = new(
        TradeEnvironmentNames.Live,
        CoinTRApiAddresses.Default.RestClientAddress,
        CoinTRApiAddresses.Default.SocketClientAddress);

    /// <summary>Tanimli ortam adlari.</summary>
    public static string[] All => [Live.Name];

    /// <summary>Adina gore ortam dondurur.</summary>
    /// <param name="name">Ortam adi.</param>
    /// <returns>Bulunan ortam; bulunamazsa <c>null</c>.</returns>
    public static CoinTREnvironment? GetEnvironmentByName(string? name)
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
    /// <param name="socketAddress">WebSocket adresi.</param>
    /// <returns>Olusturulan ortam.</returns>
    public static CoinTREnvironment CreateCustom(
        string name,
        string restAddress,
        string socketAddress)
        => new(name, restAddress, socketAddress);
}
