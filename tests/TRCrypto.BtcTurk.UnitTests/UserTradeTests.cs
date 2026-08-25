using System.Text.Json;
using TRCrypto.BtcTurk.Enums;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Kullanici islem gecmisi yanitinin dogru ayristirildigini dogrular.
/// </summary>
/// <remarks>
/// Fixture resmi dokumantasyondaki ornegi birebir yansitir; tutarlardaki negatif
/// isaretler bilincli olarak korunmustur.
/// </remarks>
public class UserTradeTests
{
    private static IReadOnlyList<BtcTurkUserTrade> Parse()
        => JsonSerializer.Deserialize<BtcTurkResponse<IReadOnlyList<BtcTurkUserTrade>>>(
               FixtureLoader.Load("user-trades.json"), BtcTurkJsonOptions.Default)!.Data!;

    [Fact]
    public void Islem_alanlari_okunur()
    {
        var trade = Parse()[0];

        Assert.Equal(1181163798924649598, trade.Id);
        Assert.Equal(10938696222, trade.OrderId);
        Assert.Equal("ETHW", trade.NumeratorSymbol);
        Assert.Equal("TRY", trade.DenominatorSymbol);
        Assert.Equal(122.00m, trade.Price);
        Assert.Equal(OrderSide.Sell, trade.Side);
        Assert.Equal(DateTimeKind.Utc, trade.Timestamp.Kind);
    }

    [Fact]
    public void Satis_tutarlari_negatif_isareti_korur()
    {
        var trade = Parse()[0];

        // Isaret varligin hesaptan ciktigini belirtir. Mutlak degere cevirmek,
        // toplam hacim ya da komisyon hesabini sessizce yanlis yapar.
        Assert.True(trade.Quantity < 0, $"Satis miktari negatif olmaliydi: {trade.Quantity}");
        Assert.True(trade.Fee < 0, $"Komisyon negatif olmaliydi: {trade.Fee}");
        Assert.True(trade.Tax < 0, $"Vergi negatif olmaliydi: {trade.Tax}");
    }

    [Fact]
    public void Hassas_miktar_ayrica_tasinir()
    {
        var trade = Parse()[0];

        // "amount" yuvarlanmis metin, "preciseAmount" tam sayisal degerdir.
        Assert.Equal(-0.3384m, trade.Quantity);
        Assert.Equal(-0.33840811m, trade.PreciseQuantity);
    }

    [Fact]
    public void Turkiyeye_ozgu_vergi_alani_korunur()
    {
        var trade = Parse()[0];

        // Bu alanin diger borsalarda karsiligi yoktur ve shared yuzeyde temsil edilemez;
        // native modelde kaybolmamasi gerekir.
        Assert.Equal(-0.01133607m, trade.Tax);
    }

    [Fact]
    public void Bos_istemci_kimligi_null_kalir()
    {
        var trade = Parse()[0];

        Assert.Null(trade.ClientOrderId);
    }
}
