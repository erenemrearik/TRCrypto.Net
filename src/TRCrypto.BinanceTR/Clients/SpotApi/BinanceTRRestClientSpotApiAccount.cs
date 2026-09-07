using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;
using TRCrypto.BinanceTR.Objects.Models;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <inheritdoc />
internal class BinanceTRRestClientSpotApiAccount : IBinanceTRRestClientSpotApiAccount
{
    private static readonly RequestDefinitionCache _definitions = new();
    private readonly BinanceTRRestClientSpotApi _baseClient;

    internal BinanceTRRestClientSpotApiAccount(BinanceTRRestClientSpotApi baseClient)
    {
        _baseClient = baseClient;
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTRAccount>> GetAccountAsync(
        long? receiveWindow = null,
        CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/account/spot",
            BinanceTRExchange.RateLimiter.Rest, 1, true);

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings);
        parameters.Add("recvWindow", receiveWindow);

        return await _baseClient.SendAsync<BinanceTRAccount>(request, parameters, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTRAccountAsset>> GetAssetAsync(
        string asset,
        long? receiveWindow = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(asset))
            throw new ArgumentException("Varlik adi bos olamaz.", nameof(asset));

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/open/v1/account/spot/asset",
            BinanceTRExchange.RateLimiter.Rest, 1, true);

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings);
        parameters.Add("asset", BinanceTRExchange.NormalizeAsset(asset));
        parameters.Add("recvWindow", receiveWindow);

        return await _baseClient
            .SendAsync<BinanceTRAccountAsset>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BinanceTRListenToken>> GetListenTokenAsync(
        long? validity = null,
        long? receiveWindow = null,
        CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Post, _baseClient.BaseAddress, "/open/v1/user-listen-token",
            BinanceTRExchange.RateLimiter.Rest, 1, true);

        var parameters = new Parameters(BinanceTRExchange.ParameterSettings);
        parameters.Add("validity", validity);
        parameters.Add("recvWindow", receiveWindow);

        return await _baseClient
            .SendAsync<BinanceTRListenToken>(request, parameters, ct)
            .ConfigureAwait(false);
    }
}
