using CryptoExchange.Net.SharedApis;
using TRCrypto.CoinTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.CoinTR.Clients.SpotApi;

/// <summary>
/// CoinTR spot REST API'sinin borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// Cagiran taraf <see cref="SharedSymbol"/> kullanir; native sembol bicimi burada uretilir.
/// </remarks>
internal partial class CoinTRRestClientSpotApi : ICoinTRRestClientSpotApiShared
{
    private const string _topicId = "CoinTRSpot";

    /// <inheritdoc />
    public TradingMode[] SupportedTradingModes { get; } = [TradingMode.Spot];

    /// <inheritdoc />
    public void SetDefaultExchangeParameter(string key, object value)
        => ExchangeParameters.SetStaticParameter(Exchange, key, value);

    /// <inheritdoc />
    public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

    /// <inheritdoc />
    public SharedClientInfo Discover() => SharedUtils.GetClientInfo(CoinTRExchange.Metadata, this);

    #region Spot Symbol client

    SharedSymbolCatalog? ISpotSymbolRestClient.SpotSymbolCatalog
        => ExchangeSymbolCache.GetSymbolCatalog(CoinTRExchange.ExchangeName, _topicId, EnvironmentName, null);

    GetSpotSymbolsOptions ISpotSymbolRestClient.GetSpotSymbolsOptions { get; }
        = new(CoinTRExchange.ExchangeName, false);

    async Task<HttpResult<SharedSpotSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsAsync(
        GetSymbolsRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotSymbolsOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotSymbol[]>(Exchange, validationError);

        var result = await ExchangeData.GetSymbolsAsync(ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotSymbol[]>(result);

        // Base ve quote ayri alanlardan okunur; sembol adi ayristirilmaz.
        var symbols = result.Data
            .Select(x => new SharedSpotSymbol(x.BaseAsset, x.QuoteAsset, x.Name, x.IsTrading)
            {
                PriceDecimals = x.PriceDecimals,
                QuantityDecimals = x.QuantityDecimals,
                MinTradeQuantity = x.MinTradeQuantity == 0 ? null : x.MinTradeQuantity,
                MaxTradeQuantity = x.MaxTradeQuantity == 0 ? null : x.MaxTradeQuantity
            })
            .ToArray();

        ExchangeSymbolCache.UpdateSymbolInfo(_topicId, EnvironmentName, null, symbols);

        return HttpResult.Ok(result, SharedUtils.ApplySymbolFilter(symbols, request));
    }

    async Task<ExchangeCallResult<SharedSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsForBaseAssetAsync(
        string baseAsset)
    {
        var error = await EnsureSymbolsCachedAsync().ConfigureAwait(false);
        if (error != null)
            return ExchangeCallResult<SharedSymbol[]>.Fail(Exchange, error);

        return ExchangeCallResult<SharedSymbol[]>.Ok(
            Exchange,
            ExchangeSymbolCache.GetSymbolsForBaseAsset(
                _topicId, EnvironmentName, null, CoinTRExchange.NormalizeAsset(baseAsset)));
    }

    async Task<ExchangeCallResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(SharedSymbol symbol)
    {
        if (symbol.TradingMode != TradingMode.Spot)
            throw new ArgumentException("CoinTR yalnizca spot islemleri destekler.", nameof(symbol));

        var error = await EnsureSymbolsCachedAsync().ConfigureAwait(false);
        if (error != null)
            return ExchangeCallResult<bool>.Fail(Exchange, error);

        return ExchangeCallResult<bool>.Ok(
            Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, EnvironmentName, null, symbol));
    }

    async Task<ExchangeCallResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(string symbolName)
    {
        var error = await EnsureSymbolsCachedAsync().ConfigureAwait(false);
        if (error != null)
            return ExchangeCallResult<bool>.Fail(Exchange, error);

        return ExchangeCallResult<bool>.Ok(
            Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, EnvironmentName, null, symbolName));
    }

    private async Task<Error?> EnsureSymbolsCachedAsync()
    {
        if (ExchangeSymbolCache.HasCached(_topicId, EnvironmentName, null))
            return null;

        var result = await ((ISpotSymbolRestClient)this)
            .GetSpotSymbolsAsync(new GetSymbolsRequest())
            .ConfigureAwait(false);

        return result.Success ? null : result.Error!;
    }

    #endregion

    #region Spot Ticker client

    GetSpotTickerOptions ISpotTickerRestClient.GetSpotTickerOptions { get; }
        = new(CoinTRExchange.ExchangeName, SharedTickerType.Day24H);

    async Task<HttpResult<SharedSpotTicker>> ISpotTickerRestClient.GetSpotTickerAsync(
        GetTickerRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotTickerOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotTicker>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);
        var result = await ExchangeData.GetTickersAsync(symbol, ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotTicker>(result);

        // Uc, tek parite istendiginde bile dizi dondurur.
        var ticker = result.Data.FirstOrDefault();
        if (ticker == null)
        {
            return HttpResult.Fail<SharedSpotTicker>(
                Exchange, new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, false, "Parite bulunamadi")));
        }

        return HttpResult.Ok(result, ToSharedTicker(ticker, request.Symbol));
    }

    GetSpotTickersOptions ISpotTickerRestClient.GetSpotTickersOptions { get; }
        = new(CoinTRExchange.ExchangeName, SharedTickerType.Day24H);

    async Task<HttpResult<SharedSpotTicker[]>> ISpotTickerRestClient.GetSpotTickersAsync(
        GetTickersRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotTickersOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotTicker[]>(Exchange, validationError);

        var result = await ExchangeData.GetTickersAsync(ct: ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotTicker[]>(result);

        return HttpResult.Ok(result, result.Data.Select(x => ToSharedTicker(x, null)).ToArray());
    }

    /// <summary>Native ticker'i borsadan bagimsiz karsiligina cevirir.</summary>
    /// <remarks>
    /// Degisim orani borsada kesir olarak gelir; shared model yuzde bekler. Donusum
    /// atlanirsa deger yuz kat kucuk gorunur.
    /// </remarks>
    private SharedSpotTicker ToSharedTicker(Objects.Models.CoinTRTicker ticker, SharedSymbol? requested)
    {
        var symbol = requested
            ?? ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, ticker.Symbol)
            ?? new SharedSymbol(TradingMode.Spot, ticker.Symbol, string.Empty);

        return new SharedSpotTicker(
            symbol,
            ticker.Symbol,
            ticker.LastPrice,
            ticker.HighPrice,
            ticker.LowPrice,
            new SharedOrderQuantity(ticker.Volume, ticker.QuoteVolume),
            ticker.ChangePercentage);
    }

    #endregion

    #region Order Book client

    GetOrderBookOptions IOrderBookRestClient.GetOrderBookOptions { get; }
        = new(CoinTRExchange.ExchangeName, 1, 150, false);

    async Task<HttpResult<SharedOrderBook>> IOrderBookRestClient.GetOrderBookAsync(
        GetOrderBookRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetOrderBookOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedOrderBook>(Exchange, validationError);

        var result = await ExchangeData
            .GetOrderBookAsync(request.Symbol!.GetSymbol(FormatSymbol), request.Limit, ct)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedOrderBook>(result);

        return HttpResult.Ok(result, new SharedOrderBook(
            SharedQuantityType.BaseAsset,
            result.Data.Asks.Cast<ISymbolOrderBookEntry>().ToArray(),
            result.Data.Bids.Cast<ISymbolOrderBookEntry>().ToArray()));
    }

    #endregion

    #region Recent Trade client

    GetRecentTradesOptions IRecentTradeRestClient.GetRecentTradesOptions { get; }
        = new(CoinTRExchange.ExchangeName, 100, false);

    async Task<HttpResult<SharedTrade[]>> IRecentTradeRestClient.GetRecentTradesAsync(
        GetRecentTradesRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetRecentTradesOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedTrade[]>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);
        var result = await ExchangeData.GetTradesAsync(symbol, request.Limit, ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedTrade[]>(result);

        return HttpResult.Ok(result, result.Data
            .Select(x => new SharedTrade(
                request.Symbol,
                symbol,
                new SharedOrderQuantity(x.Quantity),
                x.Price,
                x.Timestamp)
            {
                Side = x.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell
            })
            .ToArray());
    }

    #endregion

    #region Kline client

    GetKlinesOptions IKlineRestClient.GetKlinesOptions { get; } = new(
        CoinTRExchange.ExchangeName,
        supportsAscending: true,
        supportsDescending: false,
        timeFilterSupported: true,
        maxLimit: 1000,
        needsAuthentication: false,
        SharedKlineInterval.OneMinute,
        SharedKlineInterval.FiveMinutes,
        SharedKlineInterval.FifteenMinutes,
        SharedKlineInterval.ThirtyMinutes,
        SharedKlineInterval.OneHour,
        SharedKlineInterval.FourHours,
        SharedKlineInterval.OneDay,
        SharedKlineInterval.OneWeek);

    async Task<HttpResult<SharedKline[]>> IKlineRestClient.GetKlinesAsync(
        GetKlinesRequest request,
        PageRequest? pageRequest,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetKlinesOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedKline[]>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);
        var result = await ExchangeData.GetKlinesAsync(
            symbol,
            (Enums.KlineInterval)request.Interval,
            request.StartTime,
            request.EndTime,
            request.Limit,
            ct).ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedKline[]>(result);

        return HttpResult.Ok(result, result.Data
            .Select(x => new SharedKline(
                request.Symbol,
                symbol,
                x.OpenTime,
                x.ClosePrice,
                x.HighPrice,
                x.LowPrice,
                x.OpenPrice,
                new SharedOrderQuantity(x.Volume, x.QuoteVolume)))
            .ToArray());
    }

    #endregion
}
