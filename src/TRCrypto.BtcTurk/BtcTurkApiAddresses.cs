namespace TRCrypto.BtcTurk;

/// <summary>BtcTurk API adresleri.</summary>
public class BtcTurkApiAddresses
{
    /// <summary>REST API taban adresi.</summary>
    public string RestClientAddress { get; set; } = string.Empty;

    /// <summary>WebSocket taban adresi.</summary>
    public string SocketClientAddress { get; set; } = string.Empty;

    /// <summary>Grafik (kline) API taban adresi.</summary>
    /// <remarks>
    /// Kline ucu ayri bir host uzerinde calisir ve diger uclarin standart zarfini
    /// kullanmaz.
    /// </remarks>
    public string GraphClientAddress { get; set; } = string.Empty;

    /// <summary>BtcTurk'un canli ortam adresleri.</summary>
    public static BtcTurkApiAddresses Default { get; } = new()
    {
        RestClientAddress = "https://api.btcturk.com",
        SocketClientAddress = "wss://ws-feed-pro.btcturk.com",
        GraphClientAddress = "https://graph-api.btcturk.com"
    };
}
