using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;
using TRCrypto.BinanceTR.Objects.Models;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <inheritdoc />
internal class BinanceTRRestClientSpotApiExchangeData : IBinanceTRRestClientSpotApiExchangeData
{
    /// <summary>
    /// Emir defteri ucunun kabul ettigi kademe sayilari.
    /// </summary>
    /// <remarks>
    /// Bu degerler canli API ile belirlenmistir. Liste disindaki bir deger, sebebi
    /// belirtmeyen ve yaniltici olan bir "Incorrect Page number" hatasiyla reddedilir;
    /// bu yuzden istek aga cikmadan once dogrulanir.
    /// </remarks>
    private static readonly int[] _orderBookLimits = [5, 10, 20, 50, 100, 500, 1000];

    private static readonly RequestDefinitionCache _definitions = new();
    private readonly BinanceTRRestClientSpotApi _baseClient;

    internal BinanceTRRestClientSpotApiExchangeData(BinanceTRRestClientSpotApi baseClient)
    {
        _baseClient = baseClient;
    }

    /// <inheritdoc />
    public async Task<HttpResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/common/time",
            BinanceTRExchange.RateLimiter.Rest, 1, false);

        // Bu uc govdesinde veri tasimaz; saat yalnizca zarftadir.
        var result = await _baseClient.SendEnvelopeAsync<object>(request, null, ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<DateTime>(result);

        return HttpResult.Ok(result, result.Data.Timestamp);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTRExchangeInfo>> GetSymbolsAsync(CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/common/symbols",
            BinanceTRExchange.RateLimiter.Rest, 1, false);

        return await _baseClient.SendAsync<BinanceTRExchangeInfo>(request, null, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTROrderBook>> GetOrderBookAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        if (limit != null && Array.IndexOf(_orderBookLimits, limit.Value) < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                limit,
                $"Kademe sayisi su degerlerden biri olmalidir: {string.Join(", ", _orderBookLimits)}.");
        }

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings) { { "symbol", symbol } };
        parameters.Add("limit", limit);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/market/depth",
            BinanceTRExchange.RateLimiter.Rest, 1, false);

        return await _baseClient
            .SendAsync<BinanceTROrderBook>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTRAggregatedTradeList>> GetAggregatedTradesAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        if (limit is <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "Kayit sayisi pozitif olmalidir.");

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings) { { "symbol", symbol } };
        parameters.Add("limit", limit);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/market/agg-trades",
            BinanceTRExchange.RateLimiter.Rest, 1, false);

        return await _baseClient
            .SendAsync<BinanceTRAggregatedTradeList>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    private static void ValidateSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));
    }
}
