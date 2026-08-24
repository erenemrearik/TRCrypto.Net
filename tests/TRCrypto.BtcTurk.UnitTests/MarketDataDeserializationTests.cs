using System.Text.Json;
using TRCrypto.BtcTurk.Enums;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Piyasa verisi yanitlarinin dogru ayristirildigini dogrular.
/// Fixture'lar canli public API'den alinmis gercek yanitlardir.
/// </summary>
public class MarketDataDeserializationTests
{
    private static T Parse<T>(string fixture)
        => JsonSerializer.Deserialize<BtcTurkResponse<T>>(
               FixtureLoader.Load(fixture), BtcTurkJsonOptions.Default)!.Data!;

    [Fact]
    public void Ticker_alanlari_decimal_olarak_okunur()
    {
        var ticker = Parse<IReadOnlyList<BtcTurkTicker>>("ticker.json").Single(x => x.Pair == "BTCTRY");

        Assert.Equal("BTC", ticker.NumeratorSymbol);
        Assert.Equal("TRY", ticker.DenominatorSymbol);
        Assert.True(ticker.LastPrice > 0);
        Assert.True(ticker.BestBidPrice > 0);
        Assert.True(ticker.BestAskPrice > ticker.BestBidPrice);
        Assert.True(ticker.Volume > 0);
    }

    [Fact]
    public void Ticker_zaman_damgasi_milisaniye_olarak_cozulur()
    {
        var ticker = Parse<IReadOnlyList<BtcTurkTicker>>("ticker.json")[0];

        Assert.Equal(DateTimeKind.Utc, ticker.Timestamp.Kind);
        Assert.True(ticker.Timestamp > new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            $"Zaman damgasi saniye olarak yorumlanmis olabilir: {ticker.Timestamp:O}");
    }

    [Fact]
    public void OrderBook_kademeleri_fiyat_ve_miktar_olarak_okunur()
    {
        var book = Parse<BtcTurkOrderBook>("orderbook.json");

        Assert.NotEmpty(book.Bids);
        Assert.NotEmpty(book.Asks);

        // Kaynakta her kademe ["fiyat", "miktar"] bicimindedir; string'ler decimal'e cevrilir.
        Assert.True(book.Bids[0].Price > 0);
        Assert.True(book.Bids[0].Quantity > 0);

        // En iyi alis, en iyi satistan dusuk olmalidir.
        Assert.True(book.Bids[0].Price < book.Asks[0].Price);
    }

    [Fact]
    public void OrderBook_kademeleri_siralamayi_korur()
    {
        var book = Parse<BtcTurkOrderBook>("orderbook.json");

        // Alislar azalan, satislar artan sirada gelir.
        Assert.True(book.Bids[0].Price >= book.Bids[^1].Price);
        Assert.True(book.Asks[0].Price <= book.Asks[^1].Price);
    }

    [Fact]
    public void Trade_alanlari_okunur()
    {
        var trade = Parse<IReadOnlyList<BtcTurkTrade>>("trades.json")[0];

        Assert.Equal("BTCTRY", trade.Pair);
        Assert.Equal("BTC", trade.Numerator);
        Assert.Equal("TRY", trade.Denominator);
        Assert.True(trade.Price > 0);
        Assert.True(trade.Quantity > 0);
        Assert.False(string.IsNullOrEmpty(trade.Id));
        Assert.Equal(DateTimeKind.Utc, trade.Timestamp.Kind);
    }

    [Fact]
    public void Trade_yonu_okunur()
    {
        var trade = Parse<IReadOnlyList<BtcTurkTrade>>("trades.json")[0];

        // Resmi ornek yanitta yoktur ancak canli API bu alani dondurur.
        Assert.True(trade.Side is OrderSide.Buy or OrderSide.Sell,
            $"Beklenmeyen yon: {trade.Side}");
    }
}
