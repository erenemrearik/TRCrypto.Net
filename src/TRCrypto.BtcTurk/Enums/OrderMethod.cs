namespace TRCrypto.BtcTurk.Enums;

/// <summary>
/// Bir emrin yontemi.
/// </summary>
/// <remarks>
/// <para>
/// Yazim bicimi uclar arasinda degisir: borsa bilgisi ucu <c>STOP_MARKET</c> dondururken
/// emir uclari <c>stopmarket</c> donduruyor. Her iki bicim de eslestirilir; aksi halde
/// stop emirleri taninmaz.
/// </para>
/// <para>Tanimli olmayan bir deger geldiginde ayristirma basarisiz olmaz.</para>
/// </remarks>
[JsonConverter(typeof(EnumConverter<OrderMethod>))]
public enum OrderMethod
{
    /// <summary>["<c>MARKET</c>", "<c>market</c>"] Piyasa emri.</summary>
    [Map("MARKET", "market")]
    Market,

    /// <summary>["<c>LIMIT</c>", "<c>limit</c>"] Limit emri.</summary>
    [Map("LIMIT", "limit")]
    Limit,

    /// <summary>["<c>STOP_MARKET</c>", "<c>stopmarket</c>"] Stop piyasa emri.</summary>
    [Map("STOP_MARKET", "stopmarket")]
    StopMarket,

    /// <summary>["<c>STOP_LIMIT</c>", "<c>stoplimit</c>"] Stop limit emri.</summary>
    [Map("STOP_LIMIT", "stoplimit")]
    StopLimit
}
