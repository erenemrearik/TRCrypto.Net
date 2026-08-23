namespace TRCrypto.BtcTurk.Enums;

/// <summary>
/// Bir varligin turu.
/// </summary>
/// <remarks>
/// BtcTurk bu bilgiyi dogrudan bildirir; varlik turu sembol adindan tahmin edilmez.
/// Tanimli olmayan bir deger geldiginde ayristirma basarisiz olmaz.
/// </remarks>
[JsonConverter(typeof(EnumConverter<CurrencyType>))]
public enum CurrencyType
{
    /// <summary>["<c>FIAT</c>"] Itibari para, ornegin TRY.</summary>
    [Map("FIAT")]
    Fiat,
    /// <summary>["<c>CRYPTO</c>"] Kripto varlik.</summary>
    [Map("CRYPTO")]
    Crypto
}
