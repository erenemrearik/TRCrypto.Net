using CryptoExchange.Net.SharedApis;
using TRCrypto.CoinTR.Clients;
using Xunit;

namespace TRCrypto.CoinTR.UnitTests;

/// <summary>
/// Borsadan bagimsiz yuzeyin dogru tanimlandigini dogrular.
/// </summary>
/// <remarks>
/// Bu testler ag erisimi yapmaz; arayuz uygulamalarini ve bildirilen yetenekleri inceler.
/// </remarks>
public class SharedApiTests
{
    private static CoinTRRestClient CreateRestClient()
        => new(options => options.RateLimiterEnabled = false);

    private static CoinTRSocketClient CreateSocketClient() => new();

    [Fact]
    public void REST_shared_arayuzleri_uygulanir()
    {
        var shared = CreateRestClient().SpotApi.SharedClient;

        Assert.IsAssignableFrom<ISpotSymbolRestClient>(shared);
        Assert.IsAssignableFrom<ISpotTickerRestClient>(shared);
        Assert.IsAssignableFrom<IOrderBookRestClient>(shared);
        Assert.IsAssignableFrom<IRecentTradeRestClient>(shared);
        Assert.IsAssignableFrom<IKlineRestClient>(shared);
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
    public void Dogrulanmamis_arayuzler_bildirilmez()
    {
        var rest = CreateRestClient().SpotApi.SharedClient;

        // Bakiye ve emir uclari canli bir hesapla denenmedi. Uygulanmamis bir arayuzu
        // bildirmek Discover() ciktisini yaniltici hale getirirdi.
        Assert.False(rest is IBalanceRestClient);
        Assert.False(rest is ISpotOrderRestClient);

        // Vadeli islemler kapsam disidir.
        Assert.False(rest is IFuturesSymbolRestClient);
        Assert.False(rest is IFuturesOrderRestClient);
    }

    [Fact]
    public void Discover_borsayi_ve_islem_turunu_bildirir()
    {
        var shared = CreateRestClient().SpotApi.SharedClient;

        var info = shared.Discover();

        Assert.Equal(CoinTRExchange.ExchangeName, info.Exchange);
        Assert.Equal([TradingMode.Spot], shared.SupportedTradingModes);
    }

    [Theory]
    [InlineData("BTC", "TRY", "BTCTRY")]
    [InlineData("ETH", "USDT", "ETHUSDT")]
    [InlineData("btc", "try", "BTCTRY")]
    public void SharedSymbol_native_sembole_cevrilir(string baseAsset, string quoteAsset, string expected)
    {
        var client = CreateRestClient();
        var symbol = new SharedSymbol(TradingMode.Spot, baseAsset, quoteAsset);

        // Cagiran taraf native bicimi hic gormez; her borsa kendi bicimini uretir.
        // CoinTR birlesik ve buyuk harf yazar; Binance TR alt cizgi kullanir.
        Assert.Equal(expected, symbol.GetSymbol(client.SpotApi.FormatSymbol));
    }

    [Fact]
    public void Turk_lirasi_takma_adi_kanonik_ada_cevrilir()
    {
        // Turkiye kaynaklari zaman zaman TL yazar; borsa her zaman TRY bekler.
        Assert.Equal("BTCTRY", CoinTRExchange.FormatSymbol("BTC", "TL", TradingMode.Spot));
        Assert.Equal("TRY", CoinTRExchange.NormalizeAsset("tl"));
    }

    [Fact]
    public void Emir_defteri_kademe_secenekleri_borsa_sinirini_yansitir()
    {
        var shared = (IOrderBookRestClient)CreateRestClient().SpotApi.SharedClient;

        Assert.Equal(1, shared.GetOrderBookOptions.MinLimit);
        Assert.Equal(150, shared.GetOrderBookOptions.MaxLimit);
    }

    [Fact]
    public void Akis_emir_defteri_yalnizca_kanal_adinda_gecen_kademeleri_sunar()
    {
        var shared = (IOrderBookSocketClient)CreateSocketClient().SpotApi.SharedClient;

        // Kademe sayisi kanal adinin parcasidir; ara degerler icin kanal yoktur.
        Assert.Equal([1, 5, 15], shared.SubscribeOrderBookOptions.SupportedLimits);
    }

    [Fact]
    public void Ticker_akisi_yirmi_dort_saatlik_ozeti_bildirir()
    {
        var shared = (ITickerSocketClient)CreateSocketClient().SpotApi.SharedClient;

        Assert.Equal(SharedTickerType.Day24H, shared.SubscribeTickerOptions.TickerType);
    }
}
