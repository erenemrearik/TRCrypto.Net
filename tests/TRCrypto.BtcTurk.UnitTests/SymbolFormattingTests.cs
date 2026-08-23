using CryptoExchange.Net.SharedApis;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>Sembol bicimlendirme kurallari (vendor freeze: istek formati "BTCTRY").</summary>
public class SymbolFormattingTests
{
    [Theory]
    [InlineData("BTC", "TRY", "BTCTRY")]
    [InlineData("btc", "try", "BTCTRY")]
    [InlineData("ETH", "USDT", "ETHUSDT")]
    public void Spot_sembolu_birlestirilir_ve_buyuk_harfe_cevrilir(string baseAsset, string quoteAsset, string expected)
    {
        var result = BtcTurkExchange.FormatSymbol(baseAsset, quoteAsset, TradingMode.Spot);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TL_takma_adi_TRY_olarak_cozulur()
    {
        // Bazi Turkiye kaynaklari TRY yerine TL kullanir; merkezi alias ile cozulur,
        // dagitik string replace ile DEGIL (spesifikasyon Bolum 7.2, RISK-05).
        var result = BtcTurkExchange.FormatSymbol("BTC", "TL", TradingMode.Spot);

        Assert.Equal("BTCTRY", result);
    }
}
