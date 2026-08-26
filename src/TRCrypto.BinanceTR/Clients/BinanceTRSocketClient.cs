using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Options;
using TRCrypto.BinanceTR.Clients.SpotApi;
using TRCrypto.BinanceTR.Interfaces.Clients;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;
using TRCrypto.BinanceTR.Objects.Options;

namespace TRCrypto.BinanceTR.Clients;

/// <inheritdoc cref="IBinanceTRSocketClient" />
public class BinanceTRSocketClient
    : BaseSocketClient<BinanceTREnvironment, BinanceTRCredentials>, IBinanceTRSocketClient
{
    /// <inheritdoc />
    public IBinanceTRSocketClientSpotApi SpotApi { get; }

    /// <summary>Verilen seceneklerle yeni bir istemci olusturur.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public BinanceTRSocketClient(Action<BinanceTRSocketOptions>? optionsDelegate = null)
        : this(Options.Create(ApplyOptionsDelegate(optionsDelegate)), null)
    {
    }

    /// <summary>Yeni bir istemci olusturur.</summary>
    /// <param name="options">Secenekler.</param>
    /// <param name="loggerFactory">Gunluk fabrikasi.</param>
    public BinanceTRSocketClient(
        IOptions<BinanceTRSocketOptions> options, ILoggerFactory? loggerFactory = null)
        : base(loggerFactory, BinanceTRExchange.ExchangeName)
    {
        Initialize(options.Value);

        SpotApi = AddApiClient(new BinanceTRSocketClientSpotApi(loggerFactory, options.Value));
    }

    /// <summary>Yeni istemciler icin varsayilan secenekleri belirler.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public static void SetDefaultOptions(Action<BinanceTRSocketOptions> optionsDelegate)
    {
        BinanceTRSocketOptions.Default = ApplyOptionsDelegate(optionsDelegate);
    }

    /// <inheritdoc />
    public override void SetApiCredentials(BinanceTRCredentials credentials)
    {
        SpotApi.SetApiCredentials(credentials);
    }
}
