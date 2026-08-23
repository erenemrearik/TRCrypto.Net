using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <inheritdoc />
internal class BtcTurkRestClientSpotApiExchangeData : IBtcTurkRestClientSpotApiExchangeData
{
    private static readonly RequestDefinitionCache _definitions = new();
    private readonly BtcTurkRestClientSpotApi _baseClient;

    internal BtcTurkRestClientSpotApiExchangeData(BtcTurkRestClientSpotApi baseClient)
    {
        _baseClient = baseClient;
    }

    /// <inheritdoc />
    public async Task<HttpResult<BtcTurkExchangeInfo>> GetExchangeInfoAsync(CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/server/exchangeinfo", BtcTurkExchange.RateLimiter.PublicRest, 1, false);

        return await _baseClient.SendAsync<BtcTurkExchangeInfo>(request, null, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default)
    {
        var result = await GetExchangeInfoAsync(ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<DateTime>(result);

        return HttpResult.Ok(result, result.Data.ServerTime);
    }
}
