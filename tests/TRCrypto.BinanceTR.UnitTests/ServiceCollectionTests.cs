using CryptoExchange.Net.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TRCrypto.BinanceTR;
using TRCrypto.BinanceTR.Interfaces.Clients;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

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
        using var provider = new ServiceCollection().AddTRCryptoBinanceTR().BuildServiceProvider();

        var client = provider.GetRequiredService<IBinanceTRRestClient>();

        Assert.NotNull(client.SpotApi.ExchangeData);
    }

    [Fact]
    public void Socket_istemcisi_cozumlenebilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBinanceTR().BuildServiceProvider();

        var client = provider.GetRequiredService<IBinanceTRSocketClient>();

        Assert.NotNull(client.SpotApi);
    }

    [Fact]
    public void Socket_istemcisi_tekil_olarak_paylasilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBinanceTR().BuildServiceProvider();

        // Bu borsada her abonelik kendi baglantisini acar; istemcinin de cogaltilmasi
        // baglanti sayisini gereksiz yere katlar.
        Assert.Same(
            provider.GetRequiredService<IBinanceTRSocketClient>(),
            provider.GetRequiredService<IBinanceTRSocketClient>());
    }

    [Fact]
    public void Kimlik_bilgisi_verilmezse_istemciler_anonim_kalir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBinanceTR().BuildServiceProvider();

        // Bu surumde kimlik dogrulama devre disi; public uclar anahtarsiz calisir.
        Assert.False(provider.GetRequiredService<IBinanceTRRestClient>().SpotApi.Authenticated);
    }

    [Fact]
    public void Ortak_HTTP_istemci_fabrikasi_kaydedilir()
    {
        using var provider = new ServiceCollection().AddTRCryptoBinanceTR().BuildServiceProvider();

        Assert.NotNull(provider.GetService<IHttpClientFactory>());
    }

    [Fact]
    public void Tek_kimlik_bilgisi_her_iki_istemciye_dagitilir()
    {
        // Belgelenen kisayol: cogu uygulama ayni anahtari her iki istemcide kullanir.
        using var provider = new ServiceCollection()
            .AddTRCryptoBinanceTR(options => options.ApiCredentials = new BinanceTRCredentials("anahtar", "gizli"))
            .BuildServiceProvider();

        Assert.True(provider.GetRequiredService<IBinanceTRRestClient>().SpotApi.Authenticated);
        Assert.True(provider.GetRequiredService<IBinanceTRSocketClient>().SpotApi.Authenticated);
    }
}
