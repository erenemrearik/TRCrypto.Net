using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <summary>
/// Binance TR spot WebSocket API'sinin borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// Ticker verisi bu borsada yalnizca socket uzerinden anahtarsiz alinabilir; REST
/// tarafinda karsiligi yoktur.
/// </remarks>
internal partial class BinanceTRSocketClientSpotApi : IBinanceTRSocketClientSpotApiShared
{
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
    public IBinanceTRSocketClientSpotApiShared SharedClient => this;

    #region Ticker

    SubscribeTickerOptions ITickerSocketClient.SubscribeTickerOptions { get; }
        = new(BinanceTRExchange.ExchangeName, SharedTickerType.Day24H);

    async Task<WebSocketResult<UpdateSubscription>> ITickerSocketClient.SubscribeToTickerUpdatesAsync(
        SubscribeTickerRequest request,
        Action<DataEvent<SharedSpotTicker>> handler,
        CancellationToken ct)
    {
        var validationError = ((ITickerSocketClient)this).SubscribeTickerOptions.ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);

        return await SubscribeToTickerUpdatesAsync(symbol, update => handler(Convert(update,
            new SharedSpotTicker(
                request.Symbol,
                update.Data.Symbol,
                update.Data.LastPrice,
                update.Data.HighPrice,
                update.Data.LowPrice,
                new SharedOrderQuantity(update.Data.Volume, update.Data.QuoteVolume),
                update.Data.ChangePercentage))), ct).ConfigureAwait(false);
    }

    #endregion

    #region Trade

    SubscribeTradeOptions ITradeSocketClient.SubscribeTradeOptions { get; }
        = new(BinanceTRExchange.ExchangeName, false);

    async Task<WebSocketResult<UpdateSubscription>> ITradeSocketClient.SubscribeToTradeUpdatesAsync(
        SubscribeTradeRequest request,
        Action<DataEvent<SharedTrade[]>> handler,
        CancellationToken ct)
    {
        var validationError = ((ITradeSocketClient)this).SubscribeTradeOptions.ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);

        // Akis tek tek islem gonderir; shared sozlesmesi dizi bekledigi icin tek ogeli
        // bir dizi olarak aktarilir.
        return await SubscribeToTradeUpdatesAsync(symbol, update => handler(Convert(update,
            new[]
            {
                new SharedTrade(
                    request.Symbol,
                    update.Data.Symbol,
                    new SharedOrderQuantity(update.Data.Quantity),
                    update.Data.Price,
                    update.Data.Timestamp)
                {
                    Side = update.Data.Side == Enums.OrderSide.Buy
                        ? SharedOrderSide.Buy
                        : SharedOrderSide.Sell
                }
            })), ct).ConfigureAwait(false);
    }

    #endregion

    #region Order book

    // Akis sabit kademe sayilari sunar; borsa 5, 10 ve 20 destekler.
    SubscribeOrderBookOptions IOrderBookSocketClient.SubscribeOrderBookOptions { get; }
        = new(BinanceTRExchange.ExchangeName, false, [5, 10, 20]);

    async Task<WebSocketResult<UpdateSubscription>> IOrderBookSocketClient.SubscribeToOrderBookUpdatesAsync(
        SubscribeOrderBookRequest request,
        Action<DataEvent<SharedOrderBook>> handler,
        CancellationToken ct)
    {
        var validationError = ((IOrderBookSocketClient)this).SubscribeOrderBookOptions.ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);
        var levels = request.Limit ?? 20;

        return await SubscribeToPartialOrderBookUpdatesAsync(symbol, levels, update => handler(Convert(update,
            new SharedOrderBook(
                SharedQuantityType.BaseAsset,
                update.Data.Asks.Cast<ISymbolOrderBookEntry>().ToArray(),
                update.Data.Bids.Cast<ISymbolOrderBookEntry>().ToArray()))), ct).ConfigureAwait(false);
    }

    #endregion

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
