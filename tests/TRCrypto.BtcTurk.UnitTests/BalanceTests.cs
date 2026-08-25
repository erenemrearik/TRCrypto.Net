using System.Text.Json;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Bakiye yanitinin dogru ayristirildigini dogrular.
/// </summary>
/// <remarks>
/// Fixture, resmi dokumantasyondaki ornegi birebir yansitir; bu ucun ondalik degerleri
/// virgulle donmesi bilincli olarak korunmustur.
/// </remarks>
public class BalanceTests
{
    private static IReadOnlyList<BtcTurkBalance> Parse()
        => JsonSerializer.Deserialize<BtcTurkResponse<IReadOnlyList<BtcTurkBalance>>>(
               FixtureLoader.Load("balances.json"), BtcTurkJsonOptions.Default)!.Data!;

    [Fact]
    public void Virgullu_bakiye_dogru_buyuklukte_okunur()
    {
        var tryBalance = Parse().Single(x => x.Asset == "TRY");

        // Virgul binlik ayirici sayilsaydi sonuc 272237283250757643288 olurdu.
        Assert.Equal(27223.7283250757643288m, tryBalance.Total);
        Assert.True(tryBalance.Total < 1_000_000m, $"Bakiye buyuklugu hatali: {tryBalance.Total}");
    }

    [Fact]
    public void Serbest_ve_kilitli_bakiye_toplami_tutar()
    {
        var tryBalance = Parse().Single(x => x.Asset == "TRY");

        Assert.Equal(22349.3654565035348765m, tryBalance.Available);
        Assert.Equal(4874.3628685722294523m, tryBalance.Locked);
        Assert.Equal(tryBalance.Total, tryBalance.Available + tryBalance.Locked);
    }

    [Fact]
    public void Sifir_bakiye_okunur()
    {
        var btcBalance = Parse().Single(x => x.Asset == "BTC");

        Assert.Equal(0m, btcBalance.Locked);
        Assert.Equal(0.00312450m, btcBalance.Available);
    }

    [Fact]
    public void Varlik_bilgileri_okunur()
    {
        var tryBalance = Parse().Single(x => x.Asset == "TRY");

        Assert.Equal("Türk Lirası", tryBalance.AssetName);
        Assert.Equal(2, tryBalance.Precision);
        Assert.Equal(DateTimeKind.Utc, tryBalance.Timestamp.Kind);
    }
}
