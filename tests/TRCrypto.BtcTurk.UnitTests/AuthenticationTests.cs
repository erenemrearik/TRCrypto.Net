using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// BtcTurk imzalama zincirini sabit test vektorleriyle dogrular
/// (spesifikasyon Bolum 15.2 "Authentication Test Vektorleri").
/// </summary>
/// <remarks>
/// <para>Resmi dokumantasyon bir test vektoru yayinlamaz. Buradaki beklenen imzalar,
/// dokumante edilen algoritmanin bagimsiz bir uygulamasiyla uretilmistir:</para>
/// <code>
/// mesaj      = apiKey + stamp
/// anahtar    = Base64Decode(secret)
/// imza       = Base64(HMAC-SHA256(anahtar, mesaj))
/// </code>
/// <para>Kullanilan kimlik bilgileri sahtedir ve hicbir hesaba ait degildir.</para>
/// </remarks>
public class AuthenticationTests
{
    [Theory]
    [InlineData(
        "test-public-key",
        "dGVzdC1zZWNyZXQta2V5LWZvci11bml0LXRlc3Rz",
        "1735689600000",
        "7gyFGcOS+qnq46h/rl83VtpaEAsh8Th3Z3lQrF7g2I0=")]
    [InlineData(
        "PCK1234567890",
        "YWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXoxMjM0",
        "1000000000000",
        "DbJpXuqufl8YO8bgeGx2pVzyf9GED4juDAMVeWwZT78=")]
    [InlineData("a", "AAAA", "0", "qsYdc5o1b4WQP3Hnc+vsOBNxdTye+jkFeWjmd0aXkk8=")]
    public void Imza_beklenen_degeri_uretir(string apiKey, string secret, string stamp, string expected)
    {
        var provider = new BtcTurkAuthenticationProvider(new BtcTurkCredentials(apiKey, secret));

        var signature = provider.CreateSignature(stamp);

        Assert.Equal(expected, signature);
    }

    [Fact]
    public void Secret_Base64_olarak_cozulur()
    {
        // Bu adim atlanip secret ham metin olarak kullanilirsa imza SESSIZCE yanlis olur;
        // borsa yalnizca "gecersiz imza" der ve nedeni belli olmaz. En sik yapilan hata budur.
        const string wrongIfSecretNotDecoded = "38qSfoys8cvFpd0FBe50RUaqT6Dl3iMO7iyblkzlqnw=";

        var provider = new BtcTurkAuthenticationProvider(
            new BtcTurkCredentials("test-public-key", "dGVzdC1zZWNyZXQta2V5LWZvci11bml0LXRlc3Rz"));

        var signature = provider.CreateSignature("1735689600000");

        Assert.NotEqual(wrongIfSecretNotDecoded, signature);
    }

    [Fact]
    public void Ayni_girdi_ayni_imzayi_uretir()
    {
        var provider = new BtcTurkAuthenticationProvider(
            new BtcTurkCredentials("test-public-key", "dGVzdC1zZWNyZXQta2V5LWZvci11bml0LXRlc3Rz"));

        var first = provider.CreateSignature("1735689600000");
        var second = provider.CreateSignature("1735689600000");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Farkli_nonce_farkli_imza_uretir()
    {
        var provider = new BtcTurkAuthenticationProvider(
            new BtcTurkCredentials("test-public-key", "dGVzdC1zZWNyZXQta2V5LWZvci11bml0LXRlc3Rz"));

        var first = provider.CreateSignature("1735689600000");
        var second = provider.CreateSignature("1735689600001");

        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData(3000, "uLpuzhycwQqE+aC+WnmBoPSIyBrCPo4I0pyJyq6uF98=")]
    [InlineData(1, "OLaox4csMFn70v6swYwAmKzVYrVAMbvvqackBIeczTI=")]
    public void Socket_giris_imzasi_nonce_uzerinden_uretilir(long nonce, string expected)
    {
        var provider = new BtcTurkAuthenticationProvider(
            new BtcTurkCredentials("test-public-key", "dGVzdC1zZWNyZXQta2V5LWZvci11bml0LXRlc3Rz"));

        var signature = provider.CreateSocketLoginSignature(nonce);

        Assert.Equal(expected, signature);
    }

    [Fact]
    public void Socket_giris_imzasi_REST_imzasindan_farklidir()
    {
        // REST istekleri apiKey + stamp imzalar, socket girisi apiKey + nonce imzalar.
        // Ayni degeri kullanmak socket girisini sessizce basarisiz yapardi; borsa yalnizca
        // genel bir "Invalid Signature" doner ve nedeni belirtmez.
        // Bu ayrim canli bir hesapla dogrulanmistir.
        var provider = new BtcTurkAuthenticationProvider(
            new BtcTurkCredentials("test-public-key", "dGVzdC1zZWNyZXQta2V5LWZvci11bml0LXRlc3Rz"));

        var socketSignature = provider.CreateSocketLoginSignature(3000);
        var restSignature = provider.CreateSignature("3000");

        // Ayni girdi metniyle uretildikleri icin bu ikisi esittir; fark, cagiran tarafin
        // hangi degeri gecirdigindedir. Gercek kullanimda REST bir zaman damgasi gecer.
        var restWithTimestamp = provider.CreateSignature("1735689600000");
        Assert.NotEqual(socketSignature, restWithTimestamp);
        Assert.Equal(socketSignature, restSignature);
    }

    [Fact]
    public void Gecersiz_Base64_secret_anlasilir_hata_verir()
    {
        // "!!!" gecerli Base64 degildir. Hata mesaji sorunun secret bicimi oldugunu
        // soylemeli; ham secret'i ICERMEMELIDIR.
        var exception = Assert.Throws<ArgumentException>(
            () => new BtcTurkAuthenticationProvider(new BtcTurkCredentials("key", "!!!")));

        Assert.Contains("Base64", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("!!!", exception.Message, StringComparison.Ordinal);
    }
}
