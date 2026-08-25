using System.Text.Json;
using TRCrypto.BtcTurk.Converters;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// BtcTurk ondalik degerleri uclar arasinda farkli ayiricilarla dondurur.
/// </summary>
/// <remarks>
/// <para>
/// Piyasa verisi ve emir uclari nokta kullanir (<c>"0.00269390"</c>), ancak bakiye ucu
/// virgul kullanir (<c>"27223,7283250757643288"</c>).
/// </para>
/// <para>
/// Virgulu yok sayip <c>InvariantCulture</c> ile ayristirmak, virgulu binlik ayirici
/// sayarak bakiyeyi kat kat buyuk gosterirdi. Bu bir bakiye goruntuleme kutuphanesinde
/// kabul edilemez bir hatadir; bu nedenle her iki ayirici da desteklenir.
/// </para>
/// <para>
/// Belirsizlik yoktur: BtcTurk binlik ayirici kullanmaz (buyuk sayilar <c>"3708000"</c>
/// bicimindedir), dolayisiyla bir virgul her zaman ondalik ayiricidir.
/// </para>
/// </remarks>
public class DecimalParsingTests
{
    private sealed record Wrapper
    {
        [System.Text.Json.Serialization.JsonPropertyName("value")]
        [System.Text.Json.Serialization.JsonConverter(typeof(BtcTurkDecimalConverter))]
        public decimal Value { get; init; }
    }

    private static decimal Parse(string rawJsonValue)
        => JsonSerializer.Deserialize<Wrapper>($"{{\"value\":{rawJsonValue}}}")!.Value;

    [Fact]
    public void Nokta_ayiricili_metin_okunur()
    {
        Assert.Equal(0.00269390m, Parse("\"0.00269390\""));
    }

    [Fact]
    public void Virgul_ayiricili_metin_ondalik_olarak_okunur()
    {
        // Bakiye ucunun bicimi. Virgul binlik ayirici sayilsaydi sonuc
        // 272237283250757643288 olurdu.
        Assert.Equal(27223.7283250757643288m, Parse("\"27223,7283250757643288\""));
    }

    [Fact]
    public void Ayiricisiz_buyuk_sayi_dogru_okunur()
    {
        Assert.Equal(3708000m, Parse("\"3708000\""));
    }

    [Fact]
    public void Sayi_olarak_gelen_deger_okunur()
    {
        Assert.Equal(3713440m, Parse("3713440"));
    }

    [Fact]
    public void Negatif_deger_okunur()
    {
        Assert.Equal(-12.5m, Parse("\"-12,5\""));
    }

    [Fact]
    public void Bos_metin_sifir_olarak_okunur()
    {
        Assert.Equal(0m, Parse("\"\""));
    }

    [Fact]
    public void Hassasiyet_korunur()
    {
        // 18 ondalik basamak; double uzerinden gecilseydi kayip olurdu.
        Assert.Equal(0.123456789012345678m, Parse("\"0,123456789012345678\""));
    }
}
