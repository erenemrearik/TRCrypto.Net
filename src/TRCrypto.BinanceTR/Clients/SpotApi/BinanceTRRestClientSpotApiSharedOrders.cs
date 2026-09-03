using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Objects.Models;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <summary>
/// Binance TR spot REST API'sinin bakiye ve emir tarafindaki borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// Bu arayuzler API anahtari gerektirir. Sema resmi dokumantasyondan alinmistir; canli
/// bir hesaba karsi dogrulama anahtar geldiginde yapilacaktir.
/// </remarks>
internal partial class BinanceTRRestClientSpotApi
{
    #region Balance client

    GetBalancesOptions IBalanceRestClient.GetBalancesOptions { get; }
        = new(BinanceTRExchange.ExchangeName, AccountTypeFilter.Spot);

    async Task<HttpResult<SharedBalance[]>> IBalanceRestClient.GetBalancesAsync(
        GetBalancesRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetBalancesOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedBalance[]>(Exchange, validationError);

        var result = await Account.GetAccountAsync(ct: ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedBalance[]>(result);

        return HttpResult.Ok(result, result.Data.Assets
            .Select(x => new SharedBalance(TradingMode.Spot, x.Asset, x.Available, x.Total))
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

    SharedTimeInForce[] ISpotOrderRestClient.SpotSupportedTimeInForce { get; } =
    [
        SharedTimeInForce.GoodTillCanceled,
        SharedTimeInForce.ImmediateOrCancel,
        SharedTimeInForce.FillOrKill
    ];

    SharedQuantitySupport ISpotOrderRestClient.SpotSupportedOrderQuantity { get; } = new(
        SharedQuantityType.BaseAsset,
        SharedQuantityType.QuoteAsset,
        SharedQuantityType.BaseAsset,
        SharedQuantityType.BaseAsset);

    string ISpotOrderRestClient.GenerateClientOrderId() => Guid.NewGuid().ToString("N").Substring(0, 24);

    PlaceSpotOrderOptions ISpotOrderRestClient.PlaceSpotOrderOptions { get; }
        = new(BinanceTRExchange.ExchangeName);

    async Task<HttpResult<SharedId>> ISpotOrderRestClient.PlaceSpotOrderAsync(
        PlaceSpotOrderRequest request,
        CancellationToken ct)
    {
        var validationError = ((ISpotOrderRestClient)this).PlaceSpotOrderOptions
            .ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedId>(Exchange, validationError);

        var result = await Trading.PlaceOrderAsync(
            request.Symbol!.GetSymbol(FormatSymbol),
            request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
            request.OrderType == SharedOrderType.Market ? Enums.OrderType.Market : Enums.OrderType.Limit,
            quantity: request.Quantity?.QuantityInBaseAsset,
            quoteQuantity: request.Quantity?.QuantityInQuoteAsset,
            price: request.Price,
            clientOrderId: request.ClientOrderId,
            timeInForce: ToNativeTimeInForce(request.TimeInForce),
            ct: ct).ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedId>(result);

        return HttpResult.Ok(result, new SharedId(result.Data.OrderId.ToString()));
    }

    GetSpotOrderOptions ISpotOrderRestClient.GetSpotOrderOptions { get; }
        = new(BinanceTRExchange.ExchangeName, true);

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

        var result = await Trading.GetOrderAsync(orderId, ct: ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedSpotOrder>(result);

        return HttpResult.Ok(result, ParseOrder(result.Data, request.Symbol));
    }

    GetOpenSpotOrdersOptions ISpotOrderRestClient.GetOpenSpotOrdersOptions { get; }
        = new(BinanceTRExchange.ExchangeName, true);

    async Task<HttpResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetOpenSpotOrdersAsync(
        GetOpenOrdersRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetOpenSpotOrdersOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotOrder[]>(Exchange, validationError);

        // Borsanin emir ucu pariteyi zorunlu tutar; parite verilmeden acik emirler alinamaz.
        if (request.Symbol == null)
        {
            return HttpResult.Fail<SharedSpotOrder[]>(
                Exchange, ArgumentError.Missing(nameof(request.Symbol), "Binance TR acik emirler icin parite ister."));
        }

        var result = await Trading
            .GetOrdersAsync(request.Symbol.GetSymbol(FormatSymbol), ct: ct)
            .ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedSpotOrder[]>(result);

        return HttpResult.Ok(result, result.Data.Orders
            .Where(IsOpen)
            .Select(x => ParseOrder(x, request.Symbol))
            .ToArray());
    }

    GetSpotClosedOrdersOptions ISpotOrderRestClient.GetClosedSpotOrdersOptions { get; }
        = new(
            BinanceTRExchange.ExchangeName,
            supportsAscending: true,
            supportsDescending: true,
            timeFilterSupported: true,
            maxLimit: 1000);

    async Task<HttpResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetClosedSpotOrdersAsync(
        GetClosedOrdersRequest request,
        PageRequest? pageRequest,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetClosedSpotOrdersOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedSpotOrder[]>(Exchange, validationError);

        var result = await Trading.GetOrdersAsync(
            request.Symbol!.GetSymbol(FormatSymbol),
            startTime: request.StartTime,
            endTime: request.EndTime,
            limit: request.Limit,
            ct: ct).ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedSpotOrder[]>(result);

        return HttpResult.Ok(result, result.Data.Orders
            .Where(x => !IsOpen(x))
            .Select(x => ParseOrder(x, request.Symbol))
            .ToArray());
    }

    CancelSpotOrderOptions ISpotOrderRestClient.CancelSpotOrderOptions { get; }
        = new(BinanceTRExchange.ExchangeName, true);

    async Task<HttpResult<SharedId>> ISpotOrderRestClient.CancelSpotOrderAsync(
        CancelOrderRequest request,
        CancellationToken ct)
    {
        var validationError = SharedClient.CancelSpotOrderOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedId>(Exchange, validationError);

        if (!long.TryParse(request.OrderId, out var orderId))
            return HttpResult.Fail<SharedId>(Exchange, ArgumentError.Invalid(nameof(request.OrderId), "Emir kimligi sayisal olmalidir."));

        var result = await Trading.CancelOrderAsync(orderId, ct: ct).ConfigureAwait(false);
        if (!result.Success)
            return HttpResult.Fail<SharedId>(result);

        return HttpResult.Ok(result, new SharedId(result.Data.OrderId.ToString()));
    }


    GetSpotOrderTradesOptions ISpotOrderRestClient.GetSpotOrderTradesOptions { get; }
        = new(BinanceTRExchange.ExchangeName, true);

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

        // Borsanin islem ucu pariteyi zorunlu tutar; emir kimligi tek basina yetmez.
        var result = await Trading.GetUserTradesAsync(
            request.Symbol!.GetSymbol(FormatSymbol),
            orderId: orderId,
            ct: ct).ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedUserTrade[]>(result);

        return HttpResult.Ok(result, result.Data.Trades
            .Select(x => ParseUserTrade(x, request.Symbol))
            .ToArray());
    }

    GetSpotUserTradesOptions ISpotOrderRestClient.GetSpotUserTradesOptions { get; }
        = new(
            BinanceTRExchange.ExchangeName,
            supportsAscending: true,
            supportsDescending: true,
            timeFilterSupported: true,
            maxLimit: 1000);

    async Task<HttpResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotUserTradesAsync(
        GetUserTradesRequest request,
        PageRequest? pageRequest,
        CancellationToken ct)
    {
        var validationError = SharedClient.GetSpotUserTradesOptions.ValidateRequest(request, this);
        if (validationError != null)
            return HttpResult.Fail<SharedUserTrade[]>(Exchange, validationError);

        var result = await Trading.GetUserTradesAsync(
            request.Symbol!.GetSymbol(FormatSymbol),
            startTime: request.StartTime,
            endTime: request.EndTime,
            limit: request.Limit,
            ct: ct).ConfigureAwait(false);

        if (!result.Success)
            return HttpResult.Fail<SharedUserTrade[]>(result);

        return HttpResult.Ok(result, result.Data.Trades
            .Select(x => ParseUserTrade(x, request.Symbol))
            .ToArray());
    }

    private SharedUserTrade ParseUserTrade(BinanceTRUserTrade trade, SharedSymbol? requested)
    {
        var symbol = requested
            ?? ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, trade.Symbol)
            ?? new SharedSymbol(TradingMode.Spot, trade.Symbol, string.Empty);

        // Yon ayri bir alanda degil, alici olup olmadigi bayraginda tasinir.
        return new SharedUserTrade(
            symbol,
            trade.Symbol,
            trade.OrderId.ToString(),
            trade.TradeId.ToString(),
            trade.IsBuyer ? SharedOrderSide.Buy : SharedOrderSide.Sell,
            new SharedOrderQuantity(trade.Quantity, trade.QuoteQuantity),
            trade.Price,
            trade.Timestamp)
        {
            Fee = trade.Commission,
            FeeAsset = trade.CommissionAsset,
            Role = trade.IsMaker ? SharedRole.Maker : SharedRole.Taker
        };
    }
    /// <summary>Emrin hala defterde acik olup olmadigini soyler.</summary>
    private static bool IsOpen(BinanceTROrder order)
        => order.Status is Enums.OrderStatus.New
            or Enums.OrderStatus.PartiallyFilled
            or Enums.OrderStatus.SystemProcessing;

    private SharedSpotOrder ParseOrder(BinanceTROrder order, SharedSymbol? requested)
    {
        // Native sembol adi ayristirilmaz; onbellekte varsa oradan, yoksa istekten alinir.
        var symbol = requested
            ?? ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, order.Symbol)
            ?? new SharedSymbol(TradingMode.Spot, order.Symbol, string.Empty);

        return new SharedSpotOrder(
            symbol,
            order.Symbol,
            order.OrderId.ToString(),
            order.Type == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Limit,
            order.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
            ToSharedStatus(order.Status),
            order.CreateTime)
        {
            ClientOrderId = order.ClientOrderId,
            OrderPrice = order.Price,
            AveragePrice = order.AverageFillPrice == 0 ? null : order.AverageFillPrice,
            OrderQuantity = new SharedOrderQuantity(order.Quantity, order.QuoteQuantity),
            QuantityFilled = new SharedOrderQuantity(order.QuantityFilled, order.QuoteQuantityFilled),
            TimeInForce = ToSharedTimeInForce(order.TimeInForce)
        };
    }

    /// <summary>
    /// Borsanin sayisal durum kodunu borsadan bagimsiz karsiligina cevirir.
    /// </summary>
    /// <remarks>
    /// Reddedilen ve suresi dolan emirler shared modelde ayri bir durum tasimaz; ikisi de
    /// artik islem gormeyecegi icin iptal olarak bildirilir. Native durum korunur.
    /// </remarks>
    private static SharedOrderStatus ToSharedStatus(Enums.OrderStatus status)
        => status switch
        {
            Enums.OrderStatus.SystemProcessing => SharedOrderStatus.Open,
            Enums.OrderStatus.New => SharedOrderStatus.Open,
            Enums.OrderStatus.PartiallyFilled => SharedOrderStatus.Open,
            Enums.OrderStatus.PendingCancel => SharedOrderStatus.Open,
            Enums.OrderStatus.Filled => SharedOrderStatus.Filled,
            Enums.OrderStatus.Canceled => SharedOrderStatus.Canceled,
            Enums.OrderStatus.Rejected => SharedOrderStatus.Canceled,
            Enums.OrderStatus.Expired => SharedOrderStatus.Canceled,
            _ => SharedOrderStatus.Unknown
        };

    private static Enums.TimeInForce? ToNativeTimeInForce(SharedTimeInForce? timeInForce)
        => timeInForce switch
        {
            SharedTimeInForce.GoodTillCanceled => Enums.TimeInForce.GoodTillCanceled,
            SharedTimeInForce.ImmediateOrCancel => Enums.TimeInForce.ImmediateOrCancel,
            SharedTimeInForce.FillOrKill => Enums.TimeInForce.FillOrKill,
            _ => null
        };

    private static SharedTimeInForce? ToSharedTimeInForce(Enums.TimeInForce? timeInForce)
        => timeInForce switch
        {
            Enums.TimeInForce.GoodTillCanceled => SharedTimeInForce.GoodTillCanceled,
            Enums.TimeInForce.ImmediateOrCancel => SharedTimeInForce.ImmediateOrCancel,
            Enums.TimeInForce.FillOrKill => SharedTimeInForce.FillOrKill,
            _ => null
        };

    #endregion
}
