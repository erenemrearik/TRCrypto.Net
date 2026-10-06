using CryptoExchange.Net.Objects;
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
    public void SharedClient_ve_SharedApi_uyelerine_dogrudan_erisilebilir()
    {
        // Bu test derleme zamaninda calisir: derlendigi surece kirilma yoktur.
        //
        // 0.1.0'da kullanicilar SharedClient uzerinden dogrudan cagri yapabiliyordu
        // (client.SpotApi.SharedClient.GetOrderBookAsync). V2 arayuzleri ilk once V1
        // toplu arayuzune eklendi; V1 ve V2 ayni adli uyeler tasidigi icin cagri
        // belirsizlesti ve bu kod derlenmez oldu. Yayindan hemen once fark edildi; V1
        // ve V2 artik ayri arayuzlerdir. Biri tekrar birlestirirse bu test derlenmez.
        var client = CreateRestClient();

        Func<GetOrderBookRequest, CancellationToken, Task<HttpResult<SharedOrderBook>>> v1
            = client.SpotApi.SharedClient.GetOrderBookAsync;
        Func<GetOrderBookRequest, CancellationToken, Task<HttpResult<SharedOrderBook>>> v2
            = client.SpotApi.SharedApi.GetOrderBookAsync;

        Assert.NotNull(v1);
        Assert.NotNull(v2);

        // Iki gorunum ayni yapilandirmayi paylasir; ayri nesneler dogrulama kurallarini
        // ikiye bolerdi.
        Assert.Same(
            client.SpotApi.SharedClient.GetOrderBookOptions,
            client.SpotApi.SharedApi.GetOrderBookOptions);
    }

    [Fact]
    public async Task V2_arayuzu_uzerinden_gecersiz_istek_aga_cikmadan_reddedilir()
    {
        // CryptoExchange.Net 13 istek dogrulamasini yalnizca V2 yetenek arayuzu uzerinden
        // yapiyor. V2 uygulanmasaydi kademe siniri gibi borsaya ozgu kontroller sessizce
        // duserdi ve gecersiz istek borsaya giderdi. Bu test V2 yolunun da dogrulamadan
        // gectigini ve istegin aga hic cikmadigini sabitler: ag hatasi degil, arguman
        // hatasi beklenir.
        IGetOrderBookRest shared = CreateRestClient().SpotApi.SharedApi;

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
        IGetOrderBookRest shared = CreateRestClient().SpotApi.SharedApi;

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
    public void Kullanici_akisi_yetenek_listesi_uygulanan_arayuzlerle_birebir_ortusur()
        => CapabilityAssert.MatchesImplementedInterfaces(
            CreateSocketClient().UserApi.SharedClient, SharedTransport.Socket);

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

    [Fact]
    public void Kullanici_akisi_shared_arayuzleri_uygulanir()
    {
        var shared = CreateSocketClient().UserApi.SharedClient;

        Assert.IsAssignableFrom<IBalanceSocketClient>(shared);
        Assert.IsAssignableFrom<ISpotOrderSocketClient>(shared);
    }

    [Fact]
    public async Task Dinleme_tokeni_verilmezse_abonelik_kurulmaz()
    {
        // Kimlik bilgisi once denetlenir; token kontrolune ulasmak icin istemcinin
        // kimlik bilgisi tanimli olmalidir. Degerler gercekci gorunumlu ve SAHTEDIR.
        var client = new BinanceTRSocketClient(options =>
            options.ApiCredentials = new BinanceTRCredentials("anahtar", "gizli"));
        var shared = (IBalanceSocketClient)client.UserApi.SharedClient;

        // Token olmadan baglanmayi denemek, sunucunun sessizce hicbir sey gondermemesiyle
        // sonuclanirdi. Eksiklik aga cikmadan ve acik bir mesajla bildirilir.
        var result = await shared.SubscribeToBalanceUpdatesAsync(
            new SubscribeBalancesRequest(), _ => { });

        Assert.False(result.Success);
        Assert.Contains("token", result.Error!.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
