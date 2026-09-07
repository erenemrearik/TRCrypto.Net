namespace TRCrypto.CoinTR;

/// <summary>CoinTR API adresleri.</summary>
public class CoinTRApiAddresses
{
    /// <summary>REST API taban adresi.</summary>
    public string RestClientAddress { get; set; } = string.Empty;

    /// <summary>Piyasa verisi WebSocket adresi.</summary>
    public string SocketClientAddress { get; set; } = string.Empty;

    /// <summary>CoinTR'nin canli ortam adresleri.</summary>
    /// <remarks>Adresler 8 Eylul 2026'da canli olarak dogrulanmistir.</remarks>
    public static CoinTRApiAddresses Default { get; } = new()
    {
        RestClientAddress = "https://api.cointr.pro",
        SocketClientAddress = "wss://ws.cointr.pro/v2/ws/public"
    };
}
