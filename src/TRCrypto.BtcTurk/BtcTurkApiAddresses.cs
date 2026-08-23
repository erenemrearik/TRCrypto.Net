namespace TRCrypto.BtcTurk;

/// <summary>BtcTurk API adresleri.</summary>
public class BtcTurkApiAddresses
{
    /// <summary>REST API taban adresi.</summary>
    public string RestClientAddress { get; set; } = string.Empty;

    /// <summary>WebSocket taban adresi.</summary>
    public string SocketClientAddress { get; set; } = string.Empty;

    /// <summary>BtcTurk'un canli ortam adresleri.</summary>
    public static BtcTurkApiAddresses Default { get; } = new()
    {
        RestClientAddress = "https://api.btcturk.com",
        SocketClientAddress = "wss://ws-feed-pro.btcturk.com"
    };
}
