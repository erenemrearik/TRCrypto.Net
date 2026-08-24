namespace TRCrypto.BtcTurk.Enums;

/// <summary>Bir islemin ya da emrin yonu.</summary>
/// <remarks>Tanimli olmayan bir deger geldiginde ayristirma basarisiz olmaz.</remarks>
[JsonConverter(typeof(EnumConverter<OrderSide>))]
public enum OrderSide
{
    /// <summary>["<c>buy</c>"] Alis.</summary>
    [Map("buy")]
    Buy,

    /// <summary>["<c>sell</c>"] Satis.</summary>
    [Map("sell")]
    Sell
}
