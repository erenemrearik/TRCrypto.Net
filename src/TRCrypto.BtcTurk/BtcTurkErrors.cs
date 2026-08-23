using CryptoExchange.Net.Objects.Errors;

namespace TRCrypto.BtcTurk;

/// <summary>BtcTurk hata kodlarinin kutuphane hata tiplerine eslenmesi.</summary>
public static class BtcTurkErrors
{
    /// <summary>
    /// Bilinen hata kodlari.
    /// </summary>
    /// <remarks>
    /// Liste bilerek kucuk tutulmustur; her kod resmi dokumantasyondan dogrulandikca eklenir.
    /// Burada bulunmayan bir kod yutulmaz: ham kod ve mesaj cagirana oldugu gibi tasinir.
    /// </remarks>
    public static ErrorMapping Mapping { get; } = new(
    [
        new ErrorInfo(ErrorType.Unauthorized, false, "Kimlik dogrulama basarisiz", "FAILED_API_AUTHENTICATION"),
        new ErrorInfo(ErrorType.InvalidParameter, false, "Gecersiz istek", "INVALID_REQUEST"),
        new ErrorInfo(ErrorType.RateLimitRequest, true, "Istek limiti asildi", "TOO_MANY_REQUESTS")
    ]);
}
