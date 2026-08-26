using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Options;
using TRCrypto.BtcTurk.Clients.SpotApi;
using TRCrypto.BtcTurk.Interfaces.Clients;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;
using TRCrypto.BtcTurk.Objects.Options;

namespace TRCrypto.BtcTurk.Clients;

/// <inheritdoc cref="IBtcTurkSocketClient" />
public class BtcTurkSocketClient : BaseSocketClient<BtcTurkEnvironment, BtcTurkCredentials>, IBtcTurkSocketClient
{
    /// <inheritdoc />
    public IBtcTurkSocketClientSpotApi SpotApi { get; }

    /// <summary>Verilen seceneklerle yeni bir istemci olusturur.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public BtcTurkSocketClient(Action<BtcTurkSocketOptions>? optionsDelegate = null)
        : this(Options.Create(ApplyOptionsDelegate(optionsDelegate)), null)
    {
    }

    /// <summary>Yeni bir istemci olusturur.</summary>
    /// <param name="options">Secenekler.</param>
    /// <param name="loggerFactory">Gunluk fabrikasi.</param>
    public BtcTurkSocketClient(IOptions<BtcTurkSocketOptions> options, ILoggerFactory? loggerFactory = null)
        : base(loggerFactory, BtcTurkExchange.ExchangeName)
    {
        Initialize(options.Value);

        SpotApi = AddApiClient(new BtcTurkSocketClientSpotApi(loggerFactory, options.Value));
    }

    /// <summary>Yeni istemciler icin varsayilan secenekleri belirler.</summary>
    /// <param name="optionsDelegate">Secenek yapilandirma temsilcisi.</param>
    public static void SetDefaultOptions(Action<BtcTurkSocketOptions> optionsDelegate)
    {
        BtcTurkSocketOptions.Default = ApplyOptionsDelegate(optionsDelegate);
    }

    /// <inheritdoc />
    public override void SetApiCredentials(BtcTurkCredentials credentials)
    {
        SpotApi.SetApiCredentials(credentials);
    }
}
