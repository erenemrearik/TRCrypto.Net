using System.Net.WebSockets;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk.Clients.MessageHandlers;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models.Socket;
using TRCrypto.BtcTurk.Objects.Options;
using TRCrypto.BtcTurk.Objects.Sockets;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <inheritdoc cref="IBtcTurkSocketClientSpotApi" />
internal partial class BtcTurkSocketClientSpotApi
    : SocketApiClient<BtcTurkEnvironment, BtcTurkAuthenticationProvider, BtcTurkCredentials>,
      IBtcTurkSocketClientSpotApi
{
    /// <inheritdoc />
    public new BtcTurkSocketOptions ClientOptions => (BtcTurkSocketOptions)base.ClientOptions;

    protected override ErrorMapping ErrorMapping => BtcTurkErrors.Mapping;

    internal BtcTurkSocketClientSpotApi(ILoggerFactory? loggerFactory, BtcTurkSocketOptions options)
        : base(
            loggerFactory,
            BtcTurkExchange.ExchangeName,
            options.Environment.SocketBaseAddress,
            options,
            options.SpotOptions)
    {
        RateLimiter = BtcTurkExchange.RateLimiter.Socket;
    }

    /// <inheritdoc />
    public IBtcTurkSocketClientSpotApiShared SharedClient => this;

    /// <inheritdoc />
    protected override IMessageSerializer CreateSerializer()
        => new SystemTextJsonMessageSerializer(BtcTurkJsonOptions.Default);

    /// <inheritdoc />
    public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType)
        => new BtcTurkSocketMessageHandler();

    /// <inheritdoc />
    public override string FormatSymbol(
        string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
        => BtcTurkExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

    /// <inheritdoc />
    protected override BtcTurkAuthenticationProvider CreateAuthenticationProvider(BtcTurkCredentials credentials)
        => new(credentials);

    /// <inheritdoc />
    public async Task<WebSocketResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(
        string symbol,
        Action<DataEvent<BtcTurkSocketTicker>> onMessage,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var subscription = new BtcTurkSubscription<BtcTurkSocketTicker>(
            _logger,
            BtcTurkSocketChannel.Ticker,
            symbol,
            [BtcTurkSocketMessageType.TickerPair],
            onMessage);

        return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebSocketResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(
        string symbol,
        Action<DataEvent<BtcTurkSocketTradeUpdate>> onMessage,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        // Borsa bu kanalda hem tekil hem toplu islem mesaji gonderebilir; ikisi de
        // ayni abonelige yonlendirilir.
        var subscription = new BtcTurkSubscription<BtcTurkSocketTradeUpdate>(
            _logger,
            BtcTurkSocketChannel.Trade,
            symbol,
            [BtcTurkSocketMessageType.TradeList, BtcTurkSocketMessageType.TradeSingle],
            onMessage);

        return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebSocketResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(
        string symbol,
        Action<DataEvent<BtcTurkSocketOrderBook>> onMessage,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        // Tam goruntu ve fark mesajlari ayni akista gelir.
        var subscription = new BtcTurkSubscription<BtcTurkSocketOrderBook>(
            _logger,
            BtcTurkSocketChannel.OrderBook,
            symbol,
            [BtcTurkSocketMessageType.OrderBookFull, BtcTurkSocketMessageType.OrderBookDifference],
            onMessage);

        return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
    }

    private static void ValidateSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));
    }
}
