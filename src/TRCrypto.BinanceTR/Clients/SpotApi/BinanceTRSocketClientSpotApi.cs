using System.Globalization;
using System.Net.WebSockets;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Clients.MessageHandlers;
using TRCrypto.BinanceTR.Enums;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;
using TRCrypto.BinanceTR.Objects.Internal;
using TRCrypto.BinanceTR.Objects.Models.Socket;
using TRCrypto.BinanceTR.Objects.Options;
using TRCrypto.BinanceTR.Objects.Sockets;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <inheritdoc cref="IBinanceTRSocketClientSpotApi" />
internal partial class BinanceTRSocketClientSpotApi
    : SocketApiClient<BinanceTREnvironment, BinanceTRAuthenticationProvider, BinanceTRCredentials>,
      IBinanceTRSocketClientSpotApi
{
    /// <inheritdoc />
    public new BinanceTRSocketOptions ClientOptions => (BinanceTRSocketOptions)base.ClientOptions;

    protected override ErrorMapping ErrorMapping => BinanceTRErrors.Mapping;

    internal BinanceTRSocketClientSpotApi(ILoggerFactory? loggerFactory, BinanceTRSocketOptions options)
        : base(
            loggerFactory,
            BinanceTRExchange.ExchangeName,
            options.Environment.SocketBaseAddress,
            options,
            options.SpotOptions)
    {
    }

    /// <inheritdoc />
    protected override IMessageSerializer CreateSerializer()
        => new SystemTextJsonMessageSerializer(BinanceTRJsonOptions.Default);

    /// <inheritdoc />
    public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType)
        => new BinanceTRSocketMessageHandler();

    /// <inheritdoc />
    public override string FormatSymbol(
        string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
        => BinanceTRExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverTime);

    /// <inheritdoc />
    protected override BinanceTRAuthenticationProvider CreateAuthenticationProvider(
        BinanceTRCredentials credentials)
        => new(credentials);

    /// <inheritdoc />
    public Task<WebSocketResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(
        string symbol,
        Action<DataEvent<BinanceTRStreamTicker>> onMessage,
        CancellationToken ct = default)
        => SubscribeToStreamAsync(symbol, "ticker", "24hrTicker", onMessage, ct);

    /// <inheritdoc />
    public Task<WebSocketResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(
        string symbol,
        Action<DataEvent<BinanceTRStreamTrade>> onMessage,
        CancellationToken ct = default)
        => SubscribeToStreamAsync(symbol, "trade", "trade", onMessage, ct);

    /// <inheritdoc />
    public Task<WebSocketResult<UpdateSubscription>> SubscribeToAggregatedTradeUpdatesAsync(
        string symbol,
        Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage,
        CancellationToken ct = default)
        => SubscribeToStreamAsync(symbol, "aggTrade", "aggTrade", onMessage, ct);

    /// <inheritdoc />
    public Task<WebSocketResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(
        string symbol,
        Action<DataEvent<BinanceTRStreamOrderBookUpdate>> onMessage,
        CancellationToken ct = default)
        => SubscribeToStreamAsync(symbol, "depth", "depthUpdate", onMessage, ct);

    /// <inheritdoc />
    public Task<WebSocketResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(
        string symbol,
        int levels,
        Action<DataEvent<BinanceTRStreamOrderBook>> onMessage,
        CancellationToken ct = default)
    {
        if (levels is not (5 or 10 or 20))
        {
            throw new ArgumentOutOfRangeException(
                nameof(levels), levels, "Kademe sayisi 5, 10 ya da 20 olmalidir.");
        }

        return SubscribeToStreamAsync(
            symbol,
            "depth" + levels.ToString(CultureInfo.InvariantCulture),
            BinanceTRSocketMessageHandler.SnapshotIdentifier,
            onMessage,
            ct);
    }

    /// <inheritdoc />
    public Task<WebSocketResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(
        string symbol,
        KlineInterval interval,
        Action<DataEvent<BinanceTRStreamKlineUpdate>> onMessage,
        CancellationToken ct = default)
        => SubscribeToStreamAsync(symbol, "kline_" + ToIntervalCode(interval), "kline", onMessage, ct);

    private async Task<WebSocketResult<UpdateSubscription>> SubscribeToStreamAsync<T>(
        string symbol,
        string channel,
        string typeIdentifier,
        Action<DataEvent<T>> onMessage,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Sembol bos olamaz.", nameof(symbol));

        var streamSymbol = ToStreamSymbol(symbol);
        var stream = $"{streamSymbol}@{channel}";

        var subscription = new BinanceTRStreamSubscription<T>(
            _logger, stream, typeIdentifier, symbol, onMessage);

        // Akis, baglanti adresinin bir parcasidir; her abonelik kendi baglantisini kurar.
        var url = $"{ClientOptions.Environment.SocketBaseAddress}/ws/{stream}";
        return await SubscribeAsync(url, subscription, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Bir sembolu akis adinda kullanilan bicime cevirir.
    /// </summary>
    /// <remarks>
    /// Akislar sembolu <b>kucuk harf ve alt cizgisiz</b> bekler. Alt cizgili ya da buyuk
    /// harfli bir ad hata uretmez; baglanti kurulur ama hicbir mesaj gelmez.
    /// </remarks>
    internal static string ToStreamSymbol(string symbol)
        => symbol.Replace("_", string.Empty).ToLowerInvariant();

    private static string ToIntervalCode(KlineInterval interval)
        => interval switch
        {
            KlineInterval.OneMinute => "1m",
            KlineInterval.ThreeMinutes => "3m",
            KlineInterval.FiveMinutes => "5m",
            KlineInterval.FifteenMinutes => "15m",
            KlineInterval.ThirtyMinutes => "30m",
            KlineInterval.OneHour => "1h",
            KlineInterval.FourHours => "4h",
            KlineInterval.OneDay => "1d",
            KlineInterval.OneWeek => "1w",
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Desteklenmeyen aralik.")
        };
}
