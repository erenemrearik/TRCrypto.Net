namespace TRCrypto.BtcTurk.Enums;

/// <summary>Bir emrin durumu.</summary>
/// <remarks>Tanimli olmayan bir deger geldiginde ayristirma basarisiz olmaz.</remarks>
[JsonConverter(typeof(EnumConverter<OrderStatus>))]
public enum OrderStatus
{
    /// <summary>["<c>Untouched</c>"] Emir acik; henuz hic eslesme olmadi.</summary>
    [Map("Untouched")]
    Untouched,

    /// <summary>["<c>Partial</c>"] Emir kismen gerceklesti; kalani acik.</summary>
    [Map("Partial")]
    PartiallyFilled,

    /// <summary>["<c>Filled</c>"] Emir tamamen gerceklesti.</summary>
    [Map("Filled")]
    Filled,

    /// <summary>["<c>Canceled</c>"] Emir iptal edildi.</summary>
    [Map("Canceled")]
    Canceled
}
