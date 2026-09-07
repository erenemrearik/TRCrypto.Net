using CryptoExchange.Net.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TRCrypto.CoinTR.Interfaces.Clients;
using Xunit;

namespace TRCrypto.CoinTR.UnitTests;

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
        using var provider = new ServiceCollection().AddTRCryptoCoinTR().BuildServiceProvider();

        var client = provider.GetRequiredService<ICoinTRRestClient>();

        Assert.NotNull(client.SpotApi.ExchangeData);
    }

    [Fact]
    public void Socket_istemcisi_cozumlenebilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoCoinTR().BuildServiceProvider();

        var client = provider.GetRequiredService<ICoinTRSocketClient>();

        Assert.NotNull(client.SpotApi);
    }

    [Fact]
    public void Socket_istemcisi_tekil_olarak_paylasilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoCoinTR().BuildServiceProvider();

        // Bir baglanti uzerinden birden fazla kanala abone olunabilir; istemciyi
        // cogaltmak baglanti sayisini gereksiz yere katlar.
        Assert.Same(
            provider.GetRequiredService<ICoinTRSocketClient>(),
            provider.GetRequiredService<ICoinTRSocketClient>());
    }

    [Fact]
    public void Kimlik_bilgisi_verilmezse_istemciler_anonim_kalir()
    {
        using var provider = new ServiceCollection().AddTRCryptoCoinTR().BuildServiceProvider();

        // Public uclar ve akislar anahtarsiz calisir.
        Assert.False(provider.GetRequiredService<ICoinTRRestClient>().SpotApi.Authenticated);
    }

    [Fact]
    public void Ortak_HTTP_istemci_fabrikasi_kaydedilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoCoinTR().BuildServiceProvider();

        Assert.NotNull(provider.GetService<IHttpClientFactory>());
    }

    [Fact]
    public void Tek_kimlik_bilgisi_her_iki_istemciye_dagitilir()
    {
        // Belgelenen kisayol: cogu uygulama ayni anahtari her iki istemcide kullanir.
        // Degerler gercekci gorunumlu ve SAHTEDIR.
        using var provider = new ServiceCollection()
            .AddTRCryptoCoinTR(options =>
                options.ApiCredentials = new CoinTRCredentials("anahtar", "gizli", "parola"))
            .BuildServiceProvider();

        Assert.True(provider.GetRequiredService<ICoinTRRestClient>().SpotApi.Authenticated);
        Assert.True(provider.GetRequiredService<ICoinTRSocketClient>().SpotApi.Authenticated);
    }
}
