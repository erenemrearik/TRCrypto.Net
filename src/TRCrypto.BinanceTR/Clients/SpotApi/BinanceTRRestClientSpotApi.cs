using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Clients.MessageHandlers;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;
using TRCrypto.BinanceTR.Objects.Internal;
using TRCrypto.BinanceTR.Objects.Options;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <inheritdoc cref="IBinanceTRRestClientSpotApi" />
internal partial class BinanceTRRestClientSpotApi
    : RestApiClient<BinanceTREnvironment, BinanceTRAuthenticationProvider, BinanceTRCredentials>,
      IBinanceTRRestClientSpotApi
{
    /// <inheritdoc />
    public new BinanceTRRestOptions ClientOptions => (BinanceTRRestOptions)base.ClientOptions;

    protected override ErrorMapping ErrorMapping => BinanceTRErrors.Mapping;

    protected override IRestMessageHandler MessageHandler { get; } =
        new BinanceTRRestMessageHandler(BinanceTRErrors.Mapping);

    /// <inheritdoc />
    public IBinanceTRRestClientSpotApiExchangeData ExchangeData { get; }

    internal BinanceTRRestClientSpotApi(
        ILoggerFactory? loggerFactory, HttpClient? httpClient, BinanceTRRestOptions options)
        : base(
            loggerFactory,
            BinanceTRExchange.ExchangeName,
            httpClient,
            options.Environment.RestBaseAddress,
            options,
            options.SpotOptions)
    {
        ExchangeData = new BinanceTRRestClientSpotApiExchangeData(this);
    }

    /// <inheritdoc />
    public override string FormatSymbol(
        string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
        => BinanceTRExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

    /// <inheritdoc />
    protected override BinanceTRAuthenticationProvider CreateAuthenticationProvider(
        BinanceTRCredentials credentials)
        => new(credentials);

    protected override IMessageSerializer CreateSerializer()
        => new SystemTextJsonMessageSerializer(BinanceTRJsonOptions.Default);

    /// <summary>
    /// Zarfi acar ve icindeki veriyi dondurur.
    /// </summary>
    /// <remarks>
    /// Basari kontrolu <see cref="BinanceTRRestMessageHandler"/> icinde yapilir; buraya
    /// yalnizca basarili yanitlar ulasir.
    /// </remarks>
    internal async Task<HttpResult<T>> SendAsync<T>(
        RequestDefinition definition,
        Parameters? parameters,
        CancellationToken cancellationToken,
        int? weight = null)
    {
        var result = await SendEnvelopeAsync<T>(definition, parameters, cancellationToken, weight)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<T>(result);

        return HttpResult.Ok(result, result.Data.Data!);
    }

    /// <summary>
    /// Zarfi acmadan dondurur.
    /// </summary>
    /// <remarks>
    /// Bazi uclar govdesinde veri tasimaz ve istenen bilgi yalnizca zarfta bulunur;
    /// sunucu saati ucu bunun ornegidir.
    /// </remarks>
    internal Task<HttpResult<BinanceTRResponse<T>>> SendEnvelopeAsync<T>(
        RequestDefinition definition,
        Parameters? parameters,
        CancellationToken cancellationToken,
        int? weight = null)
        => base.SendAsync<BinanceTRResponse<T>>(definition, parameters, cancellationToken, null, weight);

    /// <inheritdoc />
    protected override Task<HttpResult<DateTime>> GetServerTimestampAsync()
        => ExchangeData.GetServerTimeAsync();
}
