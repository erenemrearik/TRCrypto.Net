namespace TRCrypto.BtcTurk.Enums;

/// <summary>
/// Bir paritenin islem durumu.
/// </summary>
/// <remarks>
/// Borsa burada tanimli olmayan bir deger dondururse ayristirma basarisiz olmaz; alan
/// tanimsiz bir enum degerine ayarlanir. Bu durum <c>Enum.IsDefined</c> ile tespit edilebilir.
/// Bu davranis CryptoExchange.Net ekosisteminin genelinde aynidir; bu yuzden burada
/// bilerek bir <c>Unknown</c> uyesi tanimlanmamistir.
/// </remarks>
[JsonConverter(typeof(EnumConverter<SymbolStatus>))]
public enum SymbolStatus
{
    /// <summary>["<c>TRADING</c>"] Parite islem gormektedir.</summary>
    [Map("TRADING")]
    Trading
}
