namespace TRCrypto.CoinTR;

/// <summary>
/// CoinTR API kimlik bilgileri.
/// </summary>
/// <remarks>
/// <para>
/// Kimlik bilgisi <b>uc parcalidir</b>: anahtar, secret ve bir <b>parola</b>. Diger uc
/// borsa anahtar ve secret ile yetinir; burada parola da her istekte gonderilir ve
/// eksikse istek reddedilir. Parola anahtar olusturulurken belirlenir ve sonradan
/// goruntulenemez.
/// </para>
/// <para>
/// Secret ham metin olarak HMAC anahtari yapilir; BtcTurk'teki gibi Base64 cozulmez.
/// </para>
/// <para>
/// <see cref="ToString"/> ciktisi hicbir zaman ham anahtar, secret ya da parola icermez.
/// </para>
/// </remarks>
public class CoinTRCredentials : HMACPassCredential
{
    /// <summary>DI ve yapilandirma baglama icin parametresiz kurucu.</summary>
    public CoinTRCredentials()
    {
    }

    /// <summary>Yeni bir kimlik bilgisi olusturur.</summary>
    /// <param name="apiKey">API anahtari; <c>ACCESS-KEY</c> basligi olarak gonderilir.</param>
    /// <param name="apiSecret">API secret; imzanin HMAC anahtaridir.</param>
    /// <param name="passphrase">
    /// Anahtar olusturulurken belirlenen parola; <c>ACCESS-PASSPHRASE</c> basligi olarak
    /// gonderilir.
    /// </param>
    public CoinTRCredentials(string apiKey, string apiSecret, string passphrase)
        : base(apiKey, apiSecret, passphrase)
    {
    }

    /// <inheritdoc />
    public override ApiCredentials Copy() => new CoinTRCredentials(Key, Secret, Pass);

    /// <summary>Teshis icin maskeli bir gosterim dondurur.</summary>
    public override string ToString() => $"CoinTRCredentials(key: {Mask(Key)})";

    private static string Mask(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "<bos>";

        if (value!.Length <= 8)
            return "...";

        var head = value.Substring(0, 4);
        var tail = value.Substring(value.Length - 2);
        return $"{head}...{tail}";
    }
}
