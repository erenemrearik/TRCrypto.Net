using System.Text.Json;
using TRCrypto.BinanceTR.Objects.Internal;
using TRCrypto.BinanceTR.Objects.Models;
using TRCrypto.BinanceTR.Enums;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

/// <summary>
/// Binance TR yanitlarinin dogru ayristirildigini dogrular.
/// </summary>
/// <remarks>
/// Fixture'lar canli API'den alinmistir. Zarf yapisi BtcTurk'ten tamamen farklidir:
/// basari alani yoktur, <c>code == 0</c> ile anlasilir.
/// </remarks>
public class ResponseParsingTests
{
    private static BinanceTRResponse<T> Parse<T>(string fixture)
        => JsonSerializer.Deserialize<BinanceTRResponse<T>>(
               FixtureLoader.Load(fixture), BinanceTRJsonOptions.Default)!;

    [Fact]
    public void Basarili_zarf_code_sifir_ile_anlasilir()
    {
        // BtcTurk'te bir "success" alani vardir; burada yoktur.
        var response = Parse<BinanceTRExchangeInfo>("symbols.json");

        Assert.Equal(0, response.Code);
        Assert.True(response.Success);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public void Sifir_disi_kod_basarisizlik_demektir()
    {
        const string json = """{"code":1106,"msg":"Incorrect Page number","timestamp":1787777430518}""";

        var response = JsonSerializer.Deserialize<BinanceTRResponse<BinanceTROrderBook>>(
            json, BinanceTRJsonOptions.Default)!;

        Assert.False(response.Success);
        Assert.Equal(1106, response.Code);
        Assert.Equal("Incorrect Page number", response.Message);
    }

    [Fact]
    public void Zarf_zaman_damgasi_tasir()
    {
        // BtcTurk'te zaman damgasi zarfta yoktur; burada vardir ve sunucu saati
        // bu alandan okunur.
        var response = Parse<BinanceTRExchangeInfo>("symbols.json");

        Assert.Equal(DateTimeKind.Utc, response.Timestamp.Kind);
        Assert.True(response.Timestamp > new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Sunucu_saati_zarftan_okunur()
    {
        // Bu ucta "data" null doner; zaman yalnizca zarftadir.
        var response = Parse<object>("time.json");

        Assert.True(response.Success);
        Assert.True(response.Timestamp > new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Parite_alanlari_okunur()
    {
        var symbol = Parse<BinanceTRExchangeInfo>("symbols.json")
            .Data!.Symbols.Single(x => x.Name == "BTC_TRY");

        // Sembol alt cizgilidir ve base/quote ayri alanlarda gelir; ad ayristirilmaz.
        Assert.Equal("BTC", symbol.BaseAsset);
        Assert.Equal("TRY", symbol.QuoteAsset);
        Assert.Equal(8, symbol.BasePrecision);
        Assert.Equal(8, symbol.QuotePrecision);
    }

    [Fact]
    public void Parite_filtreleri_okunur()
    {
        var symbol = Parse<BinanceTRExchangeInfo>("symbols.json")
            .Data!.Symbols.Single(x => x.Name == "BTC_TRY");

        var priceFilter = symbol.Filters.Single(x => x.FilterType == "PRICE_FILTER");
        Assert.NotNull(priceFilter.TickSize);
        Assert.True(priceFilter.MinPrice > 0);

        var lotSize = symbol.Filters.Single(x => x.FilterType == "LOT_SIZE");
        Assert.NotNull(lotSize.StepSize);
        Assert.True(lotSize.MinQuantity > 0);
    }

    [Fact]
    public void Emir_defteri_okunur()
    {
        var book = Parse<BinanceTROrderBook>("depth.json").Data!;

        Assert.Equal(5, book.Bids.Count);
        Assert.Equal(5, book.Asks.Count);
        Assert.True(book.Bids[0].Price > 0);
        Assert.True(book.Bids[0].Quantity > 0);

        // En iyi alis, en iyi satistan dusuk olmalidir.
        Assert.True(book.Bids[0].Price < book.Asks[0].Price);
    }

    [Fact]
    public void Emir_defteri_sira_numarasi_tasir()
    {
        // Delta senkronizasyonu icin gereklidir.
        var book = Parse<BinanceTROrderBook>("depth.json").Data!;

        Assert.True(book.LastUpdateId > 0);
    }

    [Fact]
    public void Toplu_islemler_okunur()
    {
        var trades = Parse<BinanceTRAggregatedTradeList>("agg-trades.json").Data!.Trades;

        Assert.NotEmpty(trades);

        var first = trades[0];
        Assert.True(first.Id > 0);
        Assert.True(first.Price > 0);
        Assert.True(first.Quantity > 0);
        Assert.Equal(DateTimeKind.Utc, first.Timestamp.Kind);
        Assert.True(first.Timestamp > new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Islem_yonu_alici_piyasa_yapici_alanindan_turetilir()
    {
        // Binance kalibinda "m" alici tarafin piyasa yapici olup olmadigini soyler.
        // Alici piyasa yapiciysa islemi baslatan satici, yani yon satistir.
        var first = Parse<BinanceTRAggregatedTradeList>("agg-trades.json").Data!.Trades[0];

        var expected = first.BuyerIsMaker ? OrderSide.Sell : OrderSide.Buy;
        Assert.Equal(expected, first.Side);
    }
}
