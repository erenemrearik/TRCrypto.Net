using TRCrypto.BtcTurk.Clients;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Gecersiz girdilerin aga cikmadan reddedildigini dogrular (spesifikasyon Bolum 10.4).
/// </summary>
/// <remarks>
/// Bu testler ag erisimi yapmaz: dogrulama basarisiz oldugunda istek hic olusturulmaz.
/// Dogrulama kaldirilirsa test bir istisna beklerken zaman asimina ugrar ya da
/// beklenmedik bir hata alir; boylece regresyon yakalanir.
/// </remarks>
public class InputValidationTests
{
    private static BtcTurkRestClient CreateClient()
        => new(options => options.RateLimiterEnabled = false);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Bos_sembol_reddedilir(string symbol)
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.SpotApi.ExchangeData.GetTickerAsync(symbol));
    }

    [Fact]
    public async Task Islem_sayisi_borsa_sinirini_asamaz()
    {
        var client = CreateClient();

        // BtcTurk bu ucta en fazla 50 kayit dondurur; daha fazlasi istenirse istek reddedilir.
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.ExchangeData.GetTradesAsync("BTCTRY", 51));

        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async Task Islem_sayisi_sifir_olamaz()
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.ExchangeData.GetTradesAsync("BTCTRY", 0));
    }

    [Fact]
    public async Task Emir_defteri_kademe_sayisi_negatif_olamaz()
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.ExchangeData.GetOrderBookAsync("BTCTRY", -1));
    }

    [Fact]
    public async Task Bos_quote_varlik_reddedilir()
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.SpotApi.ExchangeData.GetTickersByQuoteAssetAsync(""));
    }
}
