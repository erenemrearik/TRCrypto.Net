using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.SharedApis;
using TRCrypto.CoinTR.Clients.MessageHandlers;
using TRCrypto.CoinTR.Interfaces.Clients.SpotApi;
using TRCrypto.CoinTR.Objects.Internal;
using TRCrypto.CoinTR.Objects.Options;

namespace TRCrypto.CoinTR.Clients.SpotApi;

/// <inheritdoc cref="ICoinTRRestClientSpotApi" />
internal partial class CoinTRRestClientSpotApi
    : RestApiClient<CoinTREnvironment, CoinTRAuthenticationProvider, CoinTRCredentials>,
      ICoinTRRestClientSpotApi
{
    /// <inheritdoc />
    public new CoinTRRestOptions ClientOptions => (CoinTRRestOptions)base.ClientOptions;

    protected override ErrorMapping ErrorMapping => CoinTRErrors.Mapping;

    protected override IRestMessageHandler MessageHandler { get; } =
        new CoinTRRestMessageHandler(CoinTRErrors.Mapping);

    /// <inheritdoc />
    public ICoinTRRestClientSpotApiExchangeData ExchangeData { get; }

    internal CoinTRRestClientSpotApi(
        ILoggerFactory? loggerFactory, HttpClient? httpClient, CoinTRRestOptions options)
        : base(
            loggerFactory,
            CoinTRExchange.ExchangeName,
            httpClient,
            options.Environment.RestBaseAddress,
            options,
            options.SpotOptions)
    {
        ExchangeData = new CoinTRRestClientSpotApiExchangeData(this);
    }

    /// <inheritdoc />
    public ICoinTRRestClientSpotApiShared SharedClient => this;

    /// <inheritdoc />
    public override string FormatSymbol(
        string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
        => CoinTRExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

    /// <inheritdoc />
    protected override CoinTRAuthenticationProvider CreateAuthenticationProvider(
        CoinTRCredentials credentials)
        => new(credentials);

    protected override IMessageSerializer CreateSerializer()
        => new SystemTextJsonMessageSerializer(CoinTRJsonOptions.Default);

    /// <summary>
    /// Zarfi acar ve icindeki veriyi dondurur.
    /// </summary>
    /// <remarks>
    /// Basari kontrolu <see cref="CoinTRRestMessageHandler"/> icinde yapilir; buraya
    /// yalnizca basarili yanitlar ulasir.
    /// </remarks>
    internal async Task<HttpResult<T>> SendAsync<T>(
        RequestDefinition definition,
        Parameters? parameters,
        CancellationToken cancellationToken,
        int? weight = null)
    {
        var result = await base
            .SendAsync<CoinTRResponse<T>>(definition, parameters, cancellationToken, null, weight)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<T>(result);

        return HttpResult.Ok(result, result.Data.Data!);
    }

    /// <inheritdoc />
    protected override Task<HttpResult<DateTime>> GetServerTimestampAsync()
        => ExchangeData.GetServerTimeAsync();
}
