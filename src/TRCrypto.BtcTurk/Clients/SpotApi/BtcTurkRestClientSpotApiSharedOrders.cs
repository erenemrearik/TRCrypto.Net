using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <summary>
/// Emir ve mum verisinin borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// BtcTurk'e ozgu alanlar (ornegin islem vergisi) bu yuzeyde temsil edilemez; bunlara
/// ihtiyac duyan tuketiciler native API kullanmalidir.
/// </remarks>
internal partial class BtcTurkRestClientSpotApi
{
    #region Kline client

    GetKlinesOptions IKlineRestClient.GetKlinesOptions { get; } = new(
        BtcTurkExchange.ExchangeName,
        supportsAscending: true,
        supportsDescending: false,
        timeFilterSupported: true,
        maxLimit: 0,
        needsAuthentication: false,
        SharedKlineInterval.OneMinute,
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
                new SharedOrderQuantity(x.Volume)))
            .ToArray());
    }

    #endregion

    #region Spot Order client

    SharedFeeDeductionType ISpotOrderRestClient.SpotFeeDeductionType
        => SharedFeeDeductionType.DeductFromOutput;

    SharedFeeAssetType ISpotOrderRestClient.SpotFeeAssetType
        => SharedFeeAssetType.QuoteAsset;

    SharedOrderType[] ISpotOrderRestClient.SpotSupportedOrderTypes { get; }
        = [SharedOrderType.Limit, SharedOrderType.Market];

    // BtcTurk emirlerde time-in-force secenegi sunmaz.
    SharedTimeInForce[] ISpotOrderRestClient.SpotSupportedTimeInForce { get; } = [];

    // Miktar her zaman base varlik cinsinden verilir; quote cinsinden emir desteklenmez.
    SharedQuantitySupport ISpotOrderRestClient.SpotSupportedOrderQuantity { get; } = new(
        SharedQuantityType.BaseAsset,
        SharedQuantityType.BaseAsset,
        SharedQuantityType.BaseAsset,
        SharedQuantityType.BaseAsset);

    string ISpotOrderRestClient.GenerateClientOrderId() => ExchangeHelpers.RandomString(16);

    PlaceSpotOrderOptions ISpotOrderRestClient.PlaceSpotOrderOptions { get; }
        = new(BtcTurkExchange.ExchangeName);

    async Task<HttpResult<SharedId>> ISpotOrderRestClient.PlaceSpotOrderAsync(
        PlaceSpotOrderRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.PlaceSpotOrderOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedId>(Exchange, validationError);

        var result = await Trading.PlaceOrderAsync(
            request.Symbol!.GetSymbol(FormatSymbol),
            request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
            request.OrderType == SharedOrderType.Market ? Enums.OrderMethod.Market : Enums.OrderMethod.Limit,
            request.Quantity?.QuantityInBaseAsset ?? 0,
            request.Price,
            clientOrderId: request.ClientOrderId,
            ct: ct).ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedId>(result);

        return HttpResult.Ok(result, new SharedId(result.Data.Id.ToString()));
    }

    GetSpotOrderOptions ISpotOrderRestClient.GetSpotOrderOptions { get; }
        = new(BtcTurkExchange.ExchangeName, true);

    async Task<HttpResult<SharedSpotOrder>> ISpotOrderRestClient.GetSpotOrderAsync(
        GetOrderRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotOrderOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotOrder>(Exchange, validationError);

        if (!long.TryParse(request.OrderId, out var orderId))
        {
            return HttpResult.Fail<SharedSpotOrder>(
                Exchange, ArgumentError.Invalid(nameof(request.OrderId), "Emir kimligi sayisal olmalidir."));
        }

        var result = await Trading.GetOrderAsync(orderId, ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotOrder>(result);

        return HttpResult.Ok(result, ParseOrder(result.Data, request.Symbol));
    }

    GetOpenSpotOrdersOptions ISpotOrderRestClient.GetOpenSpotOrdersOptions { get; }
        = new(BtcTurkExchange.ExchangeName, true);

    async Task<HttpResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetOpenSpotOrdersAsync(
        GetOpenOrdersRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetOpenSpotOrdersOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotOrder[]>(Exchange, validationError);

        var symbol = request.Symbol?.GetSymbol(FormatSymbol);
        var result = await Trading.GetOpenOrdersAsync(symbol, ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotOrder[]>(result);

        // Native uc alis ve satis emirlerini ayri listelerde dondurur; shared yuzey tek liste bekler.
        var orders = result.Data.Bids
            .Concat(result.Data.Asks)
            .Select(x => ParseOrder(x, request.Symbol))
            .ToArray();

        return HttpResult.Ok(result, orders);
    }

    GetSpotClosedOrdersOptions ISpotOrderRestClient.GetClosedSpotOrdersOptions { get; }
        = new(
            BtcTurkExchange.ExchangeName,
            supportsAscending: false,
            supportsDescending: true,
            timeFilterSupported: true,
            maxLimit: 1000);

    async Task<HttpResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetClosedSpotOrdersAsync(
        GetClosedOrdersRequest request,
        PageRequest? nextPageToken,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetClosedSpotOrdersOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotOrder[]>(Exchange, validationError);

        var symbol = request.Symbol?.GetSymbol(FormatSymbol);
        var result = await Trading.GetOrdersAsync(
            symbol, request.StartTime, request.EndTime, limit: request.Limit, ct: ct)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedSpotOrder[]>(result);

        // Acik emirler bu listede de gorunur; shared sozlesmesi yalnizca kapanmislari bekler.
        var closed = result.Data
            .Where(x => x.Status is Enums.OrderStatus.Filled or Enums.OrderStatus.Canceled)
            .Select(x => ParseOrder(x, request.Symbol))
            .ToArray();

        return HttpResult.Ok(result, closed);
    }

    CancelSpotOrderOptions ISpotOrderRestClient.CancelSpotOrderOptions { get; }
        = new(BtcTurkExchange.ExchangeName, true)
        {
            RequestNotes = "Basarili yanit istegin alindigini bildirir; iptalin kesinlesmesi " +
                           "WebSocket uzerinden duyurulur. Emrin durumu ayrica sorgulanmalidir."
        };

    async Task<HttpResult<SharedId>> ISpotOrderRestClient.CancelSpotOrderAsync(
        CancelOrderRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.CancelSpotOrderOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedId>(Exchange, validationError);

        if (!long.TryParse(request.OrderId, out var orderId))
        {
            return HttpResult.Fail<SharedId>(
                Exchange, ArgumentError.Invalid(nameof(request.OrderId), "Emir kimligi sayisal olmalidir."));
        }

        var result = await Trading.CancelOrderAsync(orderId, ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedId>(result);

        return HttpResult.Ok(result, new SharedId(request.OrderId));
    }

    GetSpotOrderTradesOptions ISpotOrderRestClient.GetSpotOrderTradesOptions { get; }
        = new(BtcTurkExchange.ExchangeName, true);

    async Task<HttpResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotOrderTradesAsync(
        GetOrderTradesRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotOrderTradesOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedUserTrade[]>(Exchange, validationError);

        if (!long.TryParse(request.OrderId, out var orderId))
        {
            return HttpResult.Fail<SharedUserTrade[]>(
                Exchange, ArgumentError.Invalid(nameof(request.OrderId), "Emir kimligi sayisal olmalidir."));
        }

        var result = await Account.GetUserTradesAsync(orderId: orderId, ct: ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedUserTrade[]>(result);

        return HttpResult.Ok(result, result.Data.Select(x => ParseUserTrade(x, request.Symbol)).ToArray());
    }

    GetSpotUserTradesOptions ISpotOrderRestClient.GetSpotUserTradesOptions { get; }
        = new(
            BtcTurkExchange.ExchangeName,
            supportsAscending: false,
            supportsDescending: true,
            timeFilterSupported: true,
            // Borsa bu ucta sayfalama ya da kayit siniri sunmaz.
            maxLimit: 0);

    async Task<HttpResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotUserTradesAsync(
        GetUserTradesRequest request,
        PageRequest? nextPageToken,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotUserTradesOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedUserTrade[]>(Exchange, validationError);

        var result = await Account.GetUserTradesAsync(
            request.Symbol?.GetSymbol(FormatSymbol),
            startTime: request.StartTime,
            endTime: request.EndTime,
            ct: ct).ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedUserTrade[]>(result);

        return HttpResult.Ok(result, result.Data.Select(x => ParseUserTrade(x, request.Symbol)).ToArray());
    }

    private SharedSpotOrder ParseOrder(BtcTurkOrder order, SharedSymbol? requested)
    {
        var symbol = requested
            ?? ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, order.PairSymbol)
            ?? new SharedSymbol(TradingMode.Spot, order.PairSymbol, string.Empty);

        return new SharedSpotOrder(
            symbol,
            order.PairSymbol,
            order.Id.ToString(),
            order.Method == Enums.OrderMethod.Market ? SharedOrderType.Market : SharedOrderType.Limit,
            order.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
            ParseStatus(order.Status),
            order.CreateTime)
        {
            ClientOrderId = order.ClientOrderId,
            OrderPrice = order.Price,
            OrderQuantity = new SharedOrderQuantity(order.Quantity),
            UpdateTime = order.UpdateTime
        };
    }

    private static SharedOrderStatus ParseStatus(Enums.OrderStatus status)
        => status switch
        {
            Enums.OrderStatus.Untouched => SharedOrderStatus.Open,
            Enums.OrderStatus.PartiallyFilled => SharedOrderStatus.Open,
            Enums.OrderStatus.Filled => SharedOrderStatus.Filled,
            Enums.OrderStatus.Canceled => SharedOrderStatus.Canceled,
            _ => SharedOrderStatus.Unknown
        };

    private static SharedUserTrade ParseUserTrade(BtcTurkUserTrade trade, SharedSymbol? requested)
    {
        var nativeSymbol = trade.NumeratorSymbol + trade.DenominatorSymbol;
        var symbol = requested
            ?? new SharedSymbol(TradingMode.Spot, trade.NumeratorSymbol, trade.DenominatorSymbol);

        // Native tutarlar isaretlidir (satista negatif); shared model buyuklugu bekler,
        // yon zaten Side alaninda tasinir.
        return new SharedUserTrade(
            symbol,
            nativeSymbol,
            trade.OrderId.ToString(),
            trade.Id.ToString(),
            trade.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
            new SharedOrderQuantity(Math.Abs(trade.PreciseQuantity)),
            trade.Price,
            trade.Timestamp)
        {
            // Vergi ayri bir alandir ve shared modelde karsiligi yoktur; yalnizca komisyon
            // aktarilir. Vergi bilgisi icin native API kullanilmalidir.
            Fee = Math.Abs(trade.Fee),
            FeeAsset = trade.DenominatorSymbol
        };
    }

    #endregion
}
