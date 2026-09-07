using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <summary>
/// Kullanici akisinin borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// <para>
/// Bu akis bir dinleme tokeni ister, ama borsadan bagimsiz istek nesnelerinde boyle bir
/// alan yoktur. Token bu yuzden borsaya ozgu parametre olarak verilir:
/// </para>
/// <code>
/// var token = await rest.SpotApi.Account.GetListenTokenAsync();
/// socket.UserApi.SharedClient.SetDefaultExchangeParameter(
///     BinanceTRSocketClientUserApi.ListenTokenParameter, token.Data.Token);
/// </code>
/// <para>
/// Token verilmediginde abonelik kurulmaz ve hata bunu acikca soyler. Sessizce bos bir
/// akis dondurmek, sorunun ancak "veri gelmiyor" olarak fark edilmesine yol acardi.
/// </para>
/// </remarks>
internal partial class BinanceTRSocketClientUserApi : IBinanceTRSocketClientUserApiShared
{
    /// <summary>Dinleme tokeninin verildigi borsaya ozgu parametre adi.</summary>
    public const string ListenTokenParameter = "listenToken";

    /// <inheritdoc />
    public TradingMode[] SupportedTradingModes { get; } = [TradingMode.Spot];

    /// <inheritdoc />
    public void SetDefaultExchangeParameter(string key, object value)
        => ExchangeParameters.SetStaticParameter(Exchange, key, value);

    /// <inheritdoc />
    public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

    /// <inheritdoc />
    public SharedClientInfo Discover() => SharedUtils.GetClientInfo(BinanceTRExchange.Metadata, this);

    /// <inheritdoc />
    public IBinanceTRSocketClientUserApiShared SharedClient => this;

    #region Balance

    SubscribeBalanceOptions IBalanceSocketClient.SubscribeBalanceOptions { get; }
        = new(BinanceTRExchange.ExchangeName, true)
        {
            RequestNotes =
                "Dinleme tokeni gerekir. Token SpotApi.Account.GetListenTokenAsync ile alinir " +
                "ve SetDefaultExchangeParameter(\"listenToken\", …) ile verilir."
        };

    async Task<WebSocketResult<UpdateSubscription>> IBalanceSocketClient.SubscribeToBalanceUpdatesAsync(
        SubscribeBalancesRequest request,
        Action<DataEvent<SharedBalance[]>> handler,
        CancellationToken ct)
    {
        var validationError = ((IBalanceSocketClient)this).SubscribeBalanceOptions
            .ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        if (!TryGetListenToken(request.ExchangeParameters, out var token, out var tokenError))
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, tokenError!);

        return await SubscribeToAccountUpdatesAsync(token!, update => handler(Convert(update,
            update.Data.Balances
                .Select(x => new SharedBalance(TradingMode.Spot, x.Asset, x.Available, x.Total))
                .ToArray())), ct).ConfigureAwait(false);
    }

    #endregion

    #region Spot orders

    SubscribeSpotOrderOptions ISpotOrderSocketClient.SubscribeSpotOrderOptions { get; }
        = new(BinanceTRExchange.ExchangeName, true)
        {
            RequestNotes =
                "Dinleme tokeni gerekir. Token SpotApi.Account.GetListenTokenAsync ile alinir " +
                "ve SetDefaultExchangeParameter(\"listenToken\", …) ile verilir."
        };

    async Task<WebSocketResult<UpdateSubscription>> ISpotOrderSocketClient.SubscribeToSpotOrderUpdatesAsync(
        SubscribeSpotOrderRequest request,
        Action<DataEvent<SharedSpotOrder[]>> handler,
        CancellationToken ct)
    {
        var validationError = ((ISpotOrderSocketClient)this).SubscribeSpotOrderOptions
            .ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        if (!TryGetListenToken(request.ExchangeParameters, out var token, out var tokenError))
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, tokenError!);

        return await SubscribeToOrderUpdatesAsync(token!, update => handler(Convert(update,
            new[] { ToSharedOrder(update.Data) })), ct).ConfigureAwait(false);
    }

    /// <summary>Native emir guncellemesini borsadan bagimsiz karsiligina cevirir.</summary>
    private static SharedSpotOrder ToSharedOrder(Objects.Models.Socket.BinanceTRStreamOrderUpdate update)
        => new(
            // Akis base ve quote varligi ayri alanlarda vermiyor; yalnizca native ad var.
            new SharedSymbol(TradingMode.Spot, update.Symbol, string.Empty),
            update.Symbol,
            update.OrderId.ToString(),
            update.Type == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Limit,
            update.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
            ToSharedStatus(update.Status),
            update.CreateTime)
        {
            ClientOrderId = update.ClientOrderId,
            OrderPrice = update.Price,
            AveragePrice = update.LastPriceFilled == 0 ? null : update.LastPriceFilled,
            OrderQuantity = new SharedOrderQuantity(update.Quantity),
            QuantityFilled = new SharedOrderQuantity(update.QuantityFilled, update.QuoteQuantityFilled),
            Fee = update.Fee,
            FeeAsset = update.FeeAsset,
            UpdateTime = update.EventTime
        };

    #endregion

    /// <summary>
    /// Dinleme tokenini borsaya ozgu parametrelerden okur.
    /// </summary>
    /// <remarks>
    /// Once istekle birlikte verilen deger, sonra varsayilan olarak ayarlanmis deger
    /// aranir. Ikisi de yoksa abonelik kurulmaz.
    /// </remarks>
    private bool TryGetListenToken(
        ExchangeParameters? parameters,
        out string? token,
        out Error? error)
    {
        token = null;
        error = null;

        if (ExchangeParameters.HasValue(parameters, Exchange, ListenTokenParameter, typeof(string)))
            token = ExchangeParameters.GetValue<string>(parameters, Exchange, ListenTokenParameter);

        if (!string.IsNullOrWhiteSpace(token))
            return true;

        error = new InvalidOperationError(
            "Kullanici akisi dinleme tokeni gerektirir. Token " +
            "SpotApi.Account.GetListenTokenAsync ile alinir ve " +
            $"SetDefaultExchangeParameter(\"{ListenTokenParameter}\", token) ile verilir.");

        return false;
    }

    /// <summary>
    /// Borsanin durum degerini borsadan bagimsiz karsiligina cevirir.
    /// </summary>
    /// <remarks>
    /// Akista durum metin olarak gelir; REST tarafinda ayni bilgi sayidir. Iki taraf ayni
    /// enum'a cozumlenir, bu yuzden esleme burada da gerekir.
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

    /// <summary>
    /// Native bir guncellemeyi, zaman ve kaynak bilgisini koruyarak borsadan bagimsiz
    /// karsiligina cevirir.
    /// </summary>
    private static DataEvent<TShared> Convert<TNative, TShared>(
        DataEvent<TNative> update,
        TShared data)
        => new DataEvent<TShared>(
            update.Exchange,
            data,
            update.ReceiveTime,
            update.OriginalData)
        {
            Symbol = update.Symbol,
            StreamId = update.StreamId,
            UpdateType = update.UpdateType
        };
}
