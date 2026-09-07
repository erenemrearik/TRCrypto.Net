namespace TRCrypto.CoinTR;

/// <summary>CoinTR hata kodlarinin kutuphane hata tiplerine eslenmesi.</summary>
public static class CoinTRErrors
{
    /// <summary>
    /// Bilinen hata kodlari.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kodlar <b>metindir</b>, sayi degil; basarili yanit <c>"00000"</c> dondurur.
    /// </para>
    /// <para>
    /// Liste bilerek kucuk tutulmustur ve yalnizca gozlemlenen kodlari icerir. Burada
    /// bulunmayan bir kod yutulmaz; ham kod ve mesaj cagirana tasinir. Uydurulmus bir
    /// esleme, hatanin gercek nedenini gizlemekten baska bir ise yaramaz.
    /// </para>
    /// </remarks>
    public static ErrorMapping Mapping { get; } = new(
    [
        new ErrorInfo(ErrorType.Unauthorized, false, "Gecersiz imza", "40009"),
        new ErrorInfo(ErrorType.Unauthorized, false, "Gecersiz API anahtari", "40006"),
        new ErrorInfo(ErrorType.Unauthorized, false, "Parola gecersiz", "40011"),
        new ErrorInfo(ErrorType.Unauthorized, false, "Zaman damgasi penceresi disinda", "40008"),
        new ErrorInfo(ErrorType.UnknownSymbol, false, "Bilinmeyen parite", "40034"),
        new ErrorInfo(ErrorType.InvalidParameter, false, "Gecersiz parametre degeri", "40017")
    ]);
}
