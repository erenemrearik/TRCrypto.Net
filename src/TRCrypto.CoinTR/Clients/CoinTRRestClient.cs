using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Options;
using TRCrypto.CoinTR.Clients.SpotApi;
using TRCrypto.CoinTR.Interfaces.Clients;
using TRCrypto.CoinTR.Interfaces.Clients.SpotApi;
using TRCrypto.CoinTR.Objects.Options;

namespace TRCrypto.CoinTR.Clients;

/// <inheritdoc cref="ICoinTRRestClient" />
public class CoinTRRestClient
    : BaseRestClient<CoinTREnvironment, CoinTRCredentials>, ICoinTRRestClient
{
    /// <inheritdoc />
    public ICoinTRRestClientSpotApi SpotApi { get; }

    /// <summary>Verilen seceneklerle yeni bir istemci olusturur.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public CoinTRRestClient(Action<CoinTRRestOptions>? optionsDelegate = null)
        : this(null, null, Options.Create(ApplyOptionsDelegate(optionsDelegate)))
    {
    }

    /// <summary>Yeni bir istemci olusturur.</summary>
    /// <param name="httpClient">Kullanilacak HTTP istemcisi.</param>
    /// <param name="loggerFactory">Gunluk fabrikasi.</param>
    /// <param name="options">Secenekler.</param>
    public CoinTRRestClient(
        HttpClient? httpClient, ILoggerFactory? loggerFactory, IOptions<CoinTRRestOptions> options)
        : base(loggerFactory, CoinTRExchange.ExchangeName)
    {
        Initialize(options.Value);

        SpotApi = AddApiClient(new CoinTRRestClientSpotApi(loggerFactory, httpClient, options.Value));
    }

    /// <summary>Yeni istemciler icin varsayilan secenekleri belirler.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public static void SetDefaultOptions(Action<CoinTRRestOptions> optionsDelegate)
    {
        CoinTRRestOptions.Default = ApplyOptionsDelegate(optionsDelegate);
    }

    /// <inheritdoc />
    public override void SetApiCredentials(CoinTRCredentials credentials)
    {
        SpotApi.SetApiCredentials(credentials);
    }
}
