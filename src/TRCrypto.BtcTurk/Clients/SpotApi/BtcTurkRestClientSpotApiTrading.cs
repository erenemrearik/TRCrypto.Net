using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Enums;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <inheritdoc />
internal class BtcTurkRestClientSpotApiTrading : IBtcTurkRestClientSpotApiTrading
{
    /// <summary>Emir gecmisi ucunun kabul ettigi en yuksek kayit sayisi.</summary>
    private const int MaxOrderLimit = 1000;

    private static readonly RequestDefinitionCache _definitions = new();
    private readonly BtcTurkRestClientSpotApi _baseClient;

    internal BtcTurkRestClientSpotApiTrading(BtcTurkRestClientSpotApi baseClient)
    {
        _baseClient = baseClient;
    }

    /// <inheritdoc />
    public async Task<HttpResult<BtcTurkOpenOrders>> GetOpenOrdersAsync(
        string? symbol = null,
        CancellationToken ct = default)
    {
        var parameters = new Parameters(BtcTurkExchange.ParameterSettings);
        parameters.Add("pairSymbol", symbol);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v1/openOrders",
            BtcTurkExchange.RateLimiter.PrivateRest, 1, true);

        return await _baseClient
            .SendAsync<BtcTurkOpenOrders>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<IReadOnlyList<BtcTurkOrder>>> GetOrdersAsync(
        string? symbol = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        long? fromOrderId = null,
        int? page = null,
        int? limit = null,
        CancellationToken ct = default)
    {
        if (limit is <= 0 or > MaxOrderLimit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit), limit, $"Kayit sayisi 1 ile {MaxOrderLimit} arasinda olmalidir.");
        }

        var parameters = new Parameters(BtcTurkExchange.ParameterSettings);
        parameters.Add("pairSymbol", symbol);
        parameters.Add("startTime", startTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("endTime", endTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("orderId", fromOrderId);
        parameters.Add("page", page);
        parameters.Add("limit", limit);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v1/allOrders",
            BtcTurkExchange.RateLimiter.PrivateRest, 1, true);

        return await _baseClient
            .SendAsync<IReadOnlyList<BtcTurkOrder>>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BtcTurkOrder>> GetOrderAsync(long orderId, CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, $"/api/v1/order/{orderId}",
            BtcTurkExchange.RateLimiter.PrivateRest, 1, true);

        return await _baseClient.SendAsync<BtcTurkOrder>(request, null, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BtcTurkOrderPlacement>> PlaceOrderAsync(
        string symbol,
        OrderSide side,
        OrderMethod method,
        decimal quantity,
        decimal? price = null,
        decimal? stopPrice = null,
        string? clientOrderId = null,
        CancellationToken ct = default)
    {
        ValidateOrder(symbol, method, quantity, price, stopPrice);

        var parameters = new Parameters(BtcTurkExchange.ParameterSettings)
        {
            { "pairSymbol", symbol },
            { "orderType", side },
            { "orderMethod", method },
            { "quantity", quantity }
        };
        parameters.Add("price", price);
        parameters.Add("stopPrice", stopPrice);
        parameters.Add("newOrderClientId", clientOrderId);

        var request = _definitions.GetOrCreate(
            HttpMethod.Post, _baseClient.BaseAddress, "/api/v1/order",
            BtcTurkExchange.RateLimiter.PrivateRest, 1, true);

        return await _baseClient
            .SendAsync<BtcTurkOrderPlacement>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult> CancelOrderAsync(long orderId, CancellationToken ct = default)
    {
        var parameters = new Parameters(BtcTurkExchange.ParameterSettings) { { "id", orderId } };

        var request = _definitions.GetOrCreate(
            HttpMethod.Delete, _baseClient.BaseAddress, "/api/v1/order",
            BtcTurkExchange.RateLimiter.PrivateRest, 1, true);

        return await _baseClient.SendAsync(request, parameters, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Emri borsaya gondermeden once dogrular.
    /// </summary>
    /// <remarks>
    /// Bu kontroller aga cikilmadan yapilir. Amac yalnizca gereksiz istegi onlemek degil,
    /// eksik bir parametrenin borsada beklenmedik bir emre donusmesini engellemektir.
    /// </remarks>
    private static void ValidateOrder(
        string symbol,
        OrderMethod method,
        decimal quantity,
        decimal? price,
        decimal? stopPrice)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Miktar pozitif olmalidir.");

        if (price is <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Fiyat pozitif olmalidir.");

        if (stopPrice is <= 0)
            throw new ArgumentOutOfRangeException(nameof(stopPrice), stopPrice, "Stop fiyati pozitif olmalidir.");

        var needsPrice = method is OrderMethod.Limit or OrderMethod.StopLimit;
        if (needsPrice && price == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price), price, $"{method} emri icin fiyat belirtilmelidir.");
        }

        var needsStopPrice = method is OrderMethod.StopLimit or OrderMethod.StopMarket;
        if (needsStopPrice && stopPrice == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stopPrice), stopPrice, $"{method} emri icin stop fiyati belirtilmelidir.");
        }
    }
}
