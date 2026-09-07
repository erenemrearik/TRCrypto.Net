using System.Text.Json;
using TRCrypto.BinanceTR.Enums;
using TRCrypto.BinanceTR.Objects.Internal;
using TRCrypto.BinanceTR.Objects.Models;
using TRCrypto.BinanceTR.Objects.Models.Socket;
using TRCrypto.BinanceTR.Objects.Sockets;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

/// <summary>
/// Kullanici akisinin ve onu besleyen uclarin cozumlenmesini dogrular.
/// </summary>
/// <remarks>
/// Fixture'lar resmi dokumantasyondaki semalardan uretilmistir. Kullanici akisi canli bir
/// hesapla henuz denenmedi; token almak imzali bir istek gerektiriyor ve imza su an
/// borsada kabul edilmiyor (D-45, D-46).
/// </remarks>
public class PrivateStreamTests
{
    private static T ParseData<T>(string fixture)
    {
        var envelope = JsonSerializer.Deserialize<BinanceTRResponse<T>>(
            FixtureLoader.Load(fixture), BinanceTRJsonOptions.Default)!;

        Assert.True(envelope.Success, envelope.Message);
        return envelope.Data!;
    }

    private static T ParseStream<T>(string fixture)
        => JsonSerializer.Deserialize<T>(FixtureLoader.Load(fixture), BinanceTRJsonOptions.Default)!;

    [Fact]
    public void Toplu_iptal_basarili_ve_basarisiz_kimlikleri_ayirir()
    {
        var result = ParseData<BinanceTRBatchCancelResult>("batch-cancel.json");

        // Toplu iptalde bazi emirler iptal edilirken bazilari edilemez. Yaniti tek bir
        // basari degeri gibi okumak, iptal edilmemis emirlerin gozden kacmasina yol acar.
        Assert.Equal([1L, 2L, 3L], result.SucceededOrderIds);
        var failed = Assert.Single(result.FailedOrders);
        Assert.Equal(4, failed.OrderId);
        Assert.Equal(500, failed.Code);
        Assert.Equal("The order has been cancelled or does not exist", failed.Message);
    }

    [Fact]
    public void Tek_varlik_bakiyesi_cozulur()
    {
        var asset = ParseData<BinanceTRAccountAsset>("asset.json");

        Assert.Equal("ADA", asset.Asset);
        Assert.Equal(272.555m, asset.Available);
        Assert.Equal(3m, asset.Locked);
        Assert.Equal(275.555m, asset.Total);
    }

    [Fact]
    public void Dinleme_tokeni_ve_gecerlilik_suresi_cozulur()
    {
        var token = ParseData<BinanceTRListenToken>("listen-token.json");

        Assert.Equal("ornek-dinleme-tokeni-gercek-degil", token.Token);
        Assert.NotEqual(default, token.ExpirationTime);
    }

    [Fact]
    public void Dinleme_tokeni_kendiliginden_yenilenmez()
    {
        var token = ParseData<BinanceTRListenToken>("listen-token.json");

        // Borsa tokeni yenilemiyor; suresi dolmadan yenisi alinip yeniden abone olunmali.
        // Bu yuzden bitis ani modelde tasinir, yoksa cagiran taraf ne zaman yenileyecegini
        // bilemez ve akis sessizce durur.
        Assert.True(token.ExpirationTime > new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Hesap_guncellemesi_cozulur()
    {
        var update = ParseStream<BinanceTRStreamAccountUpdate>("stream-account.json");

        Assert.Equal("outboundAccountPosition", update.Event);
        Assert.Equal(2, update.Balances.Count);

        var lira = update.Balances.Single(x => x.Asset == "TRY");
        Assert.Equal(1250.75m, lira.Available);
        Assert.Equal(300m, lira.Locked);
    }

    [Fact]
    public void Emir_guncellemesi_cozulur()
    {
        var update = ParseStream<BinanceTRStreamOrderUpdate>("stream-order.json");

        Assert.Equal("executionReport", update.Event);
        Assert.Equal("BTC_TRY", update.Symbol);
        Assert.Equal("abc123", update.ClientOrderId);
        Assert.Equal(1234567, update.OrderId);
        Assert.Equal(3500000m, update.Price);
        Assert.Equal(0.005m, update.Quantity);
    }

    [Fact]
    public void Emir_guncellemesinde_yon_ve_tur_metin_olarak_gelir()
    {
        var update = ParseStream<BinanceTRStreamOrderUpdate>("stream-order.json");

        // REST tarafi ayni bilgileri SAYI olarak tasiyor (D-42). Akis metin kullaniyor,
        // yani iki tarafta ayni enum farkli bicimde cozumlenmek zorunda.
        Assert.Equal(OrderSide.Buy, update.Side);
        Assert.Equal(OrderType.Limit, update.Type);
        Assert.Equal(OrderStatus.PartiallyFilled, update.Status);
    }

    [Fact]
    public void Emir_guncellemesinde_gerceklesen_kisim_okunur()
    {
        var update = ParseStream<BinanceTRStreamOrderUpdate>("stream-order.json");

        Assert.Equal(0.002m, update.LastQuantityFilled);
        Assert.Equal(0.002m, update.QuantityFilled);
        Assert.Equal(3499000m, update.LastPriceFilled);
        Assert.Equal(6.998m, update.Fee);
        Assert.Equal("TRY", update.FeeAsset);
        Assert.False(update.IsMaker);
    }

    [Fact]
    public void Abonelik_istegi_dokumante_edilen_bicimde_kurulur()
    {
        var request = new BinanceTRListenTokenRequest("ornek-token");

        // Bu akis, piyasa akisindan farkli bir sunucuda ve farkli bir protokolde calisir:
        // adres yoluna akis adi yazmak yerine yontem adi tasiyan bir mesaj gonderilir.
        Assert.Equal("userDataStream.subscribe.listenToken", request.Method);
        Assert.Equal("ornek-token", request.Params.ListenToken);
        Assert.False(string.IsNullOrWhiteSpace(request.Id));
    }
}
