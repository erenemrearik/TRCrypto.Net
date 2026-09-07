namespace TRCrypto.BinanceTR;

/// <summary>Binance TR calisma ortamlari.</summary>
/// <remarks>Binance TR bir test/sandbox ortami sunmaz; yalnizca canli ortam tanimlidir.</remarks>
public class BinanceTREnvironment : TradeEnvironment
{
    /// <summary>REST API taban adresi.</summary>
    public string RestBaseAddress { get; }

    /// <summary>Piyasa verisi WebSocket taban adresi.</summary>
    public string SocketBaseAddress { get; }

    /// <summary>Kullanici akisi WebSocket adresi.</summary>
    public string UserStreamAddress { get; }

    internal BinanceTREnvironment(
        string name, string restBaseAddress, string socketBaseAddress, string userStreamAddress)
        : base(name)
    {
        RestBaseAddress = restBaseAddress;
        SocketBaseAddress = socketBaseAddress;
        UserStreamAddress = userStreamAddress;
    }

    /// <summary>Bagimlilik enjeksiyonu icin kurucu.</summary>
#pragma warning disable CS8618
    public BinanceTREnvironment() : base(TradeEnvironmentNames.Live)
#pragma warning restore CS8618
    {
    }

    /// <summary>Canli ortam.</summary>
    public static BinanceTREnvironment Live { get; } = new(
        TradeEnvironmentNames.Live,
        BinanceTRApiAddresses.Default.RestClientAddress,
        BinanceTRApiAddresses.Default.SocketClientAddress,
        BinanceTRApiAddresses.Default.UserStreamClientAddress);

    /// <summary>Tanimli ortam adlari.</summary>
    public static string[] All => [Live.Name];

    /// <summary>Adina gore ortam dondurur.</summary>
    /// <param name="name">Ortam adi.</param>
    /// <returns>Bulunan ortam; bulunamazsa <c>null</c>.</returns>
    public static BinanceTREnvironment? GetEnvironmentByName(string? name)
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
    /// <param name="socketAddress">Piyasa verisi WebSocket taban adresi.</param>
    /// <param name="userStreamAddress">
    /// Kullanici akisi WebSocket adresi. Verilmezse canli ortamin adresi kullanilir.
    /// </param>
    /// <returns>Olusturulan ortam.</returns>
    public static BinanceTREnvironment CreateCustom(
        string name,
        string restAddress,
        string socketAddress,
        string? userStreamAddress = null)
        => new(
            name,
            restAddress,
            socketAddress,
            userStreamAddress ?? BinanceTRApiAddresses.Default.UserStreamClientAddress);
}
