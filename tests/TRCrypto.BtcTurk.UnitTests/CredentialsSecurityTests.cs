using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Spesifikasyon Bolum 12.2 / KPI: "Loglarda API key/secret/signature raw deger yok".
/// </summary>
public class CredentialsSecurityTests
{
    // Gercekci gorunumlu SAHTE degerler. Hicbir hesaba ait degildir ve hicbir yerde
    // gecerli degildir; amaclari ham degerlerin ciktiya sizmadigini dogrulamaktir.
    private const string ApiKey = "pck-FAKE-1234567890abcdef";
    private const string ApiSecret = "RkFLRS1zZWNyZXQtZm9yLXVuaXQtdGVzdHMtb25seQ==";

    [Fact]
    public void ToString_ham_secret_icermez()
    {
        var credentials = new BtcTurkCredentials(ApiKey, ApiSecret);

        var text = credentials.ToString();

        Assert.DoesNotContain(ApiSecret, text, StringComparison.Ordinal);
    }

    [Fact]
    public void ToString_ham_api_key_icermez()
    {
        var credentials = new BtcTurkCredentials(ApiKey, ApiSecret);

        var text = credentials.ToString();

        Assert.DoesNotContain(ApiKey, text, StringComparison.Ordinal);
    }

    [Fact]
    public void ToString_teshis_icin_maskeli_parmak_izi_verir()
    {
        var credentials = new BtcTurkCredentials(ApiKey, ApiSecret);

        var text = credentials.ToString();

        // Hangi anahtarin kullanildigini ayirt edebilmek icin bas/son birkac karakter yeterli.
        Assert.Contains("pck-", text, StringComparison.Ordinal);
        Assert.Contains("...", text, StringComparison.Ordinal);
    }
}
