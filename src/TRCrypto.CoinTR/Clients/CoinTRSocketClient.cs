using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Options;
using TRCrypto.CoinTR.Clients.SpotApi;
using TRCrypto.CoinTR.Interfaces.Clients;
using TRCrypto.CoinTR.Interfaces.Clients.SpotApi;
using TRCrypto.CoinTR.Objects.Options;

namespace TRCrypto.CoinTR.Clients;

/// <inheritdoc cref="ICoinTRSocketClient" />
public class CoinTRSocketClient
    : BaseSocketClient<CoinTREnvironment, CoinTRCredentials>, ICoinTRSocketClient
{
    /// <inheritdoc />
    public ICoinTRSocketClientSpotApi SpotApi { get; }

    /// <summary>Verilen seceneklerle yeni bir istemci olusturur.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public CoinTRSocketClient(Action<CoinTRSocketOptions>? optionsDelegate = null)
        : this(Options.Create(ApplyOptionsDelegate(optionsDelegate)), null)
    {
    }

    /// <summary>Yeni bir istemci olusturur.</summary>
    /// <param name="options">Secenekler.</param>
    /// <param name="loggerFactory">Gunluk fabrikasi.</param>
    public CoinTRSocketClient(
        IOptions<CoinTRSocketOptions> options, ILoggerFactory? loggerFactory = null)
        : base(loggerFactory, CoinTRExchange.ExchangeName)
    {
        Initialize(options.Value);

        SpotApi = AddApiClient(new CoinTRSocketClientSpotApi(loggerFactory, options.Value));
    }

    /// <summary>Yeni istemciler icin varsayilan secenekleri belirler.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public static void SetDefaultOptions(Action<CoinTRSocketOptions> optionsDelegate)
    {
        CoinTRSocketOptions.Default = ApplyOptionsDelegate(optionsDelegate);
    }

    /// <inheritdoc />
    public override void SetApiCredentials(CoinTRCredentials credentials)
    {
        SpotApi.SetApiCredentials(credentials);
    }
}
