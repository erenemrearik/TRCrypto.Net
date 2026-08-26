namespace TRCrypto.BinanceTR;

/// <summary>
/// Binance TR API kimlik bilgileri.
/// </summary>
/// <remarks>
/// <para>
/// Binance TR, BtcTurk'ten farkli olarak secret'i Base64 kodlu beklemez; deger ham metin
/// olarak HMAC anahtari yapilir.
/// </para>
/// <para>
/// <see cref="ToString"/> ciktisi hicbir zaman ham anahtar veya secret icermez.
/// </para>
/// </remarks>
public class BinanceTRCredentials : HMACCredential
{
    /// <summary>DI ve yapilandirma baglama icin parametresiz kurucu.</summary>
    public BinanceTRCredentials()
    {
    }

    /// <summary>Yeni bir kimlik bilgisi olusturur.</summary>
    /// <param name="apiKey">API anahtari; <c>X-MBX-APIKEY</c> basligi olarak gonderilir.</param>
    /// <param name="apiSecret">API secret.</param>
    public BinanceTRCredentials(string apiKey, string apiSecret)
        : base(apiKey, apiSecret)
    {
    }

    /// <inheritdoc />
    public override ApiCredentials Copy() => new BinanceTRCredentials(Key, Secret);

    /// <summary>Teshis icin maskeli bir gosterim dondurur.</summary>
    public override string ToString() => $"BinanceTRCredentials(key: {Mask(Key)})";

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
