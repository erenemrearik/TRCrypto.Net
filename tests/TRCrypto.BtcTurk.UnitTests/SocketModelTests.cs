using System.Text.Json;
using TRCrypto.BtcTurk.Enums;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models.Socket;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// WebSocket mesaj modellerini dogrular.
/// </summary>
/// <remarks>
/// Fixture'lar canli bir baglantidan alinmistir. Resmi dokumantasyon mesaj kodlarini
/// listeler ama govde alanlarini vermez; bu nedenle alan adlarinin tamami gozlemle
/// belirlenmistir.
/// </remarks>
public class SocketModelTests
{
    /// <summary>Zarfin ikinci elemanini (govdeyi) verilen tipe cozer.</summary>
    private static T ParseBody<T>(string fixture)
    {
        using var document = JsonDocument.Parse(FixtureLoader.Load(fixture));
        var body = document.RootElement[1].GetRawText();
        return JsonSerializer.Deserialize<T>(body, BtcTurkJsonOptions.Default)!;
    }

    [Fact]
    public void Ticker_kisaltilmis_alan_adlari_cozulur()
    {
        var ticker = ParseBody<BtcTurkSocketTicker>("socket-ticker.json");

        Assert.Equal("BTCTRY", ticker.Symbol);
        Assert.Equal("BTC", ticker.NumeratorSymbol);
        Assert.Equal("TRY", ticker.DenominatorSymbol);
        Assert.Equal(3779950m, ticker.BestBidPrice);
        Assert.Equal(3781912m, ticker.BestAskPrice);
        Assert.Equal(3780291m, ticker.LastPrice);
        Assert.Equal(3891284m, ticker.HighPrice);
        Assert.Equal(3745640m, ticker.LowPrice);
        Assert.Equal(31.53834999m, ticker.Volume);
        Assert.Equal(-0.38m, ticker.DailyChangePercentage);
    }

    [Fact]
    public void Ticker_degerleri_metin_olarak_gelse_de_decimal_okunur()
    {
        // REST ticker ucu ayni verileri sayi olarak dondurur; socket metin kullanir.
        var ticker = ParseBody<BtcTurkSocketTicker>("socket-ticker.json");

        Assert.Equal(0.00092482m, ticker.BestBidQuantity);
    }

    [Fact]
    public void Islem_listesi_cozulur()
    {
        var update = ParseBody<BtcTurkSocketTradeUpdate>("socket-trades.json");

        Assert.Equal("BTCTRY", update.Symbol);
        Assert.Equal(2, update.Trades.Count);
        Assert.Equal("100163842129199947", update.Trades[0].Id);
        Assert.Equal(0.01273188m, update.Trades[0].Quantity);
        Assert.Equal(3765490m, update.Trades[0].Price);
    }

    [Fact]
    public void Islem_yonu_sayisal_koddan_cozulur()
    {
        // Bu esleme resmi dokumantasyonda yoktur. Canli akistaki islem kimlikleri REST
        // yanitiyla eslestirilerek belirlenmistir (0 = satis, 1 = alis).
        // Borsa bu kodlari degistirirse test kirilir ve sessiz bir yon hatasi onlenir.
        var trades = ParseBody<BtcTurkSocketTradeUpdate>("socket-trades.json").Trades;

        Assert.Equal(OrderSide.Sell, trades[0].Side);
        Assert.Equal(OrderSide.Buy, trades[1].Side);
    }

    [Fact]
    public void Islem_zaman_damgasi_metin_icinde_milisaniye_olarak_gelir()
    {
        var trade = ParseBody<BtcTurkSocketTradeUpdate>("socket-trades.json").Trades[0];

        Assert.Equal(DateTimeKind.Utc, trade.Timestamp.Kind);
        Assert.True(trade.Timestamp > new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            $"Zaman damgasi yanlis birimde cozulmus olabilir: {trade.Timestamp:O}");
    }

    [Fact]
    public void Emir_defteri_cozulur()
    {
        var book = ParseBody<BtcTurkSocketOrderBook>("socket-orderbook.json");

        Assert.Equal("BTCTRY", book.Symbol);
        Assert.Equal(2, book.Asks.Count);
        Assert.Equal(2, book.Bids.Count);
        Assert.Equal(3780992m, book.Asks[0].Price);
        Assert.Equal(0.0496469m, book.Asks[0].Quantity);
    }

    [Fact]
    public void Emir_defteri_sira_numarasi_tasinir()
    {
        // Sira numarasi olmadan fark mesajlarinda atlama tespit edilemez ve defter
        // sessizce bozulur.
        var book = ParseBody<BtcTurkSocketOrderBook>("socket-orderbook.json");

        Assert.Equal(2721198, book.Sequence);
    }

    [Fact]
    public void Emir_defterinde_en_iyi_alis_en_iyi_satistan_dusuktur()
    {
        var book = ParseBody<BtcTurkSocketOrderBook>("socket-orderbook.json");

        Assert.True(book.Bids[0].Price < book.Asks[0].Price);
    }
}
