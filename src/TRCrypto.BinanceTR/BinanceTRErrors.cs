namespace TRCrypto.BinanceTR;

/// <summary>Binance TR hata kodlarinin kutuphane hata tiplerine eslenmesi.</summary>
public static class BinanceTRErrors
{
    /// <summary>
    /// Bilinen hata kodlari.
    /// </summary>
    /// <remarks>
    /// Liste bilerek kucuk tutulmustur; her kod dogrulandikca eklenir. Burada bulunmayan
    /// bir kod yutulmaz, ham kod ve mesaj cagirana tasinir.
    /// </remarks>
    public static ErrorMapping Mapping { get; } = new(
    [
        new ErrorInfo(ErrorType.Unauthorized, false, "Gecersiz API anahtari, IP ya da izin", "3701"),

        // Bu kodun mesaji "Incorrect Page number" olsa da emir defteri ucunda gecersiz
        // limit degeri icin de donuyor; yaniltici oldugu icin aciklama genellestirildi.
        new ErrorInfo(ErrorType.InvalidParameter, false, "Gecersiz parametre degeri", "1106"),
        new ErrorInfo(ErrorType.InvalidParameter, false, "Gecersiz aralik", "2803"),
        new ErrorInfo(ErrorType.UnknownSymbol, false, "Bilinmeyen parite", "2802")
    ]);
}
