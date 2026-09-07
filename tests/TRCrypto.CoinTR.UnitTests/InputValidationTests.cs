using TRCrypto.CoinTR.Clients;
using Xunit;

namespace TRCrypto.CoinTR.UnitTests;

/// <summary>
/// Gecersiz girdilerin aga cikmadan reddedildigini dogrular.
/// </summary>
public class InputValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Bos_sembol_reddedilir(string symbol)
    {
        using var client = new CoinTRRestClient();

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.SpotApi.ExchangeData.GetOrderBookAsync(symbol));

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.SpotApi.ExchangeData.GetTradesAsync(symbol));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task Desteklenmeyen_kademe_sayisi_reddedilir(int levels)
    {
        // Kademe sayisi kanal adinin parcasidir (books5). Listede olmayan bir deger icin
        // borsa kanali tanimaz: abonelik onay almaz ve veri gelmez. Hata cikmadigi icin
        // dogrulama istemcide yapilir.
        using var client = new CoinTRSocketClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.SubscribeToOrderBookUpdatesAsync("BTCTRY", levels, _ => { }));
    }

    [Fact]
    public async Task Akis_aboneliginde_bos_sembol_reddedilir()
    {
        using var client = new CoinTRSocketClient();

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.SpotApi.SubscribeToTickerUpdatesAsync("", _ => { }));

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.SpotApi.SubscribeToTradeUpdatesAsync("  ", _ => { }));
    }
}
