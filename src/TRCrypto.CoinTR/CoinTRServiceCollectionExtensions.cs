using CryptoExchange.Net.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TRCrypto.CoinTR.Clients;
using TRCrypto.CoinTR.Interfaces.Clients;
using TRCrypto.CoinTR.Objects.Options;

namespace TRCrypto.CoinTR;

/// <summary>CoinTR istemcilerini bagimlilik enjeksiyonuna kaydeder.</summary>
public static class CoinTRServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="ICoinTRRestClient"/> ve <see cref="ICoinTRSocketClient"/> kaydini yapar.
    /// </summary>
    /// <remarks>
    /// Istemciler yeniden kullanilabilir; enjekte edilen ornegi saklayin, her istek icin
    /// yenisini olusturmayin.
    /// </remarks>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    /// <returns>Zincirleme icin servis koleksiyonu.</returns>
    public static IServiceCollection AddTRCryptoCoinTR(
        this IServiceCollection services,
        Action<CoinTROptions>? optionsDelegate = null)
    {
        var options = new CoinTROptions();
        optionsDelegate?.Invoke(options);

        if (options.ApiCredentials != null)
        {
            options.Rest.ApiCredentials ??= options.ApiCredentials;
            options.Socket.ApiCredentials ??= options.ApiCredentials;
        }

        options.Rest.Environment ??= CoinTREnvironment.Live;
        options.Socket.Environment ??= CoinTREnvironment.Live;

        services.Configure<CoinTRRestOptions>(o => options.Rest.Set(o));
        services.Configure<CoinTRSocketOptions>(o => options.Socket.Set(o));

        services.AddHttpClient<ICoinTRRestClient, CoinTRRestClient>(httpClient =>
        {
            httpClient.Timeout = options.Rest.RequestTimeout;
        });

        // Akis istemcisi birden fazla abonelik icin tek baglanti kullanir; cogaltmak
        // baglanti sayisini gereksiz yere katlar.
        // Uretici acikca secilir: istemcinin hem IOptions hem de temsilci alan bir kurucusu
        // vardir ve kapsayici ikisi arasinda secim yapamaz.
        services.AddSingleton<ICoinTRSocketClient>(provider => new CoinTRSocketClient(
            provider.GetRequiredService<IOptions<CoinTRSocketOptions>>(),
            provider.GetService<ILoggerFactory>()));

        return services;
    }
}
