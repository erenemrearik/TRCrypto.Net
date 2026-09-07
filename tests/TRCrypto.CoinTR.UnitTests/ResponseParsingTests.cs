using System.Text.Json;
using TRCrypto.CoinTR.Enums;
using TRCrypto.CoinTR.Objects.Internal;
using TRCrypto.CoinTR.Objects.Models;
using Xunit;

namespace TRCrypto.CoinTR.UnitTests;

/// <summary>
/// Canli yanitlardan alinan govdelerin dogru cozumlendigini dogrular.
/// </summary>
/// <remarks>
/// Fixture'lar 7 Eylul 2026'da <c>api.cointr.pro</c> uzerinden alinan gercek yanitlardir.
/// Bu borsada butun sayisal degerler metin olarak gelir; sayi bekleyen bir alan sessizce
/// sifir kalir.
/// </remarks>
public class ResponseParsingTests
{
    private static T Parse<T>(string fixture)
        => JsonSerializer.Deserialize<CoinTRResponse<T>>(
            FixtureLoader.Load(fixture), CoinTRJsonOptions.Default)!.Data!;

    private static CoinTRResponse<object> Envelope(string fixture)
        => JsonSerializer.Deserialize<CoinTRResponse<object>>(
            FixtureLoader.Load(fixture), CoinTRJsonOptions.Default)!;

    [Fact]
    public void Basari_kodu_metin_olarak_okunur()
    {
        // Kod "00000" bir metindir. int olarak okunmaya calisilirsa ayristirma hatasi
        // verir; sifir olarak okunursa her yanit basarili sayilir.
        var envelope = Envelope("time.json");

        Assert.Equal(CoinTRResponse<object>.SuccessCode, envelope.Code);
        Assert.True(envelope.Success);
    }

    [Fact]
    public void Hata_yaniti_basarisiz_sayilir()
    {
        // HTTP 200 icinde hata donebilir; zarf koduna bakilmazsa hata sessizce yutulur.
        var envelope = Envelope("error.json");

        Assert.False(envelope.Success);
        Assert.Equal("40034", envelope.Code);
        Assert.Equal("Parameter symbol does not exist", envelope.Message);
    }

    [Fact]
    public void Sunucu_saati_cozumlenir()
    {
        var data = Parse<CoinTRServerTime>("time.json");

        Assert.Equal(
            DateTimeOffset.FromUnixTimeMilliseconds(1788787397722).UtcDateTime,
            data.ServerTime);
    }

    [Fact]
    public void Pariteler_cozumlenir()
    {
        var symbols = Parse<CoinTRSymbol[]>("symbols.json");

        Assert.Equal(2, symbols.Length);

        var btc = symbols[0];
        Assert.Equal("BTCTRY", btc.Name);

        // Base ve quote ayri alanlardadir; sembol adi ayristirilmaz.
        Assert.Equal("BTC", btc.BaseAsset);
        Assert.Equal("TRY", btc.QuoteAsset);

        Assert.Equal(2, btc.PriceDecimals);
        Assert.Equal(5, btc.QuantityDecimals);
        Assert.Equal(0.001m, btc.MakerFeeRate);
        Assert.Equal(0.001m, btc.TakerFeeRate);
        Assert.True(btc.IsTrading);

        Assert.False(symbols[1].IsTrading);
    }

    [Fact]
    public void Ticker_cozumlenir()
    {
        var ticker = Parse<CoinTRTicker[]>("tickers.json")[0];

        Assert.Equal("BTCTRY", ticker.Symbol);
        Assert.Equal(3846834m, ticker.LastPrice);
        Assert.Equal(3901118m, ticker.HighPrice);
        Assert.Equal(3813063m, ticker.LowPrice);
        Assert.Equal(3846177m, ticker.BestBidPrice);
        Assert.Equal(3847875m, ticker.BestAskPrice);

        // Ondalik basamak kaybi olmamalidir.
        Assert.Equal(13.86216m, ticker.Volume);
        Assert.Equal(53409991.10738m, ticker.QuoteVolume);
    }

    [Fact]
    public void Degisim_orani_yuzdeye_cevrilir()
    {
        // Borsa kesir gonderir. Dogrudan aktarmak degeri yuz kat kucuk gosterir; bu
        // sessiz bir hatadir, yanlis deger de gecerli bir sayidir.
        var ticker = Parse<CoinTRTicker[]>("tickers.json")[0];

        Assert.Equal(-0.00205m, ticker.ChangeRatio);
        Assert.Equal(-0.205m, ticker.ChangePercentage);
    }

    [Fact]
    public void Emir_defteri_kademeleri_cozumlenir()
    {
        var book = Parse<CoinTROrderBook>("orderbook.json");

        // Kademeler alan adi tasimaz; sira fiyat, miktar seklindedir.
        Assert.Equal(3847875m, book.Asks[0].Price);
        Assert.Equal(0.06114m, book.Asks[0].Quantity);
        Assert.Equal(3846177m, book.Bids[0].Price);
        Assert.Equal(0.04892m, book.Bids[0].Quantity);

        // Satislar artan, alislar azalan siradadir.
        Assert.True(book.Asks[1].Price > book.Asks[0].Price);
        Assert.True(book.Bids[1].Price < book.Bids[0].Price);
    }

    [Fact]
    public void Islem_yonu_metin_olarak_okunur()
    {
        // Yon burada metindir; Binance TR ayni bilgiyi sayi olarak tasir.
        var trades = Parse<CoinTRTrade[]>("fills.json");

        Assert.Equal(OrderSide.Sell, trades[0].Side);
        Assert.Equal(OrderSide.Buy, trades[1].Side);
        Assert.Equal("1480847216137556071", trades[0].TradeId);
        Assert.Equal(3847741m, trades[0].Price);
        Assert.Equal(0.00009m, trades[0].Quantity);
    }

    [Fact]
    public void Mum_alanlari_dogru_sirada_eslesir()
    {
        // Mum isimsiz bir dizidir; sira anlamin tek tasiyicisidir. Yanlis sira hata
        // vermez, yalnizca yanlis mum uretir.
        var kline = Parse<CoinTRKline[]>("candles.json")[0];

        Assert.Equal(
            DateTimeOffset.FromUnixTimeMilliseconds(1788787320000).UtcDateTime,
            kline.OpenTime);
        Assert.Equal(3846995m, kline.OpenPrice);
        Assert.Equal(3847996m, kline.HighPrice);
        Assert.Equal(3846195m, kline.LowPrice);
        Assert.Equal(3847544m, kline.ClosePrice);
        Assert.Equal(0.00771m, kline.Volume);
        Assert.Equal(613.668103686567m, kline.QuoteVolume);

        // En yuksek en dusukten buyuk olmalidir; sira karisirsa bu bozulur.
        Assert.True(kline.HighPrice >= kline.LowPrice);
    }
}
