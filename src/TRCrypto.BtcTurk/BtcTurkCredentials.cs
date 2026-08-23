using CryptoExchange.Net.Authentication;

namespace TRCrypto.BtcTurk;

/// <summary>
/// BtcTurk API kimlik bilgileri.
/// </summary>
/// <remarks>
/// BtcTurk imzalama icin public key (<c>X-PCK</c>) ve Base64 kodlu bir secret kullanir.
/// Secret'in cozulmesi imzalama katmaninin sorumlulugundadir; cagiran tarafin secret'i
/// onceden decode etmesi gerekmez.
/// <para>
/// Bu tipin <see cref="ToString"/> ciktisi hicbir zaman ham anahtar veya secret icermez;
/// yalnizca hangi anahtarin kullanildigini ayirt etmeye yarayan maskeli bir parmak izi verir
/// (spesifikasyon Bolum 12.2).
/// </para>
/// </remarks>
public class BtcTurkCredentials : HMACCredential
{
    /// <summary>DI ve yapilandirma baglama icin parametresiz kurucu.</summary>
    public BtcTurkCredentials()
    {
    }

    /// <summary>Yeni bir kimlik bilgisi olusturur.</summary>
    /// <param name="apiKey">API public key; isteklerde <c>X-PCK</c> basligi olarak gonderilir.</param>
    /// <param name="apiSecret">Base64 kodlu API secret.</param>
    public BtcTurkCredentials(string apiKey, string apiSecret)
        : base(apiKey, apiSecret)
    {
    }

    /// <inheritdoc />
    public override ApiCredentials Copy() => new BtcTurkCredentials(Key, Secret);

    /// <summary>
    /// Teshis icin maskeli bir gosterim dondurur. Ham anahtar veya secret icermez.
    /// </summary>
    public override string ToString() => $"BtcTurkCredentials(key: {Mask(Key)})";

    /// <summary>
    /// Bir degeri, kaynagini ayirt etmeye yetecek kadarini birakip maskeler.
    /// Kisa degerler tamamen maskelenir; aksi halde anahtarin buyuk bolumu sizabilirdi.
    /// </summary>
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
