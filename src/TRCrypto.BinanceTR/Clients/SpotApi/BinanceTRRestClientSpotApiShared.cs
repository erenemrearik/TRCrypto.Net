using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <summary>
/// Binance TR spot REST API'sinin borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// Ticker arayuzu bilincli olarak uygulanmamistir: borsa ticker verisini anahtarsiz
/// sunmaz ve REST tarafinda karsiligi yoktur. Gercek zamanli ticker icin socket
/// yuzeyi kullanilmalidir.
/// </remarks>
internal partial class BinanceTRRestClientSpotApi : IBinanceTRRestClientSpotApiShared
{
    private const string _topicId = "BinanceTRSpot";

    /// <inheritdoc />
    public TradingMode[] SupportedTradingModes { get; } = [TradingMode.Spot];

    /// <inheritdoc />
    public void SetDefaultExchangeParameter(string key, object value)
        => ExchangeParameters.SetStaticParameter(Exchange, key, value);

    /// <inheritdoc />
    public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

    /// <inheritdoc />
    public SharedClientInfo Discover() => SharedUtils.GetClientInfo(BinanceTRExchange.Metadata, this);

    #region Spot Symbol client

    SharedSymbolCatalog? ISpotSymbolRestClient.SpotSymbolCatalog
        => ExchangeSymbolCache.GetSymbolCatalog(BinanceTRExchange.ExchangeName, _topicId, EnvironmentName, null);

    GetSpotSymbolsOptions ISpotSymbolRestClient.GetSpotSymbolsOptions { get; }
        = new(BinanceTRExchange.ExchangeName, false);

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

        var symbols = result.Data.Symbols.Select(ParseSymbol).ToArray();
        ExchangeSymbolCache.UpdateSymbolInfo(_topicId, EnvironmentName, null, symbols);

        return HttpResult.Ok(result, SharedUtils.ApplySymbolFilter(symbols, request));
    }

    private static SharedSpotSymbol ParseSymbol(Objects.Models.BinanceTRSymbol symbol)
    {
        var priceFilter = symbol.Filters.FirstOrDefault(x => x.FilterType == "PRICE_FILTER");
        var lotSize = symbol.Filters.FirstOrDefault(x => x.FilterType == "LOT_SIZE");

        // Base/quote ayri alanlardan gelir; sembol adi ayristirilmaz.
        return new SharedSpotSymbol(symbol.BaseAsset, symbol.QuoteAsset, symbol.Name, true)
        {
            PriceDecimals = symbol.QuotePrecision,
            QuantityDecimals = symbol.BasePrecision,
            PriceStep = priceFilter?.TickSize,
            QuantityStep = lotSize?.StepSize,
            MinTradeQuantity = lotSize?.MinQuantity
        };
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
                _topicId, EnvironmentName, null, BinanceTRExchange.NormalizeAsset(baseAsset)));
    }

    async Task<ExchangeCallResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(SharedSymbol symbol)
    {
        if (symbol.TradingMode != TradingMode.Spot)
            throw new ArgumentException("Binance TR yalnizca spot islemleri destekler.", nameof(symbol));

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

    #region Order Book client

    // Borsa yalnizca belirli kademe sayilarini kabul eder; en dusugu 5, en yuksegi 1000.
    GetOrderBookOptions IOrderBookRestClient.GetOrderBookOptions { get; }
        = new(BinanceTRExchange.ExchangeName, 5, 1000, false);

    async Task<HttpResult<SharedOrderBook>> IOrderBookRestClient.GetOrderBookAsync(
        GetOrderBookRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetOrderBookOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedOrderBook>(Exchange, validationError);

        // Borsa yalnizca sabit kademe degerlerini kabul ettigi icin istenen deger
        // desteklenen en yakin ust degere yuvarlanir.
        var limit = request.Limit == null ? null : (int?)RoundUpToSupportedLimit(request.Limit.Value);

        var result = await ExchangeData
            .GetOrderBookAsync(request.Symbol!.GetSymbol(FormatSymbol), limit, ct)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedOrderBook>(result);

        return HttpResult.Ok(result, new SharedOrderBook(
            SharedQuantityType.BaseAsset,
            result.Data.Asks.Cast<ISymbolOrderBookEntry>().ToArray(),
            result.Data.Bids.Cast<ISymbolOrderBookEntry>().ToArray()));
    }

    /// <summary>Istenen kademe sayisini borsanin kabul ettigi en yakin ust degere yuvarlar.</summary>
    private static int RoundUpToSupportedLimit(int requested)
    {
        foreach (var supported in new[] { 5, 10, 20, 50, 100, 500, 1000 })
        {
            if (requested <= supported)
                return supported;
        }

        return 1000;
    }

    #endregion

    #region Recent Trade client

    // Borsanin ayrintili islem ucu bos donduğu icin toplulastirilmis islemler kullanilir.
    GetRecentTradesOptions IRecentTradeRestClient.GetRecentTradesOptions { get; }
        = new(BinanceTRExchange.ExchangeName, 1000, false);

    async Task<HttpResult<SharedTrade[]>> IRecentTradeRestClient.GetRecentTradesAsync(
        GetRecentTradesRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetRecentTradesOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedTrade[]>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);
        var result = await ExchangeData
            .GetAggregatedTradesAsync(symbol, request.Limit, ct)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedTrade[]>(result);

        return HttpResult.Ok(result, result.Data.Trades.Select(x =>
            new SharedTrade(
                request.Symbol,
                symbol,
                new SharedOrderQuantity(x.Quantity),
                x.Price,
                x.Timestamp)
            {
                Side = x.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell
            }).ToArray());
    }

    #endregion
}
