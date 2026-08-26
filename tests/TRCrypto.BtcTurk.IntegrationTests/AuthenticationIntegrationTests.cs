using TRCrypto.BtcTurk.Clients;
using Xunit;
using Xunit.Abstractions;

namespace TRCrypto.BtcTurk.IntegrationTests;

/// <summary>
/// Imzalamanin gercek bir hesaba karsi calistigini dogrular.
/// </summary>
/// <remarks>
/// <para>
/// Bu testler yalnizca <b>okuma</b> yapar; emir olusturmaz, bakiye degistirmez.
/// Kimlik bilgisi tanimli degilse atlanir.
/// </para>
/// <para>
/// <b>Bakiye tutarlari hicbir zaman ciktiya yazilmaz.</b> Testler yalnizca istegin
/// basarili olup olmadigini ve verinin bicimini olcer; degerlerin kendisi hassastir.
/// </para>
/// </remarks>
public class AuthenticationIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public AuthenticationIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static BtcTurkRestClient CreateClient()
        => new(options => options.ApiCredentials = TestCredentials.Create());

    [SkippableFact]
    public async Task Kimlik_bilgisi_tanimli()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        // Degerler yazdirilmaz; yalnizca varliklari ve kaba bicimleri dogrulanir.
        _output.WriteLine($"ApiKey uzunlugu    : {TestCredentials.ApiKey!.Length}");
        _output.WriteLine($"ApiSecret uzunlugu : {TestCredentials.ApiSecret!.Length}");

        // BtcTurk secret'i Base64'tur; cozulemiyorsa imzalama sessizce yanlis olur.
        var decoded = System.Convert.FromBase64String(TestCredentials.ApiSecret!);
        _output.WriteLine($"Secret Base64 cozuldu, {decoded.Length} bayt");

        Assert.True(decoded.Length > 0);
    }

    [SkippableFact]
    public async Task Imzali_istek_borsa_tarafindan_kabul_ediliyor()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var client = CreateClient();

        // Bakiye ucu en dusuk yetkiyle calisan uctur: yalnizca "Toplam Varlik" izni ister.
        var result = await client.SpotApi.Account.GetBalancesAsync();

        _output.WriteLine($"Success : {result.Success}");
        if (!result.Success)
            _output.WriteLine($"Error   : {result.Error}");

        Assert.True(result.Success, $"Imzali istek reddedildi: {result.Error}");
    }

    [SkippableFact]
    public async Task Bakiye_yaniti_beklenen_alanlari_tasiyor()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var client = CreateClient();
        var result = await client.SpotApi.Account.GetBalancesAsync();

        Skip.IfNot(result.Success, $"Istek basarisiz: {result.Error}");

        // Tutarlar degil, yalnizca sayilari ve alan varligi raporlanir.
        _output.WriteLine($"Varlik sayisi : {result.Data.Count}");
        Assert.NotEmpty(result.Data);

        var first = result.Data[0];
        Assert.False(string.IsNullOrEmpty(first.Asset));
        Assert.NotEqual(default, first.Timestamp);
    }

    [SkippableFact]
    public async Task Bakiye_toplami_serbest_ve_kilitli_ile_tutarli()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var client = CreateClient();
        var result = await client.SpotApi.Account.GetBalancesAsync();

        Skip.IfNot(result.Success, $"Istek basarisiz: {result.Error}");

        // Ondalik ayirici yanlis yorumlanirsa bu esitlik bozulur; bu yuzden burasi
        // ayni zamanda virgul/nokta cozumunun canli dogrulamasidir.
        foreach (var balance in result.Data)
        {
            Assert.True(
                balance.Total == balance.Available + balance.Locked,
                $"{balance.Asset}: toplam, serbest + kilitli ile esit degil.");
        }

        _output.WriteLine($"{result.Data.Count} varlikta toplam = serbest + kilitli dogrulandi");
    }
}
