using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <summary>
/// BtcTurk spot API'sinin borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// Sembol donusumu burada yapilir; cagiran taraf <see cref="SharedSymbol"/> kullanir ve
/// BtcTurk'un <c>BTCTRY</c> bicimini hic gormez.
/// </remarks>
internal partial class BtcTurkRestClientSpotApi : IBtcTurkRestClientSpotApiShared
{
    private const string _topicId = "BtcTurkSpot";

    /// <inheritdoc />
    public TradingMode[] SupportedTradingModes { get; } = [TradingMode.Spot];

    /// <inheritdoc />
    public void SetDefaultExchangeParameter(string key, object value)
        => ExchangeParameters.SetStaticParameter(Exchange, key, value);

    /// <inheritdoc />
    public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

    /// <inheritdoc />
    public SharedClientInfo Discover() => SharedUtils.GetClientInfo(BtcTurkExchange.Metadata, this);

    #region Spot Symbol client

    SharedSymbolCatalog? ISpotSymbolRestClient.SpotSymbolCatalog
        => ExchangeSymbolCache.GetSymbolCatalog(BtcTurkExchange.ExchangeName, _topicId, EnvironmentName, null);

    GetSpotSymbolsOptions ISpotSymbolRestClient.GetSpotSymbolsOptions { get; }
        = new(BtcTurkExchange.ExchangeName, false);

    async Task<HttpResult<SharedSpotSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsAsync(
        GetSymbolsRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotSymbolsOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotSymbol[]>(Exchange, validationError);

        var result = await ExchangeData.GetExchangeInfoAsync(ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotSymbol[]>(result);

        // Varlik turu borsanin bildirdigi currencyType alanindan alinir, tahmin edilmez.
        var assetTypes = result.Data.Currencies.ToDictionary(
            x => x.Symbol,
            x => x.CurrencyType == CurrencyType.Fiat ? SharedAssetType.Fiat : SharedAssetType.Crypto,
            StringComparer.OrdinalIgnoreCase);

        var symbols = result.Data.Symbols
            .Select(x => ParseSymbol(x, assetTypes))
            .ToArray();

        ExchangeSymbolCache.UpdateSymbolInfo(_topicId, EnvironmentName, null, symbols);

        return HttpResult.Ok(result, SharedUtils.ApplySymbolFilter(symbols, request));
    }

    private static SharedSpotSymbol ParseSymbol(
        Objects.Models.BtcTurkSymbol symbol,
        IReadOnlyDictionary<string, SharedAssetType> assetTypes)
    {
        // Base/quote ayri alanlar olarak gelir; sembol adi ayristirilmaz.
        var priceFilter = symbol.Filters.FirstOrDefault(x => x.FilterType == "PRICE_FILTER");

        return new SharedSpotSymbol(
            symbol.Numerator,
            symbol.Denominator,
            symbol.Name,
            symbol.Status == SymbolStatus.Trading)
        {
            QuantityDecimals = symbol.NumeratorScale,
            PriceDecimals = symbol.DenominatorScale,
            PriceStep = priceFilter?.TickSize,
            MinNotionalValue = priceFilter?.MinExchangeValue,
            BaseAssetType = Lookup(assetTypes, symbol.Numerator),
            QuoteAssetType = Lookup(assetTypes, symbol.Denominator)
        };
    }

    private static SharedAssetType Lookup(
        IReadOnlyDictionary<string, SharedAssetType> assetTypes,
        string asset)
        => assetTypes.TryGetValue(asset, out var type) ? type : SharedAssetType.Unspecified;

    async Task<ExchangeCallResult<SharedSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsForBaseAssetAsync(
        string baseAsset)
    {
        var error = await EnsureSymbolsCachedAsync().ConfigureAwait(false);
        if (error != null)
            return ExchangeCallResult<SharedSymbol[]>.Fail(Exchange, error);

        return ExchangeCallResult<SharedSymbol[]>.Ok(
            Exchange,
            ExchangeSymbolCache.GetSymbolsForBaseAsset(
                _topicId, EnvironmentName, null, BtcTurkExchange.NormalizeAsset(baseAsset)));
    }

    async Task<ExchangeCallResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(SharedSymbol symbol)
    {
        if (symbol.TradingMode != TradingMode.Spot)
            throw new ArgumentException("BtcTurk yalnizca spot islemleri destekler.", nameof(symbol));

        var error = await EnsureSymbolsCachedAsync().ConfigureAwait(false);
        if (error != null)
            return ExchangeCallResult<bool>.Fail(Exchange, error);

        return ExchangeCallResult<bool>.Ok(
            Exchange,
            ExchangeSymbolCache.SupportsSymbol(_topicId, EnvironmentName, null, symbol));
    }

    async Task<ExchangeCallResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(string symbolName)
    {
        var error = await EnsureSymbolsCachedAsync().ConfigureAwait(false);
        if (error != null)
            return ExchangeCallResult<bool>.Fail(Exchange, error);

        return ExchangeCallResult<bool>.Ok(
            Exchange,
            ExchangeSymbolCache.SupportsSymbol(_topicId, EnvironmentName, null, symbolName));
    }

    /// <summary>
    /// Sembol onbellegi bos ise doldurur.
    /// </summary>
    /// <returns>Doldurma basarisiz olduysa hata, aksi halde <c>null</c>.</returns>
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

    #region Balance client

    GetBalancesOptions IBalanceRestClient.GetBalancesOptions { get; }
        = new(BtcTurkExchange.ExchangeName, AccountTypeFilter.Spot);

    async Task<HttpResult<SharedBalance[]>> IBalanceRestClient.GetBalancesAsync(
        GetBalancesRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetBalancesOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedBalance[]>(Exchange, validationError);

        var result = await Account.GetBalancesAsync(ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedBalance[]>(result);

        return HttpResult.Ok(result, result.Data
            .Select(x => new SharedBalance(TradingMode.Spot, x.Asset, x.Available, x.Total))
            .ToArray());
    }

    #endregion

    #region Spot Ticker client

    GetSpotTickerOptions ISpotTickerRestClient.GetSpotTickerOptions { get; }
        = new(BtcTurkExchange.ExchangeName, SharedTickerType.Day24H);

    async Task<HttpResult<SharedSpotTicker>> ISpotTickerRestClient.GetSpotTickerAsync(
        GetTickerRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotTickerOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotTicker>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);
        var result = await ExchangeData.GetTickerAsync(symbol, ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotTicker>(result);

        return HttpResult.Ok(result, ParseTicker(result.Data, request.Symbol));
    }

    GetSpotTickersOptions ISpotTickerRestClient.GetSpotTickersOptions { get; }
        = new(BtcTurkExchange.ExchangeName, SharedTickerType.Day24H);

    async Task<HttpResult<SharedSpotTicker[]>> ISpotTickerRestClient.GetSpotTickersAsync(
        GetTickersRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotTickersOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotTicker[]>(Exchange, validationError);

        var result = await ExchangeData.GetTickersAsync(ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotTicker[]>(result);

        return HttpResult.Ok(result, result.Data.Select(x => ParseTicker(x, null)).ToArray());
    }

    private SharedSpotTicker ParseTicker(Objects.Models.BtcTurkTicker ticker, SharedSymbol? requested)
    {
        var symbol = requested
            ?? ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, ticker.Pair)
            ?? new SharedSymbol(TradingMode.Spot, ticker.NumeratorSymbol, ticker.DenominatorSymbol);

        return new SharedSpotTicker(
            symbol,
            ticker.Pair,
            ticker.LastPrice,
            ticker.HighPrice,
            ticker.LowPrice,
            // Hacim base varlik cinsindendir; quote karsiligi ortalama fiyattan turetilir.
            new SharedOrderQuantity(ticker.Volume, ticker.Volume * ticker.AveragePrice),
            ticker.DailyChangePercentage);
    }

    #endregion

    #region Order Book client

    // Borsa varsayilani 25 kademedir; ust sinir dokumante edilmemistir.
    GetOrderBookOptions IOrderBookRestClient.GetOrderBookOptions { get; }
        = new(BtcTurkExchange.ExchangeName, 1, 1000, false);

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

        return HttpResult.Ok(
            result,
            new SharedOrderBook(
                SharedQuantityType.BaseAsset,
                result.Data.Asks.Cast<ISymbolOrderBookEntry>().ToArray(),
                result.Data.Bids.Cast<ISymbolOrderBookEntry>().ToArray()));
    }

    #endregion

    #region Recent Trade client

    // Borsa bu ucta en fazla 50 kayit dondurur.
    GetRecentTradesOptions IRecentTradeRestClient.GetRecentTradesOptions { get; }
        = new(BtcTurkExchange.ExchangeName, 50, false);

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

        return HttpResult.Ok(result, result.Data.Select(x =>
            new SharedTrade(request.Symbol, symbol, new SharedOrderQuantity(x.Quantity), x.Price, x.Timestamp)
            {
                Side = x.Side == OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell
            }).ToArray());
    }

    #endregion
}
