using CryptoExchange.Net.Objects;
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
    public async Task V2_arayuzu_uzerinden_gecersiz_istek_aga_cikmadan_reddedilir()
    {
        // CryptoExchange.Net 13 istek dogrulamasini yalnizca V2 yetenek arayuzu uzerinden
        // yapiyor. V2 uygulanmasaydi kademe siniri gibi borsaya ozgu kontroller sessizce
        // duserdi ve gecersiz istek borsaya giderdi. Bu test V2 yolunun da dogrulamadan
        // gectigini ve istegin aga hic cikmadigini sabitler: ag hatasi degil, arguman
        // hatasi beklenir.
        IGetOrderBookRest shared = CreateRestClient().SpotApi.SharedClient;

        var result = await shared.GetOrderBookAsync(
            new GetOrderBookRequest(new SharedSymbol(TradingMode.Spot, "BTC", "TRY"), 2000),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.IsType<ArgumentError>(result.Error);

        // Hatanin turu yetmez, nedeni de dogru olmali. Bu test once yalnizca turu
        // kontrol ediyordu ve secenekler kaydedilmemisken de gecti: istek yine
        // reddediliyordu, ama kademe siniri yuzunden degil, hicbir islem turu
        // desteklenmiyor gorundugu icin.
        Assert.Contains("Max limit", result.Error!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Gecerli_istek_V2_dogrulamasindan_gecer()
    {
        // Ters yonun kaniti: gecerli bir istek dogrulamadan hatasiz gecmeli. Secenekler
        // kaydedilmeden once tum paylasilan cagrilar "TradingMode.Spot is not supported"
        // ile reddediliyordu. Dogrulama dogrudan cagrilir; aga cikilmaz.
        IGetOrderBookRest shared = CreateRestClient().SpotApi.SharedClient;

        var error = shared.GetOrderBookOptions.ValidateRequest(
            new GetOrderBookRequest(new SharedSymbol(TradingMode.Spot, "BTC", "TRY"), 10), shared);

        Assert.Null(error);
    }

    [Fact]
    public void REST_yetenek_listesi_uygulanan_arayuzlerle_birebir_ortusur()
        => CapabilityAssert.MatchesImplementedInterfaces(
            CreateRestClient().SpotApi.SharedClient, SharedTransport.Rest);

    [Fact]
    public void Socket_yetenek_listesi_uygulanan_arayuzlerle_birebir_ortusur()
        => CapabilityAssert.MatchesImplementedInterfaces(
            CreateSocketClient().SpotApi.SharedClient, SharedTransport.Socket);

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
