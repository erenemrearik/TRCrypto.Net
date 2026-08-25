using System.Text.Json;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Kline yanitinin dogru ayristirildigini dogrular.
/// </summary>
/// <remarks>
/// Bu uc diger uclardan ayrilir: farkli host kullanir, standart zarfi tasimaz ve
/// verileri paralel diziler halinde dondurur. Fixture canli API'den alinmistir.
/// </remarks>
public class KlineTests
{
    private static BtcTurkKlineResponse Parse(string fixture)
        => JsonSerializer.Deserialize<BtcTurkKlineResponse>(
               FixtureLoader.Load(fixture), BtcTurkJsonOptions.Default)!;

    [Fact]
    public void Paralel_diziler_mum_nesnelerine_donusturulur()
    {
        var klines = Parse("klines.json").ToKlines();

        Assert.Equal(3, klines.Count);

        var first = klines[0];
        Assert.Equal(3802189m, first.OpenPrice);
        Assert.Equal(3809691m, first.HighPrice);
        Assert.Equal(3787859m, first.LowPrice);
        Assert.Equal(3796125m, first.ClosePrice);
        Assert.Equal(0.54581564m, first.Volume);
    }

    [Fact]
    public void Zaman_damgasi_saniye_olarak_cozulur()
    {
        var first = Parse("klines.json").ToKlines()[0];

        // Diger uclar milisaniye kullanir; burada saniye varsayilmazsa tarih 1970'e yakin cikar.
        Assert.Equal(DateTimeKind.Utc, first.OpenTime.Kind);
        Assert.True(first.OpenTime > new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            $"Zaman damgasi milisaniye olarak yorumlanmis olabilir: {first.OpenTime:O}");
    }

    [Fact]
    public void Mumlar_zaman_sirasini_korur()
    {
        var klines = Parse("klines.json").ToKlines();

        Assert.True(klines[0].OpenTime < klines[1].OpenTime);
    }

    [Fact]
    public void Durum_alani_okunur()
    {
        // "s" alani resmi dokumantasyonda gecmez ancak canli yanitta bulunur.
        Assert.Equal("ok", Parse("klines.json").Status);
    }

    [Fact]
    public void Bos_yanit_bos_liste_uretir()
    {
        const string json = """{"s":"ok","t":[],"h":[],"o":[],"l":[],"c":[],"v":[]}""";

        var klines = JsonSerializer.Deserialize<BtcTurkKlineResponse>(json, BtcTurkJsonOptions.Default)!
            .ToKlines();

        Assert.Empty(klines);
    }

    [Fact]
    public void Uyumsuz_dizi_uzunluklari_sessizce_yutulmaz()
    {
        // Diziler ayni uzunlukta olmazsa hangi degerin hangi muma ait oldugu belirsizdir.
        // Kisa dizinin sonunu doldurmak yerine hata verilir.
        const string json = """{"s":"ok","t":[1,2],"h":[1],"o":[1,2],"l":[1,2],"c":[1,2],"v":[1,2]}""";

        var response = JsonSerializer.Deserialize<BtcTurkKlineResponse>(json, BtcTurkJsonOptions.Default)!;

        Assert.Throws<InvalidOperationException>(() => response.ToKlines());
    }
}
