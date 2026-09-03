using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <summary>
/// BtcTurk spot WebSocket API'sinin borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// Cagiran taraf <see cref="SharedSymbol"/> kullanir; native sembol bicimi burada uretilir.
/// </remarks>
internal partial class BtcTurkSocketClientSpotApi : IBtcTurkSocketClientSpotApiShared
{
    /// <inheritdoc />
    public TradingMode[] SupportedTradingModes { get; } = [TradingMode.Spot];

    /// <inheritdoc />
    public void SetDefaultExchangeParameter(string key, object value)
        => ExchangeParameters.SetStaticParameter(Exchange, key, value);

    /// <inheritdoc />
    public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

    /// <inheritdoc />
    public SharedClientInfo Discover() => SharedUtils.GetClientInfo(BtcTurkExchange.Metadata, this);

    #region All tickers

    SubscribeTickersOptions ITickersSocketClient.SubscribeAllTickersOptions { get; }
        = new(BtcTurkExchange.ExchangeName, SharedTickerType.Day24H);

    async Task<WebSocketResult<UpdateSubscription>> ITickersSocketClient.SubscribeToAllTickersUpdatesAsync(
        SubscribeAllTickersRequest request,
        Action<DataEvent<SharedSpotTicker[]>> handler,
        CancellationToken ct)
    {
        var validationError = ((ITickersSocketClient)this).SubscribeAllTickersOptions
            .ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        return await SubscribeToAllTickerUpdatesAsync(update => handler(Convert(update,
            update.Data.Items.Select(ToSharedTicker).ToArray())), ct).ConfigureAwait(false);
    }

    /// <summary>Native ticker ogesini borsadan bagimsiz karsiligina cevirir.</summary>
    /// <remarks>
    /// Base ve quote varlik adlari ayri alanlardan okunur; sembol adi ayristirilmaz.
    /// </remarks>
    private static SharedSpotTicker ToSharedTicker(Objects.Models.Socket.BtcTurkSocketTicker ticker)
        => new(
            new SharedSymbol(TradingMode.Spot, ticker.NumeratorSymbol, ticker.DenominatorSymbol),
            ticker.Symbol,
            ticker.LastPrice,
            ticker.HighPrice,
            ticker.LowPrice,
            new SharedOrderQuantity(ticker.Volume),
            ticker.DailyChangePercentage);

    #endregion

    #region Ticker

    SubscribeTickerOptions ITickerSocketClient.SubscribeTickerOptions { get; }
        = new(BtcTurkExchange.ExchangeName, SharedTickerType.Day24H);

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
                new SharedOrderQuantity(update.Data.Volume, update.Data.Volume * update.Data.AveragePrice),
                update.Data.DailyChangePercentage))), ct).ConfigureAwait(false);
    }

    #endregion

    #region Trade

    SubscribeTradeOptions ITradeSocketClient.SubscribeTradeOptions { get; }
        = new(BtcTurkExchange.ExchangeName, false);

    async Task<WebSocketResult<UpdateSubscription>> ITradeSocketClient.SubscribeToTradeUpdatesAsync(
        SubscribeTradeRequest request,
        Action<DataEvent<SharedTrade[]>> handler,
        CancellationToken ct)
    {
        var validationError = ((ITradeSocketClient)this).SubscribeTradeOptions.ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);

        return await SubscribeToTradeUpdatesAsync(symbol, update => handler(Convert(update,
            update.Data.Trades.Select(x => new SharedTrade(
                request.Symbol,
                update.Data.Symbol,
                new SharedOrderQuantity(x.Quantity),
                x.Price,
                x.Timestamp)
            {
                Side = x.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell
            }).ToArray())), ct).ConfigureAwait(false);
    }

    #endregion

    #region Order book

    // Borsa derinlik secenegi sunmaz; tam goruntu gonderilir.
    SubscribeOrderBookOptions IOrderBookSocketClient.SubscribeOrderBookOptions { get; }
        = new(BtcTurkExchange.ExchangeName, false, []);

    async Task<WebSocketResult<UpdateSubscription>> IOrderBookSocketClient.SubscribeToOrderBookUpdatesAsync(
        SubscribeOrderBookRequest request,
        Action<DataEvent<SharedOrderBook>> handler,
        CancellationToken ct)
    {
        var validationError = ((IOrderBookSocketClient)this).SubscribeOrderBookOptions.ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        var symbol = request.Symbol!.GetSymbol(FormatSymbol);

        return await SubscribeToOrderBookUpdatesAsync(symbol, update => handler(Convert(update,
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
    /// <remarks>
    /// Alis zamani ve ham veri korunur; aksi halde tuketici verinin ne kadar eski
    /// oldugunu anlayamaz.
    /// </remarks>
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
