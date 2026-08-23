using CryptoExchange.Net.Clients;
using Microsoft.Extensions.DependencyInjection;
using TRCrypto.BtcTurk.Clients;
using TRCrypto.BtcTurk.Interfaces.Clients;
using TRCrypto.BtcTurk.Objects.Options;

namespace TRCrypto.BtcTurk;

/// <summary>BtcTurk istemcilerini bagimlilik enjeksiyonuna kaydeder.</summary>
public static class BtcTurkServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="IBtcTurkRestClient"/> kaydini yapar.
    /// </summary>
    /// <remarks>
    /// Istemci yeniden kullanilabilir; enjekte edilen ornegi saklayin, her istek icin
    /// yenisini olusturmayin (spesifikasyon Bolum 10.1).
    /// </remarks>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    /// <returns>Zincirleme icin servis koleksiyonu.</returns>
    public static IServiceCollection AddTRCryptoBtcTurk(
        this IServiceCollection services,
        Action<BtcTurkRestOptions>? optionsDelegate = null)
    {
        var options = new BtcTurkRestOptions();
        optionsDelegate?.Invoke(options);
        options.Environment ??= BtcTurkEnvironment.Live;

        services.Configure<BtcTurkRestOptions>(o => options.Set(o));

        services.AddHttpClient<IBtcTurkRestClient, BtcTurkRestClient>(httpClient =>
        {
            httpClient.Timeout = options.RequestTimeout;
        });

        return services;
    }
}
