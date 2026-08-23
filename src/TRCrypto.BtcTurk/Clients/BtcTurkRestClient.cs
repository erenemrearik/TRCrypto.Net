using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Options;
using TRCrypto.BtcTurk.Clients.SpotApi;
using TRCrypto.BtcTurk.Interfaces.Clients;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;
using TRCrypto.BtcTurk.Objects.Options;

namespace TRCrypto.BtcTurk.Clients;

/// <inheritdoc cref="IBtcTurkRestClient" />
public class BtcTurkRestClient : BaseRestClient<BtcTurkEnvironment, BtcTurkCredentials>, IBtcTurkRestClient
{
    /// <inheritdoc />
    public IBtcTurkRestClientSpotApi SpotApi { get; }

    /// <summary>Verilen seceneklerle yeni bir istemci olusturur.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public BtcTurkRestClient(Action<BtcTurkRestOptions>? optionsDelegate = null)
        : this(null, null, Options.Create(ApplyOptionsDelegate(optionsDelegate)))
    {
    }

    /// <summary>Yeni bir istemci olusturur.</summary>
    /// <param name="httpClient">Kullanilacak HTTP istemcisi.</param>
    /// <param name="loggerFactory">Gunluk fabrikasi.</param>
    /// <param name="options">Secenekler.</param>
    public BtcTurkRestClient(HttpClient? httpClient, ILoggerFactory? loggerFactory, IOptions<BtcTurkRestOptions> options)
        : base(loggerFactory, BtcTurkExchange.ExchangeName)
    {
        Initialize(options.Value);

        SpotApi = AddApiClient(new BtcTurkRestClientSpotApi(loggerFactory, httpClient, options.Value));
    }

    /// <summary>Yeni istemciler icin varsayilan secenekleri belirler.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public static void SetDefaultOptions(Action<BtcTurkRestOptions> optionsDelegate)
    {
        BtcTurkRestOptions.Default = ApplyOptionsDelegate(optionsDelegate);
    }

    /// <inheritdoc />
    public override void SetApiCredentials(BtcTurkCredentials credentials)
    {
        SpotApi.SetApiCredentials(credentials);
    }
}
