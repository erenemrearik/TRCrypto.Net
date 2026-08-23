namespace TRCrypto.BtcTurk.Enums;

/// <summary>
/// Bir paritede kullanilabilen emir yontemi.
/// </summary>
/// <remarks>
/// Borsa burada tanimli olmayan bir deger dondururse ayristirma basarisiz olmaz; deger
/// tanimsiz bir enum degerine ayarlanir ve <c>Enum.IsDefined</c> ile tespit edilebilir.
/// </remarks>
[JsonConverter(typeof(EnumConverter<OrderMethod>))]
public enum OrderMethod
{
    /// <summary>["<c>MARKET</c>"] Piyasa emri.</summary>
    [Map("MARKET")]
    Market,
    /// <summary>["<c>LIMIT</c>"] Limit emri.</summary>
    [Map("LIMIT")]
    Limit,
    /// <summary>["<c>STOP_MARKET</c>"] Stop piyasa emri.</summary>
    [Map("STOP_MARKET")]
    StopMarket,
    /// <summary>["<c>STOP_LIMIT</c>"] Stop limit emri.</summary>
    [Map("STOP_LIMIT")]
    StopLimit
}
