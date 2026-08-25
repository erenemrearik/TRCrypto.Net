using CryptoExchange.Net.Objects;
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
}
