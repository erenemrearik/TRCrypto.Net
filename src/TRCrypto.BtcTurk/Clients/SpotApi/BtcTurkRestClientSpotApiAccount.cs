using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Enums;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <inheritdoc />
internal class BtcTurkRestClientSpotApiAccount : IBtcTurkRestClientSpotApiAccount
{
    private static readonly RequestDefinitionCache _definitions = new();
    private readonly BtcTurkRestClientSpotApi _baseClient;

    internal BtcTurkRestClientSpotApiAccount(BtcTurkRestClientSpotApi baseClient)
    {
        _baseClient = baseClient;
    }

    /// <inheritdoc />
    public async Task<HttpResult<IReadOnlyList<BtcTurkBalance>>> GetBalancesAsync(CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v1/users/balances",
            BtcTurkExchange.RateLimiter.PrivateRest, 1, true);

        return await _baseClient
            .SendAsync<IReadOnlyList<BtcTurkBalance>>(request, null, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<IReadOnlyList<BtcTurkUserTrade>>> GetUserTradesAsync(
        string? symbol = null,
        OrderSide? side = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        long? orderId = null,
        CancellationToken ct = default)
    {
        // Borsa, emir kimligiyle filtrelemeyi diger filtrelerle birlestirmeyi desteklemiyor.
        // Sessizce yok saymak yerine cagiran taraf uyarilir.
        if (orderId != null && (symbol != null || side != null || startTime != null || endTime != null))
        {
            throw new ArgumentException(
                "Emir kimligi filtresi diger filtrelerle birlikte kullanilamaz.", nameof(orderId));
        }

        var parameters = new Parameters(BtcTurkExchange.ParameterSettings);
        parameters.Add("pairSymbol", symbol);
        parameters.Add("orderId", orderId);
        parameters.Add("startDate", startTime, DateTimeSerialization.MillisecondsNumber);
        parameters.Add("endDate", endTime, DateTimeSerialization.MillisecondsNumber);
        if (side != null)
            parameters.Add("type", side == OrderSide.Buy ? "buy" : "sell");

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v1/users/transactions/trade",
            BtcTurkExchange.RateLimiter.PrivateRest, 1, true);

        return await _baseClient
            .SendAsync<IReadOnlyList<BtcTurkUserTrade>>(request, parameters, ct)
            .ConfigureAwait(false);
    }
}
