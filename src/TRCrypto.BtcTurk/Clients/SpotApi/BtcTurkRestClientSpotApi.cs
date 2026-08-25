using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk.Clients.MessageHandlers;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Options;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <inheritdoc cref="IBtcTurkRestClientSpotApi" />
internal partial class BtcTurkRestClientSpotApi
    : RestApiClient<BtcTurkEnvironment, BtcTurkAuthenticationProvider, BtcTurkCredentials>, IBtcTurkRestClientSpotApi
{
    /// <inheritdoc />
    public new BtcTurkRestOptions ClientOptions => (BtcTurkRestOptions)base.ClientOptions;

    protected override ErrorMapping ErrorMapping => BtcTurkErrors.Mapping;

    protected override IRestMessageHandler MessageHandler { get; } =
        new BtcTurkRestMessageHandler(BtcTurkErrors.Mapping);

    /// <inheritdoc />
    public IBtcTurkRestClientSpotApiExchangeData ExchangeData { get; }

    /// <inheritdoc />
    public IBtcTurkRestClientSpotApiAccount Account { get; }

    /// <inheritdoc />
    public IBtcTurkRestClientSpotApiTrading Trading { get; }

    /// <summary>Borsadan bagimsiz (shared) yuzey.</summary>
    public IBtcTurkRestClientSpotApiShared SharedClient => this;

    internal BtcTurkRestClientSpotApi(ILoggerFactory? loggerFactory, HttpClient? httpClient, BtcTurkRestOptions options)
        : base(loggerFactory, BtcTurkExchange.ExchangeName, httpClient, options.Environment.RestBaseAddress, options, options.SpotOptions)
    {
        ExchangeData = new BtcTurkRestClientSpotApiExchangeData(this);
        Account = new BtcTurkRestClientSpotApiAccount(this);
        Trading = new BtcTurkRestClientSpotApiTrading(this);
    }

    /// <inheritdoc />
    public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
        => BtcTurkExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

    /// <inheritdoc />
    protected override BtcTurkAuthenticationProvider CreateAuthenticationProvider(BtcTurkCredentials credentials)
        => new(credentials);

    protected override IMessageSerializer CreateSerializer()
        => new SystemTextJsonMessageSerializer(BtcTurkJsonOptions.Default);

    /// <summary>
    /// BtcTurk zarfini acar ve icindeki veriyi dondurur.
    /// </summary>
    /// <remarks>
    /// Zarftaki <c>success</c> kontrolu <see cref="BtcTurkRestMessageHandler"/> icinde yapilir;
    /// buraya yalnizca basarili yanitlar ulasir.
    /// </remarks>
    /// <summary>Govde tasimayan yanitlar icin zarfi acar.</summary>
    internal async Task<HttpResult> SendAsync(
        RequestDefinition definition,
        Parameters? parameters,
        CancellationToken cancellationToken,
        int? weight = null)
    {
        var result = await base.SendAsync<BtcTurkResponse<object>>(definition, parameters, cancellationToken, null, weight)
            .ConfigureAwait(false);

        return result.Success ? HttpResult.Ok(result) : HttpResult.Fail(result);
    }

    internal async Task<HttpResult<T>> SendAsync<T>(
        RequestDefinition definition,
        Parameters? parameters,
        CancellationToken cancellationToken,
        int? weight = null)
    {
        var result = await base.SendAsync<BtcTurkResponse<T>>(definition, parameters, cancellationToken, null, weight)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<T>(result);

        return HttpResult.Ok(result, result.Data.Data!);
    }

    /// <inheritdoc />
    protected override Task<HttpResult<DateTime>> GetServerTimestampAsync()
        => ExchangeData.GetServerTimeAsync();
}
