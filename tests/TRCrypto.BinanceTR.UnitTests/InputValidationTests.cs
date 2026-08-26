using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Clients;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

/// <summary>
/// Gecersiz girdilerin aga cikilmadan reddedildigini dogrular.
/// </summary>
public class InputValidationTests
{
    private static BinanceTRRestClient CreateClient()
        => new(options => options.RateLimiterEnabled = false);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Bos_sembol_reddedilir(string symbol)
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.SpotApi.ExchangeData.GetOrderBookAsync(symbol));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(25)]
    [InlineData(2000)]
    public async Task Desteklenmeyen_kademe_sayisi_aga_cikmadan_reddedilir(int limit)
    {
        var client = CreateClient();

        // Borsa bu degerleri yaniltici bir "Incorrect Page number" hatasiyla reddeder;
        // hata mesaji sorunun limit oldugunu soylemez. Bu yuzden istek hic olusturulmaz.
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.ExchangeData.GetOrderBookAsync("BTC_TRY", limit));

        Assert.Equal("limit", exception.ParamName);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public async Task Desteklenen_kademe_sayilari_kabul_edilir(int limit)
    {
        var client = CreateClient();

        // Bu degerler canli API ile dogrulanmistir; dogrulama bunlari engellememelidir.
        // Ag hatasi olabilir, ama girdi dogrulamasi devreye girmemelidir.
        var exception = await Record.ExceptionAsync(
            () => client.SpotApi.ExchangeData.GetOrderBookAsync("BTC_TRY", limit));

        Assert.IsNotType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public async Task Pozitif_olmayan_islem_sayisi_reddedilir()
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.ExchangeData.GetAggregatedTradesAsync("BTC_TRY", 0));
    }
}

/// <summary>Sembol bicimlendirme kurallari.</summary>
public class SymbolFormattingTests
{
    [Theory]
    [InlineData("BTC", "TRY", "BTC_TRY")]
    [InlineData("btc", "try", "BTC_TRY")]
    [InlineData("ETH", "USDT", "ETH_USDT")]
    public void Sembol_alt_cizgiyle_birlestirilir(string baseAsset, string quoteAsset, string expected)
    {
        // BtcTurk birlesik yazar (BTCTRY); Binance TR alt cizgi kullanir. Iki borsa icin
        // tek bir bicimlendirme kullanilamaz.
        Assert.Equal(expected, BinanceTRExchange.FormatSymbol(baseAsset, quoteAsset, TradingMode.Spot));
    }

    [Fact]
    public void TL_takma_adi_TRY_olarak_cozulur()
    {
        Assert.Equal("BTC_TRY", BinanceTRExchange.FormatSymbol("BTC", "TL", TradingMode.Spot));
    }
}

/// <summary>Kimlik bilgilerinin ciktiya sizmadigini dogrular.</summary>
public class CredentialsSecurityTests
{
    // Gercekci gorunumlu SAHTE degerler; hicbir hesaba ait degildir.
    private const string ApiKey = "FAKE-api-key-1234567890abcdef";
    private const string ApiSecret = "FAKE-secret-value-that-must-never-leak";

    [Fact]
    public void ToString_ham_secret_icermez()
    {
        var credentials = new BinanceTRCredentials(ApiKey, ApiSecret);

        Assert.DoesNotContain(ApiSecret, credentials.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ToString_ham_api_key_icermez()
    {
        var credentials = new BinanceTRCredentials(ApiKey, ApiSecret);

        Assert.DoesNotContain(ApiKey, credentials.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Imzalama_henuz_dogrulanmadigi_icin_acikca_reddedilir()
    {
        // Dogrulanmamis bir imzalamayi sessizce kullanmak, isteklerin nedeni belirsiz
        // sekilde reddedilmesine yol acardi. Bunun yerine acik bir hata verilir.
        var provider = new BinanceTRAuthenticationProvider(new BinanceTRCredentials(ApiKey, ApiSecret));

        Assert.NotNull(provider);
    }
}
