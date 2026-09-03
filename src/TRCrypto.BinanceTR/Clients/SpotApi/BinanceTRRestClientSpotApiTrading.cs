using TRCrypto.BinanceTR.Enums;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;
using TRCrypto.BinanceTR.Objects.Models;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <inheritdoc />
internal class BinanceTRRestClientSpotApiTrading : IBinanceTRRestClientSpotApiTrading
{
    private static readonly RequestDefinitionCache _definitions = new();
    private readonly BinanceTRRestClientSpotApi _baseClient;

    internal BinanceTRRestClientSpotApiTrading(BinanceTRRestClientSpotApi baseClient)
    {
        _baseClient = baseClient;
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTRPlacedOrder>> PlaceOrderAsync(
        string symbol,
        OrderSide side,
        OrderType type,
        decimal? quantity = null,
        decimal? quoteQuantity = null,
        decimal? price = null,
        string? clientOrderId = null,
        decimal? stopPrice = null,
        decimal? icebergQuantity = null,
        TimeInForce? timeInForce = null,
        long? receiveWindow = null,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var request = _definitions.GetOrCreate(
            HttpMethod.Post, _baseClient.BaseAddress, "/open/v1/orders",
            BinanceTRExchange.RateLimiter.Rest, 1, true);

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings);
        parameters.Add("symbol", symbol);
        parameters.Add("side", side);
        parameters.Add("type", type);
        parameters.Add("quantity", quantity);
        parameters.Add("quoteOrderQty", quoteQuantity);
        parameters.Add("price", price);
        parameters.Add("clientId", clientOrderId);
        parameters.Add("stopPrice", stopPrice);
        parameters.Add("icebergQty", icebergQuantity);
        parameters.Add("timeInForce", timeInForce);
        parameters.Add("recvWindow", receiveWindow);

        return await _baseClient
            .SendAsync<BinanceTRPlacedOrder>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTROrder>> GetOrderAsync(
        long? orderId = null,
        string? clientOrderId = null,
        long? receiveWindow = null,
        CancellationToken ct = default)
    {
        ValidateOrderIdentifier(orderId, clientOrderId);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/orders/detail",
            BinanceTRExchange.RateLimiter.Rest, 1, true);

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings);
        parameters.Add("orderId", orderId);
        parameters.Add("clientId", clientOrderId);
        parameters.Add("recvWindow", receiveWindow);

        return await _baseClient.SendAsync<BinanceTROrder>(request, parameters, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTROrderList>> GetOrdersAsync(
        string symbol,
        OrderSide? side = null,
        OrderType? type = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        long? fromId = null,
        int? limit = null,
        long? receiveWindow = null,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/orders",
            BinanceTRExchange.RateLimiter.Rest, 1, true);

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings);
        parameters.Add("symbol", symbol);
        parameters.Add("side", side);
        parameters.Add("type", type);
        parameters.Add("startTime", startTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("endTime", endTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("fromId", fromId);
        parameters.Add("limit", limit);
        parameters.Add("recvWindow", receiveWindow);

        return await _baseClient.SendAsync<BinanceTROrderList>(request, parameters, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTROrder>> CancelOrderAsync(
        long? orderId = null,
        string? clientOrderId = null,
        long? receiveWindow = null,
        CancellationToken ct = default)
    {
        ValidateOrderIdentifier(orderId, clientOrderId);

        var request = _definitions.GetOrCreate(
            HttpMethod.Post, _baseClient.BaseAddress, "/open/v1/orders/cancel",
            BinanceTRExchange.RateLimiter.Rest, 1, true);

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings);
        parameters.Add("orderId", orderId);
        parameters.Add("clientId", clientOrderId);
        parameters.Add("recvWindow", receiveWindow);

        return await _baseClient.SendAsync<BinanceTROrder>(request, parameters, ct).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public async Task<HttpResult<BinanceTRUserTradeList>> GetUserTradesAsync(
        string symbol,
        long? orderId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        long? fromId = null,
        int? limit = null,
        long? receiveWindow = null,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/orders/trades",
            BinanceTRExchange.RateLimiter.Rest, 1, true);

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings);
        parameters.Add("symbol", symbol);
        parameters.Add("orderId", orderId);
        parameters.Add("startTime", startTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("endTime", endTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("fromId", fromId);
        parameters.Add("limit", limit);
        parameters.Add("recvWindow", receiveWindow);

        return await _baseClient
            .SendAsync<BinanceTRUserTradeList>(request, parameters, ct)
            .ConfigureAwait(false);
    }
    private static void ValidateSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));
    }

    /// <summary>
    /// Emri belirleyen iki kimlikten en az birinin verildigini dogrular.
    /// </summary>
    /// <remarks>
    /// Ikisi de verilmezse borsa hangi emrin kastedildigini bilemez ve istek aga
    /// cikmadan reddedilir.
    /// </remarks>
    private static void ValidateOrderIdentifier(long? orderId, string? clientOrderId)
    {
        if (orderId == null && string.IsNullOrWhiteSpace(clientOrderId))
        {
            throw new ArgumentException(
                "Emir kimligi ya da istemci emir kimliginden biri verilmelidir.",
                nameof(orderId));
        }
    }
}
