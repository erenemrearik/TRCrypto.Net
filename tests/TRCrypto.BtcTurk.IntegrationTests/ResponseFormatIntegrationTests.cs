using System.Text.RegularExpressions;
using TRCrypto.BtcTurk.Clients;
using Xunit;
using Xunit.Abstractions;

namespace TRCrypto.BtcTurk.IntegrationTests;

/// <summary>
/// Canli yanitlarin bicimini, degerleri aciga cikarmadan dogrular.
/// </summary>
/// <remarks>
/// Resmi dokumantasyon bakiye tutarlarini virgul ayiricili gosterir. Bu, kutuphanenin
/// ondalik cozumunu dogrudan etkileyen bir ayrintidir ve yalnizca canli yanitla
/// dogrulanabilir.
/// <para>
/// Testler tutarlari <b>yazdirmaz</b>; ham yanitta yalnizca ayirici karakterin varligini
/// olcer ve sayilari maskeler.
/// </para>
/// </remarks>
public class ResponseFormatIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public ResponseFormatIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableFact]
    public async Task Bakiye_yanitindaki_ondalik_ayirici_belirlenir()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var client = new BtcTurkRestClient(options =>
        {
            options.ApiCredentials = TestCredentials.Create();
            options.OutputOriginalData = true;
        });

        var result = await client.SpotApi.Account.GetBalancesAsync();
        Skip.IfNot(result.Success, $"Istek basarisiz: {result.Error}");

        var raw = result.OriginalData;
        Skip.If(string.IsNullOrEmpty(raw), "Ham yanit alinamadi.");

        // Yalnizca tirnak icindeki sayisal alanlarin ayiricisina bakilir; degerler
        // hicbir zaman ciktiya yazilmaz.
        var quotedNumbers = Regex.Matches(raw!, @"""(-?\d+[.,]\d+)""")
            .Select(m => m.Groups[1].Value)
            .ToArray();

        var withComma = quotedNumbers.Count(x => x.Contains(','));
        var withDot = quotedNumbers.Count(x => x.Contains('.'));

        _output.WriteLine($"Ondalikli sayi alani : {quotedNumbers.Length}");
        _output.WriteLine($"  virgul ayiricili   : {withComma}");
        _output.WriteLine($"  nokta ayiricili    : {withDot}");
        _output.WriteLine(withComma > 0
            ? "SONUC: bu uc VIRGUL kullaniyor (dokumantasyonla uyumlu)"
            : "SONUC: bu uc NOKTA kullaniyor (dokumantasyondaki ornek yaniltici)");

        // Her iki bicim de desteklendigi icin test basarisiz olmaz; amac gercek
        // davranisi kayit altina almaktir.
        Assert.True(quotedNumbers.Length > 0, "Yanitta ondalikli sayi alani bulunamadi.");
    }

    [SkippableFact]
    public async Task Bakiye_buyuklukleri_makul_araliktadir()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var client = new BtcTurkRestClient(options =>
            options.ApiCredentials = TestCredentials.Create());

        var result = await client.SpotApi.Account.GetBalancesAsync();
        Skip.IfNot(result.Success, $"Istek basarisiz: {result.Error}");

        // Ayirici yanlis yorumlanirsa tutar katlarca buyur. Gercek tutari bilmeden de
        // bu hata yakalanabilir: hicbir bakiye asiri buyuk olmamalidir.
        foreach (var balance in result.Data)
        {
            Assert.True(
                balance.Total < 1_000_000_000_000m,
                $"{balance.Asset}: tutar buyuklugu hatali, ondalik ayirici yanlis cozulmus olabilir.");

            Assert.True(balance.Total >= 0, $"{balance.Asset}: negatif toplam bakiye.");
        }

        _output.WriteLine($"{result.Data.Count} varligin buyuklugu makul araliktadir");
    }
}
