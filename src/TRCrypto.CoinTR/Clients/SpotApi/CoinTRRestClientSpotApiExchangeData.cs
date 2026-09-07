using TRCrypto.CoinTR.Interfaces.Clients.SpotApi;
using TRCrypto.CoinTR.Objects.Models;

namespace TRCrypto.CoinTR.Clients.SpotApi;

/// <inheritdoc />
internal class CoinTRRestClientSpotApiExchangeData : ICoinTRRestClientSpotApiExchangeData
{
    private static readonly RequestDefinitionCache _definitions = new();
    private readonly CoinTRRestClientSpotApi _baseClient;

    internal CoinTRRestClientSpotApiExchangeData(CoinTRRestClientSpotApi baseClient)
    {
        _baseClient = baseClient;
    }

    /// <inheritdoc />
    public async Task<HttpResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/public/time",
            CoinTRExchange.RateLimiter.Rest, 1, false);

        var result = await _baseClient
            .SendAsync<CoinTRServerTime>(request, null, ct)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<DateTime>(result);

        return HttpResult.Ok(result, result.Data.ServerTime);
    }

    /// <inheritdoc />
    public async Task<HttpResult<CoinTRSymbol[]>> GetSymbolsAsync(CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/spot/public/symbols",
            CoinTRExchange.RateLimiter.Rest, 1, false);

        return await _baseClient.SendAsync<CoinTRSymbol[]>(request, null, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<CoinTRTicker[]>> GetTickersAsync(
        string? symbol = null,
        CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/spot/market/tickers",
            CoinTRExchange.RateLimiter.Rest, 1, false);

        var parameters = new Parameters(CoinTRExchange.ParameterSettings);
        parameters.Add("symbol", symbol);

        return await _baseClient.SendAsync<CoinTRTicker[]>(request, parameters, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<CoinTROrderBook>> GetOrderBookAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/spot/market/orderbook",
            CoinTRExchange.RateLimiter.Rest, 1, false);

        var parameters = new Parameters(CoinTRExchange.ParameterSettings);
        parameters.Add("symbol", symbol);
        parameters.Add("limit", limit);

        return await _baseClient.SendAsync<CoinTROrderBook>(request, parameters, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<CoinTRTrade[]>> GetTradesAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/spot/market/fills",
            CoinTRExchange.RateLimiter.Rest, 1, false);

        var parameters = new Parameters(CoinTRExchange.ParameterSettings);
        parameters.Add("symbol", symbol);
        parameters.Add("limit", limit);

        return await _baseClient.SendAsync<CoinTRTrade[]>(request, parameters, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<CoinTRKline[]>> GetKlinesAsync(
        string symbol,
        KlineInterval interval,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? limit = null,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/spot/market/candles",
            CoinTRExchange.RateLimiter.Rest, 1, false);

        var parameters = new Parameters(CoinTRExchange.ParameterSettings);
        parameters.Add("symbol", symbol);
        parameters.Add("granularity", interval);
        parameters.Add("startTime", startTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("endTime", endTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("limit", limit);

        return await _baseClient.SendAsync<CoinTRKline[]>(request, parameters, ct).ConfigureAwait(false);
    }

    private static void ValidateSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));
    }
}
