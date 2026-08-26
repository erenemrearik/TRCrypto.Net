using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Clients;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

/// <summary>
/// Borsadan bagimsiz yuzeyin dogru tanimlandigini dogrular.
/// </summary>
/// <remarks>
/// Bu testler ag erisimi yapmaz; arayuz uygulamalarini ve bildirilen yetenekleri inceler.
/// </remarks>
public class SharedApiTests
{
    private static BinanceTRRestClient CreateRestClient()
        => new(options => options.RateLimiterEnabled = false);

    private static BinanceTRSocketClient CreateSocketClient() => new();

    [Fact]
    public void REST_shared_arayuzleri_uygulanir()
    {
        var shared = CreateRestClient().SpotApi.SharedClient;

        Assert.IsAssignableFrom<ISpotSymbolRestClient>(shared);
        Assert.IsAssignableFrom<IOrderBookRestClient>(shared);
        Assert.IsAssignableFrom<IRecentTradeRestClient>(shared);
    }

    [Fact]
    public void REST_ticker_arayuzu_bildirilmez()
    {
        var shared = CreateRestClient().SpotApi.SharedClient;

        // Borsa ticker verisini anahtarsiz sunmuyor; uygulanmamis bir arayuzu bildirmek
        // Discover() ciktisini yaniltici hale getirirdi. Ticker icin socket yuzeyi vardir.
        Assert.False(shared is ISpotTickerRestClient);
    }

    [Fact]
    public void Socket_shared_arayuzleri_uygulanir()
    {
        var shared = CreateSocketClient().SpotApi.SharedClient;

        Assert.IsAssignableFrom<ITickerSocketClient>(shared);
        Assert.IsAssignableFrom<ITradeSocketClient>(shared);
        Assert.IsAssignableFrom<IOrderBookSocketClient>(shared);
    }

    [Fact]
    public void Vadeli_islem_arayuzleri_uygulanmaz()
    {
        var shared = CreateRestClient().SpotApi.SharedClient;

        Assert.False(shared is IFuturesSymbolRestClient);
        Assert.False(shared is IFuturesOrderRestClient);
    }

    [Fact]
    public void Discover_borsayi_ve_islem_turunu_bildirir()
    {
        var shared = CreateRestClient().SpotApi.SharedClient;

        var info = shared.Discover();

        Assert.Equal(BinanceTRExchange.ExchangeName, info.Exchange);
        Assert.Equal([TradingMode.Spot], shared.SupportedTradingModes);
    }

    [Theory]
    [InlineData("BTC", "TRY", "BTC_TRY")]
    [InlineData("ETH", "USDT", "ETH_USDT")]
    public void SharedSymbol_native_sembole_cevrilir(string baseAsset, string quoteAsset, string expected)
    {
        var client = CreateRestClient();
        var symbol = new SharedSymbol(TradingMode.Spot, baseAsset, quoteAsset);

        // Cagiran taraf native bicimi hic gormez; her borsa kendi bicimini uretir.
        Assert.Equal(expected, symbol.GetSymbol(client.SpotApi.FormatSymbol));
    }

    [Fact]
    public void Emir_defteri_kademe_secenekleri_borsa_sinirini_yansitir()
    {
        var shared = (IOrderBookRestClient)CreateRestClient().SpotApi.SharedClient;

        // Borsa en az 5, en fazla 1000 kademe donduruyor.
        Assert.Equal(5, shared.GetOrderBookOptions.MinLimit);
        Assert.Equal(1000, shared.GetOrderBookOptions.MaxLimit);
    }
}
