using TRCrypto.BtcTurk.Clients;
using TRCrypto.BtcTurk.Enums;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Emir olusturmada girdi dogrulamasini sinar.
/// </summary>
/// <remarks>
/// Emirler gercek para hareketi yaratir. Eksik ya da hatali bir parametrenin borsaya
/// ulasmasi, beklenmedik bir emre donusebilir; bu nedenle dogrulama aga cikilmadan
/// yapilir. Bu testler ag erisimi gerektirmez.
/// </remarks>
public class OrderValidationTests
{
    private static BtcTurkRestClient CreateClient()
        => new(options =>
        {
            options.RateLimiterEnabled = false;
            options.ApiCredentials = new BtcTurkCredentials(
                "FAKE-public-key", "RkFLRS1zZWNyZXQtZm9yLXVuaXQtdGVzdHMtb25seQ==");
        });

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Bos_sembolle_emir_verilemez(string symbol)
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentException>(() => client.SpotApi.Trading.PlaceOrderAsync(
            symbol, OrderSide.Buy, OrderMethod.Limit, quantity: 1m, price: 100m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Pozitif_olmayan_miktarla_emir_verilemez(decimal quantity)
    {
        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.Trading.PlaceOrderAsync(
                "BTCTRY", OrderSide.Buy, OrderMethod.Limit, quantity, price: 100m));

        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public async Task Negatif_fiyatla_emir_verilemez()
    {
        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.Trading.PlaceOrderAsync(
                "BTCTRY", OrderSide.Buy, OrderMethod.Limit, quantity: 1m, price: -100m));

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public async Task Limit_emri_fiyat_ister()
    {
        var client = CreateClient();

        // Fiyatsiz bir limit emri gonderilirse borsa bunu nasil yorumlayacagi belirsizdir;
        // istek hic olusturulmamalidir.
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.Trading.PlaceOrderAsync(
                "BTCTRY", OrderSide.Buy, OrderMethod.Limit, quantity: 1m));

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public async Task Stop_market_emri_stop_fiyati_ister()
    {
        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.Trading.PlaceOrderAsync(
                "BTCTRY", OrderSide.Sell, OrderMethod.StopMarket, quantity: 1m));

        Assert.Equal("stopPrice", exception.ParamName);
    }

    [Fact]
    public async Task Stop_limit_emri_hem_fiyat_hem_stop_fiyati_ister()
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.Trading.PlaceOrderAsync(
                "BTCTRY", OrderSide.Buy, OrderMethod.StopLimit, quantity: 1m, stopPrice: 100m));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.Trading.PlaceOrderAsync(
                "BTCTRY", OrderSide.Buy, OrderMethod.StopLimit, quantity: 1m, price: 100m));
    }

    [Fact]
    public async Task Emir_gecmisi_kayit_sayisi_siniri_asilamaz()
    {
        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => client.SpotApi.Trading.GetOrdersAsync(limit: 1001));

        Assert.Equal("limit", exception.ParamName);
    }
}
