using System.Text.Json;
using TRCrypto.BinanceTR.Enums;
using TRCrypto.BinanceTR.Objects.Internal;
using TRCrypto.BinanceTR.Objects.Models;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

/// <summary>
/// Hesap ve emir yanitlarinin cozumlenmesini dogrular.
/// </summary>
/// <remarks>
/// Fixture'lar resmi dokumantasyondaki semalardan uretilmistir; canli hesapla dogrulama
/// API anahtari geldiginde yapilacaktir. Buradaki en kritik nokta sayisal enum'lardir:
/// borsa durum ve tur bilgisini metin yerine sayi olarak tasir ve yanlis eslesme
/// hata vermeden sessizce yanlis emir durumu uretir.
/// </remarks>
public class PrivateModelTests
{
    private static T ParseData<T>(string fixture)
    {
        var envelope = JsonSerializer.Deserialize<BinanceTRResponse<T>>(
            FixtureLoader.Load(fixture), BinanceTRJsonOptions.Default)!;

        Assert.True(envelope.Success, envelope.Message);
        return envelope.Data!;
    }

    [Fact]
    public void Hesap_bilgisi_cozulur()
    {
        var account = ParseData<BinanceTRAccount>("account.json");

        Assert.Equal(0.001m, account.MakerCommission);
        Assert.Equal(0.0015m, account.FiatTakerCommission);
        Assert.Equal(2, account.Assets.Count);
    }

    [Fact]
    public void Hesap_izinleri_sayisal_bayraklardan_okunur()
    {
        var account = ParseData<BinanceTRAccount>("account.json");

        // Borsa bu bayraklari 0 ya da 1 olarak dondurur, true/false olarak degil.
        Assert.True(account.CanTrade);
        Assert.False(account.CanWithdraw);
        Assert.True(account.CanDeposit);
    }

    [Fact]
    public void Bakiye_toplami_iki_bilesenden_hesaplanir()
    {
        var account = ParseData<BinanceTRAccount>("account.json");
        var lira = account.Assets.Single(x => x.Asset == "TRY");

        // Borsa toplami ayri bir alanda vermez; yalnizca kullanilabilir ve bloke tutari.
        Assert.Equal(1250.75m, lira.Available);
        Assert.Equal(300.00m, lira.Locked);
        Assert.Equal(1550.75m, lira.Total);
    }

    [Fact]
    public void Emir_listesi_sarmalayici_alandan_cozulur()
    {
        var orders = ParseData<BinanceTROrderList>("orders.json");

        Assert.Equal(2, orders.Orders.Count);
        Assert.Equal("BTC_TRY", orders.Orders[0].Symbol);
        Assert.Equal(1234567, orders.Orders[0].OrderId);
    }

    [Fact]
    public void Emir_yonu_ve_turu_sayisal_degerlerden_cozulur()
    {
        var orders = ParseData<BinanceTROrderList>("orders.json");

        Assert.Equal(OrderSide.Buy, orders.Orders[0].Side);
        Assert.Equal(OrderType.Limit, orders.Orders[0].Type);
        Assert.Equal(OrderSide.Sell, orders.Orders[1].Side);
        Assert.Equal(OrderType.Market, orders.Orders[1].Type);
    }

    [Fact]
    public void Emir_durumu_sayisal_degerden_cozulur()
    {
        var orders = ParseData<BinanceTROrderList>("orders.json");

        Assert.Equal(OrderStatus.PartiallyFilled, orders.Orders[0].Status);
        Assert.Equal(OrderStatus.Filled, orders.Orders[1].Status);
    }

    [Fact]
    public void Emir_miktarlari_ve_gerceklesme_okunur()
    {
        var order = ParseData<BinanceTROrderList>("orders.json").Orders[0];

        Assert.Equal(3500000m, order.Price);
        Assert.Equal(0.005m, order.Quantity);
        Assert.Equal(0.002m, order.QuantityFilled);
        Assert.Equal(3499000m, order.AverageFillPrice);
        Assert.Equal(6998m, order.QuoteQuantityFilled);
        Assert.Equal(TimeInForce.GoodTillCanceled, order.TimeInForce);
        Assert.True(order.IsWorking);
    }

    [Fact]
    public void Emir_ayrintisi_cozulur()
    {
        var order = ParseData<BinanceTROrder>("order-detail.json");

        Assert.Equal(1234567, order.OrderId);
        Assert.Equal(-1, order.OrderListId);
        Assert.Equal("abc123", order.ClientOrderId);
        Assert.Equal(OrderStatus.Canceled, order.Status);
    }

    [Fact]
    public void Emir_olusturma_yaniti_yalnizca_kimlik_tasir()
    {
        var placed = ParseData<BinanceTRPlacedOrder>("place-order.json");

        // Yanit emrin tam halini dondurmez; durum icin emir ayrica sorgulanmalidir.
        Assert.Equal(9876543, placed.OrderId);
        Assert.NotEqual(default, placed.CreateTime);
    }

    [Fact]
    public void Emir_olusturma_yanitinda_mesaj_alani_farkli_adlanir()
    {
        // Bu uc hata metnini "message" alaninda, digerleri "msg" alaninda dondurur.
        // Tek bir ad beklemek, hatanin bazi uclarda bos gorunmesine yol acar.
        var envelope = JsonSerializer.Deserialize<BinanceTRResponse<BinanceTRPlacedOrder>>(
            """{"code":-1013,"message":"Filter failure: MIN_NOTIONAL","data":null,"timestamp":1}""",
            BinanceTRJsonOptions.Default)!;

        Assert.False(envelope.Success);
        Assert.Equal("Filter failure: MIN_NOTIONAL", ((IBinanceTRResponse)envelope).Message);
    }

    [Fact]
    public void Kullanici_islemleri_cozulur()
    {
        var trades = ParseData<BinanceTRUserTradeList>("user-trades.json");
        var trade = Assert.Single(trades.Trades);

        Assert.Equal(3311, trade.TradeId);
        Assert.Equal(1234567, trade.OrderId);
        Assert.Equal(3499000m, trade.Price);
        Assert.Equal(0.002m, trade.Quantity);
        Assert.Equal(6.998m, trade.Commission);
        Assert.Equal("TRY", trade.CommissionAsset);
    }

    [Fact]
    public void Islemin_yonu_alici_bayragindan_okunur()
    {
        var trade = ParseData<BinanceTRUserTradeList>("user-trades.json").Trades[0];

        // Islem kaydinda ayri bir yon alani yoktur; yon yalnizca bu bayraktan cikarilir.
        Assert.True(trade.IsBuyer);
        Assert.False(trade.IsMaker);
    }
}
