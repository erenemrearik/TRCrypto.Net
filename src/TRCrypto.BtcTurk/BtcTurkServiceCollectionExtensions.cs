using CryptoExchange.Net.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TRCrypto.BtcTurk.Clients;
using TRCrypto.BtcTurk.Interfaces.Clients;
using TRCrypto.BtcTurk.Objects.Options;

namespace TRCrypto.BtcTurk;

/// <summary>BtcTurk istemcilerini bagimlilik enjeksiyonuna kaydeder.</summary>
public static class BtcTurkServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="IBtcTurkRestClient"/> ve <see cref="IBtcTurkSocketClient"/> kaydini yapar.
    /// </summary>
    /// <remarks>
    /// Istemciler yeniden kullanilabilir; enjekte edilen ornegi saklayin, her istek icin
    /// yenisini olusturmayin (spesifikasyon Bolum 10.1).
    /// </remarks>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    /// <returns>Zincirleme icin servis koleksiyonu.</returns>
    public static IServiceCollection AddTRCryptoBtcTurk(
        this IServiceCollection services,
        Action<BtcTurkOptions>? optionsDelegate = null)
    {
        var options = new BtcTurkOptions();
        optionsDelegate?.Invoke(options);

        if (options.ApiCredentials != null)
        {
            options.Rest.ApiCredentials ??= options.ApiCredentials;
            options.Socket.ApiCredentials ??= options.ApiCredentials;
        }

        options.Rest.Environment ??= BtcTurkEnvironment.Live;
        options.Socket.Environment ??= BtcTurkEnvironment.Live;

        services.Configure<BtcTurkRestOptions>(o => options.Rest.Set(o));
        services.Configure<BtcTurkSocketOptions>(o => options.Socket.Set(o));

        services.AddHttpClient<IBtcTurkRestClient, BtcTurkRestClient>(httpClient =>
        {
            httpClient.Timeout = options.Rest.RequestTimeout;
        });

        // Socket istemcisi acik baglantilari tasir ve abonelikleri kendi icinde birlestirir;
        // her tuketiciye yeni bir ornek verilirse baglantilar gereksiz yere cogalir.
        // Uretici acikca secilir: istemcinin hem IOptions hem de temsilci alan bir kurucusu
        // vardir ve kapsayici ikisi arasinda secim yapamaz.
        services.AddSingleton<IBtcTurkSocketClient>(provider => new BtcTurkSocketClient(
            provider.GetRequiredService<IOptions<BtcTurkSocketOptions>>(),
            provider.GetService<ILoggerFactory>()));

        return services;
    }
}
