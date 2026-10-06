using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk.Clients;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Borsadan bagimsiz (shared) yuzeyin dogru tanimlandigini dogrular.
/// </summary>
/// <remarks>
/// Bu testler ag erisimi yapmaz; yalnizca arayuz uygulamalarini ve sembol donusumunu
/// inceler. Uctan uca davranis ornek uygulamada canli API'ye karsi dogrulanir.
/// </remarks>
public class SharedApiTests
{
    private static BtcTurkRestClient CreateClient()
        => new(options => options.RateLimiterEnabled = false);

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
        var client = CreateClient();

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
        IGetOrderBookRest shared = CreateClient().SpotApi.SharedApi;

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
        IGetOrderBookRest shared = CreateClient().SpotApi.SharedApi;

        var error = shared.GetOrderBookOptions.ValidateRequest(
            new GetOrderBookRequest(new SharedSymbol(TradingMode.Spot, "BTC", "TRY"), 10), shared);

        Assert.Null(error);
    }

    [Fact]
    public void REST_yetenek_listesi_uygulanan_arayuzlerle_birebir_ortusur()
        => CapabilityAssert.MatchesImplementedInterfaces(
            CreateClient().SpotApi.SharedClient, SharedTransport.Rest);

    [Fact]
    public void Socket_yetenek_listesi_uygulanan_arayuzlerle_birebir_ortusur()
        => CapabilityAssert.MatchesImplementedInterfaces(
            new BtcTurkSocketClient().SpotApi.SharedClient, SharedTransport.Socket);

    [Fact]
    public void Piyasa_verisi_shared_arayuzleri_uygulanir()
    {
        var shared = CreateClient().SpotApi.SharedClient;

        Assert.IsAssignableFrom<ISpotSymbolRestClient>(shared);
        Assert.IsAssignableFrom<ISpotTickerRestClient>(shared);
        Assert.IsAssignableFrom<IOrderBookRestClient>(shared);
        Assert.IsAssignableFrom<IRecentTradeRestClient>(shared);
    }

    [Fact]
    public void Bakiye_shared_arayuzu_uygulanir()
    {
        var shared = CreateClient().SpotApi.SharedClient;

        Assert.IsAssignableFrom<IBalanceRestClient>(shared);
    }

    [Fact]
    public void Emir_ve_mum_shared_arayuzleri_uygulanir()
    {
        var shared = CreateClient().SpotApi.SharedClient;

        Assert.IsAssignableFrom<ISpotOrderRestClient>(shared);
        Assert.IsAssignableFrom<IKlineRestClient>(shared);
    }

    [Fact]
    public void Desteklenmeyen_ozellikler_bildirilmez()
    {
        var shared = (ISpotOrderRestClient)CreateClient().SpotApi.SharedClient;

        // BtcTurk time-in-force secenegi sunmaz; desteklenmeyen bir ozelligi bildirmek
        // Discover() ciktisini yaniltici hale getirirdi.
        Assert.Empty(shared.SpotSupportedTimeInForce);

        // Emir miktari yalnizca base varlik cinsinden verilebilir.
        Assert.Equal(SharedQuantityType.BaseAsset, shared.SpotSupportedOrderQuantity.BuyLimit);
    }

    [Fact]
    public void Vadeli_islem_arayuzleri_uygulanmaz()
    {
        var shared = CreateClient().SpotApi.SharedClient;

        // BtcTurk yalnizca spot islem sunar.
        Assert.False(shared is IFuturesOrderRestClient);
        Assert.False(shared is IFuturesSymbolRestClient);
    }

    [Fact]
    public void Discover_borsayi_ve_desteklenen_islem_turunu_bildirir()
    {
        var shared = CreateClient().SpotApi.SharedClient;

        var info = shared.Discover();

        Assert.Equal(BtcTurkExchange.ExchangeName, info.Exchange);
        Assert.Equal([TradingMode.Spot], shared.SupportedTradingModes);
    }

    [Theory]
    [InlineData("BTC", "TRY", "BTCTRY")]
    [InlineData("ETH", "USDT", "ETHUSDT")]
    public void SharedSymbol_native_sembole_cevrilir(string baseAsset, string quoteAsset, string expected)
    {
        var client = CreateClient();
        var symbol = new SharedSymbol(TradingMode.Spot, baseAsset, quoteAsset);

        // Cagiran taraf native bicimi hic gormez; donusum kutuphane icinde yapilir.
        var native = symbol.GetSymbol(client.SpotApi.FormatSymbol);

        Assert.Equal(expected, native);
    }

    [Fact]
    public void Islem_sayisi_secenegi_borsa_sinirini_yansitir()
    {
        var shared = (IRecentTradeRestClient)CreateClient().SpotApi.SharedClient;

        // BtcTurk bu ucta en fazla 50 kayit dondurur.
        Assert.Equal(50, shared.GetRecentTradesOptions.MaxLimit);
    }
}
