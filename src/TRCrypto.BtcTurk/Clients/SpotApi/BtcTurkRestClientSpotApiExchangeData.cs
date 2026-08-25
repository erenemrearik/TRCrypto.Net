using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <inheritdoc />
internal class BtcTurkRestClientSpotApiExchangeData : IBtcTurkRestClientSpotApiExchangeData
{
    /// <summary>Borsanin islem ucunda kabul ettigi en yuksek kayit sayisi.</summary>
    private const int MaxTradeLimit = 50;

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
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/server/exchangeinfo",
            BtcTurkExchange.RateLimiter.PublicRest, 1, false);

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

    /// <inheritdoc />
    public async Task<HttpResult<IReadOnlyList<BtcTurkTicker>>> GetTickersAsync(CancellationToken ct = default)
    {
        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/ticker",
            BtcTurkExchange.RateLimiter.PublicRest, 1, false);

        return await _baseClient.SendAsync<IReadOnlyList<BtcTurkTicker>>(request, null, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BtcTurkTicker>> GetTickerAsync(string symbol, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));

        var parameters = new Parameters(BtcTurkExchange.ParameterSettings) { { "pairSymbol", symbol } };

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/ticker",
            BtcTurkExchange.RateLimiter.PublicRest, 1, false);

        var result = await _baseClient
            .SendAsync<IReadOnlyList<BtcTurkTicker>>(request, parameters, ct)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<BtcTurkTicker>(result);

        // Uc, tek parite icin bile dizi dondurur; bos dizi bilinmeyen sembol demektir.
        if (result.Data.Count == 0)
        {
            return HttpResult.Fail<BtcTurkTicker>(
                result,
                new ServerError(new ErrorInfo(
                    ErrorType.UnknownSymbol,
                    false,
                    $"{symbol} paritesi icin ozet fiyat bilgisi dondurulmedi.")));
        }

        return HttpResult.Ok(result, result.Data[0]);
    }

    /// <inheritdoc />
    public async Task<HttpResult<IReadOnlyList<BtcTurkTicker>>> GetTickersByQuoteAssetAsync(
        string quoteAsset,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(quoteAsset))
            throw new ArgumentException("Quote varlik bos olamaz.", nameof(quoteAsset));

        var parameters = new Parameters(BtcTurkExchange.ParameterSettings) { { "symbol", BtcTurkExchange.NormalizeAsset(quoteAsset) } };

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/ticker/currency",
            BtcTurkExchange.RateLimiter.PublicRest, 1, false);

        return await _baseClient
            .SendAsync<IReadOnlyList<BtcTurkTicker>>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<BtcTurkOrderBook>> GetOrderBookAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));

        if (limit is <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "Kademe sayisi pozitif olmalidir.");

        var parameters = new Parameters(BtcTurkExchange.ParameterSettings) { { "pairSymbol", symbol } };
        parameters.Add("limit", limit);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/orderbook",
            BtcTurkExchange.RateLimiter.PublicRest, 1, false);

        return await _baseClient.SendAsync<BtcTurkOrderBook>(request, parameters, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<IReadOnlyList<BtcTurkTrade>>> GetTradesAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));

        // Borsa sinirini asan istek reddedilir; aga cikmadan burada yakalanir.
        if (limit is <= 0 or > MaxTradeLimit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit), limit, $"Islem sayisi 1 ile {MaxTradeLimit} arasinda olmalidir.");
        }

        var parameters = new Parameters(BtcTurkExchange.ParameterSettings) { { "pairSymbol", symbol } };
        parameters.Add("last", limit);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.BaseAddress, "/api/v2/trades",
            BtcTurkExchange.RateLimiter.PublicRest, 1, false);

        return await _baseClient
            .SendAsync<IReadOnlyList<BtcTurkTrade>>(request, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResult<IReadOnlyList<BtcTurkKline>>> GetKlinesAsync(
        string symbol,
        KlineInterval interval,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));

        // Bu uc saniye cinsinden zaman damgasi bekler; diger uclar milisaniye kullanir.
        var from = startTime ?? DateTime.UtcNow.AddDays(-1);
        var to = endTime ?? DateTime.UtcNow;

        var parameters = new Parameters(BtcTurkExchange.ParameterSettings)
        {
            { "symbol", symbol },
            { "resolution", ToResolution(interval) }
        };
        parameters.Add("from", from, DateTimeSerialization.SecondsNumber);
        parameters.Add("to", to, DateTimeSerialization.SecondsNumber);

        var request = _definitions.GetOrCreate(
            HttpMethod.Get, _baseClient.GraphBaseAddress, "/v1/klines/history",
            BtcTurkExchange.RateLimiter.GraphRest, 1, false);

        // Standart zarf tasimadigi icin yanit dogrudan okunur.
        var result = await _baseClient
            .SendRawAsync<BtcTurkKlineResponse>(request, parameters, ct)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<IReadOnlyList<BtcTurkKline>>(result);

        return HttpResult.Ok<IReadOnlyList<BtcTurkKline>>(result, result.Data.ToKlines());
    }

    /// <summary>
    /// Mum araligini borsanin bekledigi <c>resolution</c> degerine cevirir.
    /// </summary>
    /// <remarks>
    /// Gunluk ve daha uzun araliklar borsa tarafindan harf koduyla ifade edilir;
    /// daha kisa araliklar dakika sayisidir.
    /// </remarks>
    private static string ToResolution(KlineInterval interval)
        => interval switch
        {
            KlineInterval.OneDay => "1D",
            KlineInterval.OneWeek => "1W",
            _ => ((int)interval / 60).ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
}
