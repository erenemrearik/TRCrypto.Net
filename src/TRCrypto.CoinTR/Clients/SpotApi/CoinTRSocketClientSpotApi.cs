using System.Globalization;
using System.Net.WebSockets;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.CoinTR.Clients.MessageHandlers;
using TRCrypto.CoinTR.Interfaces.Clients.SpotApi;
using TRCrypto.CoinTR.Objects.Internal;
using TRCrypto.CoinTR.Objects.Models.Socket;
using TRCrypto.CoinTR.Objects.Options;
using TRCrypto.CoinTR.Objects.Sockets;

namespace TRCrypto.CoinTR.Clients.SpotApi;

/// <inheritdoc cref="ICoinTRSocketClientSpotApi" />
internal partial class CoinTRSocketClientSpotApi
    : SocketApiClient<CoinTREnvironment, CoinTRAuthenticationProvider, CoinTRCredentials>,
      ICoinTRSocketClientSpotApi
{
    /// <summary>Ozet fiyat kanali.</summary>
    internal const string TickerChannel = "ticker";

    /// <summary>Islem kanali.</summary>
    internal const string TradeChannel = "trade";

    /// <summary>Borsanin kabul ettigi tam goruntu kademe sayilari.</summary>
    /// <remarks>
    /// Bu degerler kanal adinin parcasidir (<c>books5</c>). Listede olmayan bir deger
    /// icin borsa kanali tanimaz; abonelik onay almaz ve veri gelmez.
    /// </remarks>
    private static readonly int[] _orderBookLevels = [1, 5, 15];

    /// <inheritdoc />
    public new CoinTRSocketOptions ClientOptions => (CoinTRSocketOptions)base.ClientOptions;

    protected override ErrorMapping ErrorMapping => CoinTRErrors.Mapping;

    internal CoinTRSocketClientSpotApi(ILoggerFactory? loggerFactory, CoinTRSocketOptions options)
        : base(
            loggerFactory,
            CoinTRExchange.ExchangeName,
            options.Environment.SocketBaseAddress,
            options,
            options.SpotOptions)
    {
    }

    /// <inheritdoc />
    public ICoinTRSocketClientSpotApiShared SharedClient => this;

    /// <inheritdoc />
    protected override IMessageSerializer CreateSerializer()
        => new SystemTextJsonMessageSerializer(CoinTRJsonOptions.Default);

    /// <inheritdoc />
    public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType)
        => new CoinTRSocketMessageHandler();

    /// <inheritdoc />
    public override string FormatSymbol(
        string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
        => CoinTRExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

    /// <inheritdoc />
    protected override CoinTRAuthenticationProvider CreateAuthenticationProvider(
        CoinTRCredentials credentials)
        => new(credentials);

    /// <inheritdoc />
    public async Task<WebSocketResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(
        string symbol,
        Action<DataEvent<CoinTRStreamTicker>> onMessage,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var subscription = new CoinTRSubscription<CoinTRStreamTicker>(
            _logger, TickerChannel, symbol, onMessage);

        return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebSocketResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(
        string symbol,
        int levels,
        Action<DataEvent<CoinTRStreamOrderBook>> onMessage,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        if (Array.IndexOf(_orderBookLevels, levels) < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(levels),
                levels,
                "Kademe sayisi su degerlerden biri olmalidir: " +
                string.Join(", ", _orderBookLevels) + ".");
        }

        var channel = "books" + levels.ToString(CultureInfo.InvariantCulture);
        var subscription = new CoinTRSubscription<CoinTRStreamOrderBook>(
            _logger, channel, symbol, onMessage);

        return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebSocketResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(
        string symbol,
        Action<DataEvent<CoinTRStreamTrade>> onMessage,
        CancellationToken ct = default)
    {
        ValidateSymbol(symbol);

        var subscription = new CoinTRSubscription<CoinTRStreamTrade>(
            _logger, TradeChannel, symbol, onMessage);

        return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
    }

    private static void ValidateSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));
    }
}
