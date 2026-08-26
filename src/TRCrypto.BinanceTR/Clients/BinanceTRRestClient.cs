using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Options;
using TRCrypto.BinanceTR.Clients.SpotApi;
using TRCrypto.BinanceTR.Interfaces.Clients;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;
using TRCrypto.BinanceTR.Objects.Options;

namespace TRCrypto.BinanceTR.Clients;

/// <inheritdoc cref="IBinanceTRRestClient" />
public class BinanceTRRestClient
    : BaseRestClient<BinanceTREnvironment, BinanceTRCredentials>, IBinanceTRRestClient
{
    /// <inheritdoc />
    public IBinanceTRRestClientSpotApi SpotApi { get; }

    /// <summary>Verilen seceneklerle yeni bir istemci olusturur.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public BinanceTRRestClient(Action<BinanceTRRestOptions>? optionsDelegate = null)
        : this(null, null, Options.Create(ApplyOptionsDelegate(optionsDelegate)))
    {
    }

    /// <summary>Yeni bir istemci olusturur.</summary>
    /// <param name="httpClient">Kullanilacak HTTP istemcisi.</param>
    /// <param name="loggerFactory">Gunluk fabrikasi.</param>
    /// <param name="options">Secenekler.</param>
    public BinanceTRRestClient(
        HttpClient? httpClient, ILoggerFactory? loggerFactory, IOptions<BinanceTRRestOptions> options)
        : base(loggerFactory, BinanceTRExchange.ExchangeName)
    {
        Initialize(options.Value);

        SpotApi = AddApiClient(new BinanceTRRestClientSpotApi(loggerFactory, httpClient, options.Value));
    }

    /// <summary>Yeni istemciler icin varsayilan secenekleri belirler.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public static void SetDefaultOptions(Action<BinanceTRRestOptions> optionsDelegate)
    {
        BinanceTRRestOptions.Default = ApplyOptionsDelegate(optionsDelegate);
    }

    /// <inheritdoc />
    public override void SetApiCredentials(BinanceTRCredentials credentials)
    {
        SpotApi.SetApiCredentials(credentials);
    }
}
