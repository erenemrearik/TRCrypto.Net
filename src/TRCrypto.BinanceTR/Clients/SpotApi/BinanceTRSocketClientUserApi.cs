using System.Net.WebSockets;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Clients.MessageHandlers;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;
using TRCrypto.BinanceTR.Objects.Internal;
using TRCrypto.BinanceTR.Objects.Models.Socket;
using TRCrypto.BinanceTR.Objects.Options;
using TRCrypto.BinanceTR.Objects.Sockets;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <inheritdoc cref="IBinanceTRSocketClientUserApi" />
/// <remarks>
/// Piyasa akisindan ayri bir sunucuya baglanir. Adres, protokol ve abonelik bicimi
/// farklidir; bu yuzden ayri bir istemcidir.
/// </remarks>
internal partial class BinanceTRSocketClientUserApi
    : SocketApiClient<BinanceTREnvironment, BinanceTRAuthenticationProvider, BinanceTRCredentials>,
      IBinanceTRSocketClientUserApi
{
    /// <summary>Hesap bakiyesi guncellemesi olay turu.</summary>
    internal const string AccountUpdateEvent = "outboundAccountPosition";

    /// <summary>Emir guncellemesi olay turu.</summary>
    internal const string OrderUpdateEvent = "executionReport";

    /// <inheritdoc />
    public new BinanceTRSocketOptions ClientOptions => (BinanceTRSocketOptions)base.ClientOptions;

    protected override ErrorMapping ErrorMapping => BinanceTRErrors.Mapping;

    internal BinanceTRSocketClientUserApi(ILoggerFactory? loggerFactory, BinanceTRSocketOptions options)
        : base(
            loggerFactory,
            BinanceTRExchange.ExchangeName,
            options.Environment.UserStreamAddress,
            options,
            options.UserOptions)
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
    public async Task<WebSocketResult<UpdateSubscription>> SubscribeToAccountUpdatesAsync(
        string listenToken,
        Action<DataEvent<BinanceTRStreamAccountUpdate>> onMessage,
        CancellationToken ct = default)
    {
        ValidateToken(listenToken);

        var subscription = new BinanceTRUserStreamSubscription<BinanceTRStreamAccountUpdate>(
            _logger, listenToken, AccountUpdateEvent, onMessage);

        return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebSocketResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(
        string listenToken,
        Action<DataEvent<BinanceTRStreamOrderUpdate>> onMessage,
        CancellationToken ct = default)
    {
        ValidateToken(listenToken);

        var subscription = new BinanceTRUserStreamSubscription<BinanceTRStreamOrderUpdate>(
            _logger, listenToken, OrderUpdateEvent, onMessage);

        return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
    }

    private static void ValidateToken(string listenToken)
    {
        if (string.IsNullOrWhiteSpace(listenToken))
        {
            throw new ArgumentException(
                "Dinleme tokeni bos olamaz. Token SpotApi.Account.GetListenTokenAsync ile alinir.",
                nameof(listenToken));
        }
    }
}
