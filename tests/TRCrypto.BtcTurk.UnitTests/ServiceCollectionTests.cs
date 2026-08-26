using CryptoExchange.Net.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TRCrypto.BtcTurk;
using TRCrypto.BtcTurk.Interfaces.Clients;
using TRCrypto.BtcTurk.Objects.Options;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Bagimlilik enjeksiyonu kaydinin dogru kurulumu urettigini dogrular.
/// </summary>
/// <remarks>
/// Bu testler ag erisimi yapmaz; yalnizca cozumleme ve yasam suresini inceler.
/// </remarks>
public class ServiceCollectionTests
{
    [Fact]
    public void REST_istemcisi_cozumlenebilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBtcTurk().BuildServiceProvider();

        var client = provider.GetRequiredService<IBtcTurkRestClient>();

        Assert.NotNull(client.SpotApi.ExchangeData);
    }

    [Fact]
    public void Socket_istemcisi_cozumlenebilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBtcTurk().BuildServiceProvider();

        var client = provider.GetRequiredService<IBtcTurkSocketClient>();

        Assert.NotNull(client.SpotApi);
    }

    [Fact]
    public void Socket_istemcisi_tekil_olarak_paylasilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBtcTurk().BuildServiceProvider();

        // Socket istemcisi acik baglantilari tasir; her cozumlemede yenisi olusursa
        // her tuketici kendi baglantisini acar ve borsanin baglanti siniri asilir.
        Assert.Same(
            provider.GetRequiredService<IBtcTurkSocketClient>(),
            provider.GetRequiredService<IBtcTurkSocketClient>());
    }

    [Fact]
    public void Secenekler_her_iki_istemciye_de_uygulanir()
    {
        var credentials = new BtcTurkCredentials("anahtar", "Z2l6bGk=");

        using var provider = new ServiceCollection()
            .AddTRCryptoBtcTurk(options =>
            {
                options.Rest.ApiCredentials = credentials;
                options.Socket.ApiCredentials = credentials;
            })
            .BuildServiceProvider();

        Assert.True(provider.GetRequiredService<IBtcTurkRestClient>().SpotApi.Authenticated);
        Assert.True(provider.GetRequiredService<IBtcTurkSocketClient>().SpotApi.Authenticated);
    }

    [Fact]
    public void Kimlik_bilgisi_verilmezse_istemciler_anonim_kalir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBtcTurk().BuildServiceProvider();

        Assert.False(provider.GetRequiredService<IBtcTurkRestClient>().SpotApi.Authenticated);
    }

    [Fact]
    public void Ortak_HTTP_istemci_fabrikasi_kaydedilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBtcTurk().BuildServiceProvider();

        // REST istemcisi soket paylasimli bir HttpClient uzerinden calismalidir;
        // her ornek icin yeni HttpClient soket tukenmesine yol acar.
        Assert.NotNull(provider.GetService<IHttpClientFactory>());
    }

    [Fact]
    public void Tek_kimlik_bilgisi_her_iki_istemciye_dagitilir()
    {
        // Belgelenen kisayol: cogu uygulama ayni anahtari her iki istemcide kullanir.
        using var provider = new ServiceCollection()
            .AddTRCryptoBtcTurk(options => options.ApiCredentials = new BtcTurkCredentials("anahtar", "Z2l6bGk="))
            .BuildServiceProvider();

        Assert.True(provider.GetRequiredService<IBtcTurkRestClient>().SpotApi.Authenticated);
        Assert.True(provider.GetRequiredService<IBtcTurkSocketClient>().SpotApi.Authenticated);
    }
}
