using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <summary>
/// CryptoExchange.Net 13 ile gelen ince taneli (V2) paylasilan API arayuzleri.
/// </summary>
/// <remarks>
/// <para>
/// Her V2 uyesi ayni sinifin V1 uygulamasina delege eder; davranis tek yerde durur.
/// Secenek ozellikleri V1 ile ayni nesneyi dondurur, bu yuzden dogrulama kurallari
/// ve yetenek listesi tek bir yapilandirmayi paylasir.
/// </para>
/// <para>
/// V2 arayuzleri istege bagli degildir: 13 surumunde istek dogrulamasi
/// (<c>ValidateRequest</c>) yalnizca V2 yetenek arayuzunu kabul eder. Bunlar
/// uygulanmadan borsaya ozgu sinirlar (kademe sayisi, aralik gibi) denetlenemezdi.
/// </para>
/// </remarks>
internal partial class BtcTurkRestClientSpotApi
{
    #region IGetSpotSymbolsRest

    SharedSymbolCatalog? IGetSpotSymbols.SpotSymbolCatalog => ((ISpotSymbolRestClient)this).SpotSymbolCatalog;
    GetSpotSymbolsOptions IGetSpotSymbols.GetSpotSymbolsOptions => ((ISpotSymbolRestClient)this).GetSpotSymbolsOptions;

    Task<ExchangeCallResult<SharedSymbol[]>> IGetSpotSymbols.GetSpotSymbolsForBaseAssetAsync(string baseAsset)
        => ((ISpotSymbolRestClient)this).GetSpotSymbolsForBaseAssetAsync(baseAsset);

    Task<ExchangeCallResult<bool>> IGetSpotSymbols.SupportsSpotSymbolAsync(SharedSymbol symbol)
        => ((ISpotSymbolRestClient)this).SupportsSpotSymbolAsync(symbol);

    Task<ExchangeCallResult<bool>> IGetSpotSymbols.SupportsSpotSymbolAsync(string symbolName)
        => ((ISpotSymbolRestClient)this).SupportsSpotSymbolAsync(symbolName);

    Task<HttpResult<SharedSpotSymbol[]>> IGetSpotSymbolsRest.GetSpotSymbolsAsync(GetSymbolsRequest request, CancellationToken ct)
        => ((ISpotSymbolRestClient)this).GetSpotSymbolsAsync(request, ct);

    async Task<IExchangeCallResult<SharedSpotSymbol[]>> IGetSpotSymbols.GetSpotSymbolsAsync(GetSymbolsRequest request, CancellationToken ct)
        => await ((IGetSpotSymbolsRest)this).GetSpotSymbolsAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetTickerRest

    GetTickerOptions IGetTicker.GetTickerOptions => ((ISpotTickerRestClient)this).GetSpotTickerOptions;

    async Task<HttpResult<SharedTicker>> IGetTickerRest.GetTickerAsync(GetTickerRequest request, CancellationToken ct)
    {
        var result = await ((ISpotTickerRestClient)this).GetSpotTickerAsync(request, ct).ConfigureAwait(false);
        return result.Success
            ? HttpResult.Ok<SharedTicker>(result, result.Data)
            : HttpResult.Fail<SharedTicker>(result);
    }

    async Task<IExchangeCallResult<SharedTicker>> IGetTicker.GetTickerAsync(GetTickerRequest request, CancellationToken ct)
        => await ((IGetTickerRest)this).GetTickerAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetAllTickersRest

    GetAllTickersOptions IGetAllTickers.GetAllTickersOptions => ((ISpotTickerRestClient)this).GetSpotTickersOptions;

    async Task<HttpResult<SharedTicker[]>> IGetAllTickersRest.GetAllTickersAsync(GetTickersRequest request, CancellationToken ct)
    {
        var result = await ((ISpotTickerRestClient)this).GetSpotTickersAsync(request, ct).ConfigureAwait(false);
        return result.Success
            ? HttpResult.Ok<SharedTicker[]>(result, result.Data)
            : HttpResult.Fail<SharedTicker[]>(result);
    }

    async Task<IExchangeCallResult<SharedTicker[]>> IGetAllTickers.GetAllTickersAsync(GetTickersRequest request, CancellationToken ct)
        => await ((IGetAllTickersRest)this).GetAllTickersAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetOrderBookRest

    GetOrderBookOptions IGetOrderBook.GetOrderBookOptions => ((IOrderBookRestClient)this).GetOrderBookOptions;

    Task<HttpResult<SharedOrderBook>> IGetOrderBookRest.GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
        => ((IOrderBookRestClient)this).GetOrderBookAsync(request, ct);

    async Task<IExchangeCallResult<SharedOrderBook>> IGetOrderBook.GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
        => await ((IGetOrderBookRest)this).GetOrderBookAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetRecentTradesRest

    GetRecentTradesOptions IGetRecentTrades.GetRecentTradesOptions => ((IRecentTradeRestClient)this).GetRecentTradesOptions;

    Task<HttpResult<SharedTrade[]>> IGetRecentTradesRest.GetRecentTradesAsync(GetRecentTradesRequest request, CancellationToken ct)
        => ((IRecentTradeRestClient)this).GetRecentTradesAsync(request, ct);

    async Task<IExchangeCallResult<SharedTrade[]>> IGetRecentTrades.GetRecentTradesAsync(GetRecentTradesRequest request, CancellationToken ct)
        => await ((IGetRecentTradesRest)this).GetRecentTradesAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetKlinesRest

    GetKlinesOptions IGetKlines.GetKlinesOptions => ((IKlineRestClient)this).GetKlinesOptions;

    Task<HttpResult<SharedKline[]>> IGetKlinesRest.GetKlinesAsync(GetKlinesRequest request, PageRequest? pageRequest, CancellationToken ct)
        => ((IKlineRestClient)this).GetKlinesAsync(request, pageRequest, ct);

    async Task<IExchangeCallResult<SharedKline[]>> IGetKlines.GetKlinesAsync(GetKlinesRequest request, PageRequest? pageRequest, CancellationToken ct)
        => await ((IGetKlinesRest)this).GetKlinesAsync(request, pageRequest, ct).ConfigureAwait(false);

    #endregion

    #region IGetBalancesRest

    GetBalancesOptions IGetBalances.GetBalancesOptions => ((IBalanceRestClient)this).GetBalancesOptions;

    Task<HttpResult<SharedBalance[]>> IGetBalancesRest.GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
        => ((IBalanceRestClient)this).GetBalancesAsync(request, ct);

    async Task<IExchangeCallResult<SharedBalance[]>> IGetBalances.GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
        => await ((IGetBalancesRest)this).GetBalancesAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IPlaceSpotOrderRest

    SharedFeeDeductionType IPlaceSpotOrder.SpotFeeDeductionType => ((ISpotOrderRestClient)this).SpotFeeDeductionType;
    SharedFeeAssetType IPlaceSpotOrder.SpotFeeAssetType => ((ISpotOrderRestClient)this).SpotFeeAssetType;
    SharedOrderType[] IPlaceSpotOrder.SpotSupportedOrderTypes => ((ISpotOrderRestClient)this).SpotSupportedOrderTypes;
    SharedTimeInForce[] IPlaceSpotOrder.SpotSupportedTimeInForce => ((ISpotOrderRestClient)this).SpotSupportedTimeInForce;
    SharedQuantitySupport IPlaceSpotOrder.SpotSupportedOrderQuantity => ((ISpotOrderRestClient)this).SpotSupportedOrderQuantity;
    PlaceSpotOrderOptions IPlaceSpotOrder.PlaceSpotOrderOptions => ((ISpotOrderRestClient)this).PlaceSpotOrderOptions;

    string IPlaceSpotOrder.GenerateClientOrderId() => ((ISpotOrderRestClient)this).GenerateClientOrderId();

    Task<HttpResult<SharedId>> IPlaceSpotOrderRest.PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct)
        => ((ISpotOrderRestClient)this).PlaceSpotOrderAsync(request, ct);

    async Task<IExchangeCallResult<SharedId>> IPlaceSpotOrder.PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct)
        => await ((IPlaceSpotOrderRest)this).PlaceSpotOrderAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetSpotOrderRest

    GetSpotOrderOptions IGetSpotOrder.GetSpotOrderOptions => ((ISpotOrderRestClient)this).GetSpotOrderOptions;

    Task<HttpResult<SharedSpotOrder>> IGetSpotOrderRest.GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct)
        => ((ISpotOrderRestClient)this).GetSpotOrderAsync(request, ct);

    async Task<IExchangeCallResult<SharedSpotOrder>> IGetSpotOrder.GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct)
        => await ((IGetSpotOrderRest)this).GetSpotOrderAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetOpenSpotOrdersRest

    GetOpenSpotOrdersOptions IGetOpenSpotOrders.GetOpenSpotOrdersOptions => ((ISpotOrderRestClient)this).GetOpenSpotOrdersOptions;

    Task<HttpResult<SharedSpotOrder[]>> IGetOpenSpotOrdersRest.GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct)
        => ((ISpotOrderRestClient)this).GetOpenSpotOrdersAsync(request, ct);

    async Task<IExchangeCallResult<SharedSpotOrder[]>> IGetOpenSpotOrders.GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct)
        => await ((IGetOpenSpotOrdersRest)this).GetOpenSpotOrdersAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetClosedSpotOrdersRest

    GetSpotClosedOrdersOptions IGetClosedSpotOrders.GetClosedSpotOrdersOptions => ((ISpotOrderRestClient)this).GetClosedSpotOrdersOptions;

    Task<HttpResult<SharedSpotOrder[]>> IGetClosedSpotOrdersRest.GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, PageRequest? pageRequest, CancellationToken ct)
        => ((ISpotOrderRestClient)this).GetClosedSpotOrdersAsync(request, pageRequest, ct);

    async Task<IExchangeCallResult<SharedSpotOrder[]>> IGetClosedSpotOrders.GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, PageRequest? pageRequest, CancellationToken ct)
        => await ((IGetClosedSpotOrdersRest)this).GetClosedSpotOrdersAsync(request, pageRequest, ct).ConfigureAwait(false);

    #endregion

    #region ICancelSpotOrderRest

    CancelSpotOrderOptions ICancelSpotOrder.CancelSpotOrderOptions => ((ISpotOrderRestClient)this).CancelSpotOrderOptions;

    Task<HttpResult<SharedId>> ICancelSpotOrderRest.CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct)
        => ((ISpotOrderRestClient)this).CancelSpotOrderAsync(request, ct);

    async Task<IExchangeCallResult<SharedId>> ICancelSpotOrder.CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct)
        => await ((ICancelSpotOrderRest)this).CancelSpotOrderAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetSpotOrderTradesRest

    GetSpotOrderTradesOptions IGetSpotOrderTrades.GetSpotOrderTradesOptions => ((ISpotOrderRestClient)this).GetSpotOrderTradesOptions;

    Task<HttpResult<SharedUserTrade[]>> IGetSpotOrderTradesRest.GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct)
        => ((ISpotOrderRestClient)this).GetSpotOrderTradesAsync(request, ct);

    async Task<IExchangeCallResult<SharedUserTrade[]>> IGetSpotOrderTrades.GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct)
        => await ((IGetSpotOrderTradesRest)this).GetSpotOrderTradesAsync(request, ct).ConfigureAwait(false);

    #endregion

    #region IGetSpotUserTradeHistoryRest

    GetSpotUserTradeHistoryOptions IGetSpotUserTradeHistory.GetSpotUserTradeHistoryOptions => ((ISpotOrderRestClient)this).GetSpotUserTradesOptions;

    Task<HttpResult<SharedUserTrade[]>> IGetSpotUserTradeHistoryRest.GetSpotUserTradeHistoryAsync(GetUserTradesRequest request, PageRequest? pageRequest, CancellationToken ct)
        => ((ISpotOrderRestClient)this).GetSpotUserTradesAsync(request, pageRequest, ct);

    async Task<IExchangeCallResult<SharedUserTrade[]>> IGetSpotUserTradeHistory.GetSpotUserTradeHistoryAsync(GetUserTradesRequest request, PageRequest? pageRequest, CancellationToken ct)
        => await ((IGetSpotUserTradeHistoryRest)this).GetSpotUserTradeHistoryAsync(request, pageRequest, ct).ConfigureAwait(false);

    #endregion
}
