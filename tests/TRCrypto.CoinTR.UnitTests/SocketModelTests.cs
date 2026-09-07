using System.Text.Json;
using TRCrypto.CoinTR.Enums;
using TRCrypto.CoinTR.Objects.Internal;
using TRCrypto.CoinTR.Objects.Models.Socket;
using TRCrypto.CoinTR.Objects.Sockets;
using Xunit;

namespace TRCrypto.CoinTR.UnitTests;

/// <summary>Akis govdelerinin dogru cozumlendigini dogrular.</summary>
/// <remarks>
/// Akis govdesi REST karsiligindan farkli alan adlari kullanir: parite adi <c>symbol</c>
/// degil <c>instId</c>, acilis fiyati <c>open</c> degil <c>open24h</c> alanindadir.
/// </remarks>
public class SocketModelTests
{
    private static CoinTRStreamUpdate<T> Parse<T>(string fixture)
        => JsonSerializer.Deserialize<CoinTRStreamUpdate<T>>(
            FixtureLoader.Load(fixture), CoinTRJsonOptions.Default)!;

    [Fact]
    public void Ticker_guncellemesi_cozumlenir()
    {
        var update = Parse<CoinTRStreamTicker>("socket-ticker.json");

        Assert.Equal("snapshot", update.Action);
        Assert.Equal("ticker", update.Argument.Channel);
        Assert.Equal("BTCTRY", update.Argument.InstrumentId);

        // Govde tek ogeli olsa bile her zaman dizidir.
        var ticker = Assert.Single(update.Data);
        Assert.Equal("BTCTRY", ticker.Symbol);
        Assert.Equal(3830046m, ticker.LastPrice);
        Assert.Equal(3810376m, ticker.OpenPrice);
        Assert.Equal(3901118m, ticker.HighPrice);
        Assert.Equal(3800024m, ticker.LowPrice);
        Assert.Equal(13.72871m, ticker.Volume);
        Assert.Equal(52468521.90m, ticker.QuoteVolume);
    }

    [Fact]
    public void Akis_degisim_orani_da_yuzdeye_cevrilir()
    {
        var ticker = Parse<CoinTRStreamTicker>("socket-ticker.json").Data[0];

        Assert.Equal(-0.00912m, ticker.ChangeRatio);
        Assert.Equal(-0.912m, ticker.ChangePercentage);
    }

    [Fact]
    public void Emir_defteri_goruntusu_cozumlenir()
    {
        var update = Parse<CoinTRStreamOrderBook>("socket-books.json");

        Assert.Equal("books5", update.Argument.Channel);

        var book = Assert.Single(update.Data);
        Assert.Equal(3830901m, book.Asks[0].Price);
        Assert.Equal(0.04752m, book.Asks[0].Quantity);
        Assert.Equal(3829767m, book.Bids[0].Price);
        Assert.Equal(0.05552m, book.Bids[0].Quantity);
    }

    [Fact]
    public void Islem_guncellemesi_cozumlenir()
    {
        var update = Parse<CoinTRStreamTrade>("socket-trade.json");

        Assert.Equal("update", update.Action);

        var trade = Assert.Single(update.Data);
        Assert.Equal("1480847216137556071", trade.TradeId);
        Assert.Equal(OrderSide.Sell, trade.Side);
        Assert.Equal(3847741m, trade.Price);
        Assert.Equal(0.00009m, trade.Quantity);
    }

    [Fact]
    public void Abonelik_onayi_cozumlenir()
    {
        var ack = JsonSerializer.Deserialize<CoinTRSubscribeResponse>(
            FixtureLoader.Load("socket-subscribe-ack.json"), CoinTRJsonOptions.Default)!;

        Assert.Equal("subscribe", ack.Event);
        Assert.NotNull(ack.Argument);
        Assert.Equal("ticker", ack.Argument!.Channel);
        Assert.Equal("BTCTRY", ack.Argument.InstrumentId);
        Assert.Null(ack.Message);
    }

    [Fact]
    public void Abonelik_istegi_borsanin_bekledigi_bicimde_uretilir()
    {
        // Yanlis bicimlenmis bir istek hata dondurmez; baglanti kurulur ve hicbir mesaj
        // gelmez. Bu yuzden govde testle sabitlenmistir.
        var request = new CoinTRSubscribeRequest
        {
            Operation = "subscribe",
            Arguments =
            [
                new CoinTRStreamArgument
                {
                    InstrumentType = CoinTRSubscribeRequest.SpotInstrumentType,
                    Channel = "ticker",
                    InstrumentId = "BTCTRY"
                }
            ]
        };

        var json = JsonSerializer.Serialize(request, CoinTRJsonOptions.Default);

        Assert.Equal(
            """{"op":"subscribe","args":[{"instType":"SPOT","channel":"ticker","instId":"BTCTRY"}]}""",
            json);
    }
}
