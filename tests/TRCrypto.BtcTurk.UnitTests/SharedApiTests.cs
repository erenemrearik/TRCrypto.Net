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
    public void Henuz_uygulanmayan_arayuzler_bildirilmez()
    {
        var shared = CreateClient().SpotApi.SharedClient;

        // Uygulanmamis bir arayuzu bildirmek, Discover() ciktisini yaniltici hale getirir.
        Assert.False(shared is ISpotOrderRestClient);
        Assert.False(shared is IKlineRestClient);
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
