namespace TRCrypto.BinanceTR;

/// <summary>Binance TR API adresleri.</summary>
public class BinanceTRApiAddresses
{
    /// <summary>REST API taban adresi.</summary>
    public string RestClientAddress { get; set; } = string.Empty;

    /// <summary>Piyasa verisi WebSocket taban adresi.</summary>
    public string SocketClientAddress { get; set; } = string.Empty;

    /// <summary>Kullanici akisi WebSocket adresi.</summary>
    /// <remarks>
    /// Piyasa akisindan <b>ayri bir sunucudur</b> ve protokolu de farklidir: akis adi
    /// adres yoluna yazilmaz, baglanti kurulduktan sonra yontem adi tasiyan bir mesaj
    /// gonderilir.
    /// </remarks>
    public string UserStreamClientAddress { get; set; } = string.Empty;

    /// <summary>Binance TR'nin canli ortam adresleri.</summary>
    public static BinanceTRApiAddresses Default { get; } = new()
    {
        RestClientAddress = "https://www.binance.tr",
        SocketClientAddress = "wss://stream-cloud.binance.tr",
        UserStreamClientAddress = "wss://ws-api.binance.tr/ws-api/v3"
    };
}
