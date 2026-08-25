using System.Text.Json;
using TRCrypto.BtcTurk.Enums;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Emir yanitlarinin dogru ayristirildigini dogrular.
/// </summary>
/// <remarks>
/// Fixture'lar resmi dokumantasyondaki ornekleri birebir yansitir. Alan adlari ve enum
/// bicimleri uclar arasinda farklilik gosterir; bu farklar bilincli olarak korunmustur.
/// </remarks>
public class OrderTests
{
    private static T Parse<T>(string fixture)
        => JsonSerializer.Deserialize<BtcTurkResponse<T>>(
               FixtureLoader.Load(fixture), BtcTurkJsonOptions.Default)!.Data!;

    [Fact]
    public void Acik_emirler_alis_ve_satis_olarak_ayrilir()
    {
        var orders = Parse<BtcTurkOpenOrders>("open-orders.json");

        Assert.Single(orders.Asks);
        Assert.Single(orders.Bids);
        Assert.Equal(OrderSide.Sell, orders.Asks[0].Side);
        Assert.Equal(OrderSide.Buy, orders.Bids[0].Side);
    }

    [Fact]
    public void Emir_alanlari_okunur()
    {
        var order = Parse<BtcTurkOpenOrders>("open-orders.json").Bids[0];

        Assert.Equal(9932534, order.Id);
        Assert.Equal(20000.00m, order.Price);
        Assert.Equal(0.001m, order.Quantity);
        Assert.Equal("BTCTRY", order.PairSymbol);
        Assert.Equal(OrderMethod.Limit, order.Method);
        Assert.Equal(OrderStatus.Untouched, order.Status);
        Assert.Equal("test", order.ClientOrderId);
        Assert.Equal(DateTimeKind.Utc, order.CreateTime.Kind);
    }

    [Fact]
    public void Kucuk_harfli_alan_adlari_da_okunur()
    {
        // Bu uc "pairsymbol" (kucuk harf) dondurur; tekil emir ucu "pairSymbol" dondurur.
        var order = Parse<IReadOnlyList<BtcTurkOrder>>("all-orders.json")[0];

        Assert.Equal("BTCTRY", order.PairSymbol);
        Assert.Equal("BTC_TRY", order.PairSymbolNormalized);
    }

    [Fact]
    public void Alt_cizgisiz_emir_yontemi_de_eslesir()
    {
        // exchangeinfo ucu "STOP_MARKET" dondururken emir uclari "stopmarket" donduruyor.
        // Tek bicimi eslestirmek, stop emirlerinin taninmamasina yol acardi.
        var order = Parse<BtcTurkOrder>("single-order.json");

        Assert.Equal(OrderMethod.StopMarket, order.Method);
        Assert.True(Enum.IsDefined(order.Method), $"Emir yontemi taninmadi: {order.Method}");
    }

    [Fact]
    public void Tekil_emirde_stop_fiyati_ve_kalan_miktar_okunur()
    {
        var order = Parse<BtcTurkOrder>("single-order.json");

        Assert.Equal(20000m, order.StopPrice);
        Assert.Equal(0.1234567800000000m, order.RemainingQuantity);
        Assert.Equal(OrderSide.Sell, order.Side);
    }

    [Fact]
    public void Emir_olusturma_yaniti_okunur()
    {
        // Bu uc farkli alan adlari kullanir: "datetime" ve "newOrderClientId".
        var placed = Parse<BtcTurkOrderPlacement>("submit-order.json");

        Assert.Equal(9932534, placed.Id);
        Assert.Equal(20000.00m, placed.Price);
        Assert.Equal(0.001m, placed.Quantity);
        Assert.Equal("BTCTRY", placed.PairSymbol);
        Assert.Equal(OrderSide.Buy, placed.Side);
        Assert.Equal("test", placed.ClientOrderId);
        Assert.Equal(DateTimeKind.Utc, placed.CreateTime.Kind);
    }

    [Fact]
    public void Iptal_yanitindaki_bos_durum_kodu_ayristirmayi_dusurmez()
    {
        // Iptal ucu "code" alanini bos metin olarak donduruyor.
        var response = JsonSerializer.Deserialize<BtcTurkResponse<object>>(
            FixtureLoader.Load("cancel-order.json"), BtcTurkJsonOptions.Default)!;

        Assert.True(response.Success);
    }
}
