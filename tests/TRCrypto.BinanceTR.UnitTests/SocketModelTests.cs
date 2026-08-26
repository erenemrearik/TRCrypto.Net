using System.Text.Json;
using TRCrypto.BinanceTR.Enums;
using TRCrypto.BinanceTR.Objects.Internal;
using TRCrypto.BinanceTR.Objects.Models.Socket;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

/// <summary>
/// WebSocket mesaj modellerini dogrular.
/// </summary>
/// <remarks>
/// Fixture'lar canli bir baglantidan alinmistir. Mesaj bicimi global Binance ile aynidir:
/// tek bir JSON nesnesi, olay turu <c>e</c> alaninda.
/// </remarks>
public class SocketModelTests
{
    private static T Parse<T>(string fixture)
        => JsonSerializer.Deserialize<T>(FixtureLoader.Load(fixture), BinanceTRJsonOptions.Default)!;

    [Fact]
    public void Ticker_alanlari_okunur()
    {
        var ticker = Parse<BinanceTRStreamTicker>("socket-ticker.json");

        Assert.Equal("BTCTRY", ticker.Symbol);
        Assert.Equal(3785106m, ticker.LastPrice);
        Assert.Equal(3785105m, ticker.BestBidPrice);
        Assert.Equal(3785106m, ticker.BestAskPrice);
        Assert.Equal(3809300m, ticker.HighPrice);
        Assert.Equal(3755729m, ticker.LowPrice);
        Assert.Equal(0.624m, ticker.ChangePercentage);
        Assert.Equal(25.96219002m, ticker.Volume);
    }

    [Fact]
    public void Ticker_yanitindaki_sembol_alt_cizgisizdir()
    {
        // Abonelik "btctry" ile yapilir, yanit "BTCTRY" doner, REST ise "BTC_TRY" kullanir.
        var ticker = Parse<BinanceTRStreamTicker>("socket-ticker.json");

        Assert.DoesNotContain('_', ticker.Symbol);
        Assert.Equal(ticker.Symbol.ToUpperInvariant(), ticker.Symbol);
    }

    [Fact]
    public void Tekil_islem_okunur()
    {
        var trade = Parse<BinanceTRStreamTrade>("socket-trade.json");

        Assert.Equal(80020071, trade.Id);
        Assert.Equal(3784348m, trade.Price);
        Assert.Equal(0.00101000m, trade.Quantity);
        Assert.Equal(DateTimeKind.Utc, trade.Timestamp.Kind);
    }

    [Fact]
    public void Islem_yonu_alici_piyasa_yapici_alanindan_turetilir()
    {
        // Akislar yonu dogrudan vermez. Alici piyasa yapiciysa islemi baslatan saticidir.
        var trade = Parse<BinanceTRStreamTrade>("socket-trade.json");
        var aggregated = Parse<BinanceTRStreamAggregatedTrade>("socket-agg-trade.json");

        Assert.False(trade.BuyerIsMaker);
        Assert.Equal(OrderSide.Buy, trade.Side);

        Assert.True(aggregated.BuyerIsMaker);
        Assert.Equal(OrderSide.Sell, aggregated.Side);
    }

    [Fact]
    public void Emir_defteri_farki_sira_araligini_tasir()
    {
        var update = Parse<BinanceTRStreamOrderBookUpdate>("socket-depth.json");

        // U ve u, bu guncellemenin kapsadigi ilk ve son sira numarasidir; atlama
        // tespiti bunlarsiz yapilamaz.
        Assert.Equal(5893366894, update.FirstUpdateId);
        Assert.Equal(5893366934, update.LastUpdateId);
        Assert.Equal("BTCTRY", update.Symbol);
    }

    [Fact]
    public void Sifir_miktarli_kademe_silme_anlamina_gelir()
    {
        var update = Parse<BinanceTRStreamOrderBookUpdate>("socket-depth.json");

        // Miktari sifir olan kademe, o fiyat seviyesinin defterden kaldirildigini bildirir.
        var removed = update.Bids.Single(x => x.Quantity == 0);
        Assert.Equal(3785232m, removed.Price);
    }

    [Fact]
    public void Emir_defteri_goruntusu_okunur()
    {
        // Bu akis olay turu alani tasimaz; yalin bir goruntudur.
        var book = Parse<BinanceTRStreamOrderBook>("socket-book.json");

        Assert.Equal(5893367072, book.LastUpdateId);
        Assert.Equal(2, book.Bids.Count);
        Assert.True(book.Bids[0].Price < book.Asks[0].Price);
    }

    [Fact]
    public void Mum_okunur()
    {
        var update = Parse<BinanceTRStreamKlineUpdate>("socket-kline.json");
        var kline = update.Kline;

        Assert.Equal("BTCTRY", update.Symbol);
        Assert.Equal("1m", kline.Interval);
        Assert.Equal(3785105m, kline.OpenPrice);
        Assert.Equal(3784054m, kline.ClosePrice);
        Assert.Equal(0.01253000m, kline.Volume);
        Assert.Equal(18, kline.TradeCount);
    }

    [Fact]
    public void Kapanmamis_mum_isaretlenir()
    {
        // Kapanmamis bir mumun degerleri degismeye devam eder; tuketicinin bunu
        // ayirt edebilmesi gerekir.
        var kline = Parse<BinanceTRStreamKlineUpdate>("socket-kline.json").Kline;

        Assert.False(kline.Closed);
        Assert.Equal(DateTimeKind.Utc, kline.OpenTime.Kind);
        Assert.True(kline.CloseTime > kline.OpenTime);
    }
}
