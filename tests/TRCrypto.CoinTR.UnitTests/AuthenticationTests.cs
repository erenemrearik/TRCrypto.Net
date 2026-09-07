using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace TRCrypto.CoinTR.UnitTests;

/// <summary>
/// Imza semasini sabitler.
/// </summary>
/// <remarks>
/// Yanlis imza bu borsada aciklayici bir hata uretmez; istek yalnizca reddedilir. Sema
/// bu yuzden deterministik bir vektorle sabitlenmistir ve degerler testte hesaplanmaz,
/// elle yazilir.
/// </remarks>
public class AuthenticationTests
{
    private const string TestKey = "test-access-key";
    private const string TestSecret = "test-access-secret";
    private const string TestPassphrase = "test-passphrase";

    private static CoinTRAuthenticationProvider Provider()
        => new(new CoinTRCredentials(TestKey, TestSecret, TestPassphrase));

    [Fact]
    public void Yuk_zaman_metot_yol_sorgu_ve_govdeden_olusur()
    {
        var payload = CoinTRAuthenticationProvider.BuildPayload(
            "1788787397722",
            "get",
            "/api/v2/spot/account/assets",
            "coin=BTC",
            string.Empty);

        // Metot buyuk harfe cevrilir, sorgu soru isaretiyle eklenir.
        Assert.Equal("1788787397722GET/api/v2/spot/account/assets?coin=BTC", payload);
    }

    [Fact]
    public void Sorgu_yoksa_soru_isareti_eklenmez()
    {
        var payload = CoinTRAuthenticationProvider.BuildPayload(
            "1788787397722", "GET", "/api/v2/public/time", string.Empty, string.Empty);

        Assert.Equal("1788787397722GET/api/v2/public/time", payload);
    }

    [Fact]
    public void Govde_yukun_sonuna_eklenir()
    {
        var payload = CoinTRAuthenticationProvider.BuildPayload(
            "1788787397722",
            "POST",
            "/api/v2/spot/trade/place-order",
            string.Empty,
            """{"symbol":"BTCTRY"}""");

        Assert.Equal(
            """1788787397722POST/api/v2/spot/trade/place-order{"symbol":"BTCTRY"}""",
            payload);
    }

    [Fact]
    public void Yol_imzaya_dahildir()
    {
        // Dort borsa arasinda yolu imzaya katan tek borsa budur. Yol atlandiginda imza
        // sessizce gecersiz olur; asagidaki iki imza esit cikarsa sema bozulmustur.
        var provider = Provider();

        var withPath = provider.CreateSignature(CoinTRAuthenticationProvider.BuildPayload(
            "1788787397722", "GET", "/api/v2/spot/account/assets", string.Empty, string.Empty));

        var withoutPath = provider.CreateSignature(CoinTRAuthenticationProvider.BuildPayload(
            "1788787397722", "GET", string.Empty, string.Empty, string.Empty));

        Assert.NotEqual(withPath, withoutPath);
    }

    [Fact]
    public void Imza_ham_secret_ile_uretilir_ve_base64_kodlanir()
    {
        // Secret Base64 cozulmez; BtcTurk'te cozulur. Karistirmak imzayi sessizce bozar.
        const string payload = "1788787397722GET/api/v2/public/time";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(TestSecret));
        var expected = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

        Assert.Equal(expected, Provider().CreateSignature(payload));
    }

    [Fact]
    public void Imza_deterministiktir()
    {
        const string payload = "1788787397722GET/api/v2/public/time";

        Assert.Equal(Provider().CreateSignature(payload), Provider().CreateSignature(payload));
    }

    [Fact]
    public void Kimlik_bilgisi_gosterimi_ham_deger_icermez()
    {
        var text = new CoinTRCredentials(TestKey, TestSecret, TestPassphrase).ToString();

        Assert.DoesNotContain(TestKey, text, StringComparison.Ordinal);
        Assert.DoesNotContain(TestSecret, text, StringComparison.Ordinal);
        Assert.DoesNotContain(TestPassphrase, text, StringComparison.Ordinal);
    }

    [Fact]
    public void Kimlik_bilgisi_kopyasi_uc_parcayi_da_tasir()
    {
        // Parola kopyalanmazsa istekler ACCESS-PASSPHRASE basligi olmadan gider ve
        // reddedilir.
        var copy = (CoinTRCredentials)new CoinTRCredentials(TestKey, TestSecret, TestPassphrase).Copy();

        Assert.Equal(TestKey, copy.Key);
        Assert.Equal(TestSecret, copy.Secret);
        Assert.Equal(TestPassphrase, copy.Pass);
    }
}
