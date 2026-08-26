using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Clients;
using Xunit;
using Xunit.Abstractions;

namespace TRCrypto.BinanceTR.IntegrationTests;

/// <summary>
/// Borsanin belgelenmemis davranislarini canli olarak sabitler.
/// </summary>
/// <remarks>
/// Bu davranislar kutuphanenin tasarim kararlarinin gerekcesidir. Borsa bunlari
/// duzeltirse kararlarin yeniden gozden gecirilmesi gerekir; test bu degisikligi
/// sessizce gecmek yerine bildirir.
/// <para>
/// Kimlik bilgisi gerektirmez.
/// </para>
/// </remarks>
public class PublicEndpointBehaviourTests
{
    private readonly ITestOutputHelper _output;

    public PublicEndpointBehaviourTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Toplulastirilmis_islemler_veri_dondurur()
    {
        var client = new BinanceTRRestClient();

        var result = await client.SpotApi.ExchangeData.GetAggregatedTradesAsync("BTC_TRY", limit: 10);

        Assert.True(result.Success, result.Error?.ToString());

        // Ayrintili islem ucu bos donduğu icin shared yuzey bu ucu kullanir; veri
        // gelmiyorsa o karar gecersiz kalir.
        Assert.NotEmpty(result.Data.Trades);
        _output.WriteLine($"Toplulastirilmis islem sayisi: {result.Data.Trades.Count}");
    }

    [Fact]
    public async Task Emir_defteri_desteklenmeyen_kademe_sayisini_aga_cikmadan_reddeder()
    {
        var client = new BinanceTRRestClient();

        // Borsa yalnizca sabit degerleri kabul eder ve reddederken sorunun limit oldugunu
        // soylemez; hata mesaji sayfa numarasindan bahseder. Desteklenmeyen bir deger
        // cagiran tarafin hatasidir, borsanin degil; bu yuzden istisna uretilir.
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.ExchangeData.GetOrderBookAsync("BTC_TRY", limit: 7));

        Assert.Equal("limit", exception.ParamName);
        _output.WriteLine(exception.Message);
    }

    [Fact]
    public async Task Shared_yuzey_desteklenmeyen_kademe_sayisini_yuvarlar()
    {
        var shared = (IOrderBookRestClient)new BinanceTRRestClient().SpotApi.SharedClient;

        // Borsadan bagimsiz yuzeyde cagiran taraf borsanin sabit degerlerini bilemez;
        // istenen deger desteklenen en yakin ust degere yuvarlanir.
        var result = await shared.GetOrderBookAsync(
            new GetOrderBookRequest(new SharedSymbol(TradingMode.Spot, "BTC", "TRY"), 7));

        Assert.True(result.Success, result.Error?.ToString());
        Assert.Equal(10, result.Data.Bids.Length);
        _output.WriteLine($"7 kademe istendi, {result.Data.Bids.Length} kademe geldi");
    }

    [Fact]
    public async Task Pariteler_alt_cizgili_bicimde_gelir()
    {
        var client = new BinanceTRRestClient();

        var result = await client.SpotApi.ExchangeData.GetSymbolsAsync();

        Assert.True(result.Success, result.Error?.ToString());
        Assert.NotEmpty(result.Data.Symbols);

        var tryPairs = result.Data.Symbols.Where(x => x.QuoteAsset == "TRY").ToList();
        Assert.NotEmpty(tryPairs);

        // Sembol bicimi kutuphanenin FormatSymbol kararinin dayanagidir.
        Assert.All(tryPairs, x => Assert.Contains('_', x.Name));
        _output.WriteLine($"Toplam parite: {result.Data.Symbols.Count}, TRY paritesi: {tryPairs.Count}");
    }
}
