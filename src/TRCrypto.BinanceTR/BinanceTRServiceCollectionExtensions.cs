using CryptoExchange.Net.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TRCrypto.BinanceTR.Clients;
using TRCrypto.BinanceTR.Interfaces.Clients;
using TRCrypto.BinanceTR.Objects.Options;

namespace TRCrypto.BinanceTR;

/// <summary>Binance TR istemcilerini bagimlilik enjeksiyonuna kaydeder.</summary>
public static class BinanceTRServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="IBinanceTRRestClient"/> ve <see cref="IBinanceTRSocketClient"/> kaydini yapar.
    /// </summary>
    /// <remarks>
    /// Istemciler yeniden kullanilabilir; enjekte edilen ornegi saklayin, her istek icin
    /// yenisini olusturmayin.
    /// </remarks>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    /// <returns>Zincirleme icin servis koleksiyonu.</returns>
    public static IServiceCollection AddTRCryptoBinanceTR(
        this IServiceCollection services,
        Action<BinanceTROptions>? optionsDelegate = null)
    {
        var options = new BinanceTROptions();
        optionsDelegate?.Invoke(options);

        if (options.ApiCredentials != null)
        {
            options.Rest.ApiCredentials ??= options.ApiCredentials;
            options.Socket.ApiCredentials ??= options.ApiCredentials;
        }

        options.Rest.Environment ??= BinanceTREnvironment.Live;
        options.Socket.Environment ??= BinanceTREnvironment.Live;

        services.Configure<BinanceTRRestOptions>(o => options.Rest.Set(o));
        services.Configure<BinanceTRSocketOptions>(o => options.Socket.Set(o));

        services.AddHttpClient<IBinanceTRRestClient, BinanceTRRestClient>(httpClient =>
        {
            httpClient.Timeout = options.Rest.RequestTimeout;
        });

        // Bu borsada her abonelik kendi baglantisini acar; istemcinin de cogaltilmasi
        // baglanti sayisini gereksiz yere katlar.
        // Uretici acikca secilir: istemcinin hem IOptions hem de temsilci alan bir kurucusu
        // vardir ve kapsayici ikisi arasinda secim yapamaz.
        services.AddSingleton<IBinanceTRSocketClient>(provider => new BinanceTRSocketClient(
            provider.GetRequiredService<IOptions<BinanceTRSocketOptions>>(),
            provider.GetService<ILoggerFactory>()));

        return services;
    }
}
