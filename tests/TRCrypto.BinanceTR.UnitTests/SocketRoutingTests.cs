using System.Text;
using TRCrypto.BinanceTR.Clients.MessageHandlers;
using TRCrypto.BinanceTR.Clients.SpotApi;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

/// <summary>
/// Akis mesajlarinin dogru abonelige yonlendirildigini dogrular.
/// </summary>
/// <remarks>
/// Bu testler gercek bir hatayi yakalar: yonlendirme kimligi baglantiya bagli bir alanda
/// tutuldugunda, ayni istemci uzerinden acilan her yeni abonelik oncekinin kimligini
/// eziyordu ve yalnizca en son abonelik mesaj aliyordu. Digerleri sessizce bos kaliyordu;
/// hata yok, veri yok. Kimlik artik mesajin kendisinden turetiliyor.
/// </remarks>
public class SocketRoutingTests
{
    private static string? TypeIdentifierOf(string message)
    {
        var handler = new BinanceTRSocketMessageHandler();
        return handler.GetTypeIdentifier(Encoding.UTF8.GetBytes(message), null);
    }

    [Theory]
    [InlineData("""{"e":"24hrTicker","s":"BTCTRY"}""", "24hrTicker")]
    [InlineData("""{"e":"trade","s":"BTCTRY"}""", "trade")]
    [InlineData("""{"e":"aggTrade","s":"BTCTRY"}""", "aggTrade")]
    [InlineData("""{"e":"depthUpdate","s":"BTCTRY"}""", "depthUpdate")]
    [InlineData("""{"e":"kline","s":"BTCTRY"}""", "kline")]
    public void Olay_turu_tasiyan_mesajlar_turlerine_gore_yonlendirilir(string message, string expected)
    {
        Assert.Equal(expected, TypeIdentifierOf(message));
    }

    [Fact]
    public void Olay_turu_tasimayan_goruntu_mesaji_da_yonlendirilir()
    {
        // Kismi emir defteri goruntusu "e" alani tasimaz; sira numarasiyla taninir.
        const string snapshot = """{"lastUpdateId":5893367072,"bids":[],"asks":[]}""";

        Assert.Equal(BinanceTRSocketMessageHandler.SnapshotIdentifier, TypeIdentifierOf(snapshot));
    }

    [Fact]
    public void Farkli_mesajlar_farkli_kimlik_uretir()
    {
        // Ayni istemci uzerinden birden fazla akisa abone olundugunda her mesajin
        // kendi aboneligine gitmesi buna baglidir.
        var ticker = TypeIdentifierOf("""{"e":"24hrTicker","s":"BTCTRY"}""");
        var depth = TypeIdentifierOf("""{"e":"depthUpdate","s":"BTCTRY"}""");
        var snapshot = TypeIdentifierOf("""{"lastUpdateId":1,"bids":[],"asks":[]}""");

        Assert.NotEqual(ticker, depth);
        Assert.NotEqual(ticker, snapshot);
        Assert.NotEqual(depth, snapshot);
    }

    [Theory]
    [InlineData("BTC_TRY", "btctry")]
    [InlineData("btc_try", "btctry")]
    [InlineData("BTCTRY", "btctry")]
    [InlineData("ETH_USDT", "ethusdt")]
    public void Akis_sembolu_kucuk_harf_ve_alt_cizgisiz_uretilir(string input, string expected)
    {
        // Yanlis bicim hata uretmez; baglanti kurulur ama hicbir mesaj gelmez.
        // Bu yuzden donusum testle sabitlenmistir.
        Assert.Equal(expected, BinanceTRSocketClientSpotApi.ToStreamSymbol(input));
    }
}
