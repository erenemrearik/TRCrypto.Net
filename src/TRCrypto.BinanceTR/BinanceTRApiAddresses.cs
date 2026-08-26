namespace TRCrypto.BinanceTR;

/// <summary>Binance TR API adresleri.</summary>
public class BinanceTRApiAddresses
{
    /// <summary>REST API taban adresi.</summary>
    public string RestClientAddress { get; set; } = string.Empty;

    /// <summary>WebSocket taban adresi.</summary>
    public string SocketClientAddress { get; set; } = string.Empty;

    /// <summary>Binance TR'nin canli ortam adresleri.</summary>
    public static BinanceTRApiAddresses Default { get; } = new()
    {
        RestClientAddress = "https://www.binance.tr",
        SocketClientAddress = "wss://stream-cloud.binance.tr"
    };
}
