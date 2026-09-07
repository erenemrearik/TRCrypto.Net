using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using TRCrypto.BinanceTR.Clients.MessageHandlers;

namespace TRCrypto.BinanceTR.Objects.Sockets;

/// <summary>
/// Kullanici akisina yapilan abonelik.
/// </summary>
/// <remarks>
/// Piyasa akislarindan iki yonden ayrilir: baglanti ayri bir sunucuya kurulur ve akis
/// adres yoluna yazilmak yerine, baglanti kurulduktan sonra gonderilen bir mesajla
/// secilir. Bu yuzden bu abonelik gercek bir abonelik sorgusu tasir.
/// </remarks>
/// <typeparam name="T">Gelen guncellemenin tipi.</typeparam>
internal class BinanceTRUserStreamSubscription<T> : Subscription
{
    private readonly string _listenToken;
    private readonly Action<DataEvent<T>> _handler;

    /// <param name="logger">Gunluk.</param>
    /// <param name="listenToken">
    /// <c>/open/v1/user-listen-token</c> ucundan alinan token.
    /// </param>
    /// <param name="eventType">
    /// Bu abonelige yonlendirilecek olay turu; <c>outboundAccountPosition</c> ya da
    /// <c>executionReport</c>.
    /// </param>
    /// <param name="handler">Guncelleme geldiginde cagrilacak islev.</param>
    public BinanceTRUserStreamSubscription(
        ILogger logger,
        string listenToken,
        string eventType,
        Action<DataEvent<T>> handler)
        : base(logger, authenticated: true)
    {
        _listenToken = listenToken;
        _handler = handler;

        Topic = eventType;
        MessageRouter = MessageRouter.CreateForEvent<T>(eventType, HandleUpdate);
    }

    /// <inheritdoc />
    protected override Query? GetSubQuery(SocketConnection connection)
        => new BinanceTRUserStreamQuery(_listenToken);

    /// <inheritdoc />
    /// <remarks>
    /// Borsa abonelikten cikma icin bir yontem belgelemiyor; baglanti kapatilarak
    /// cikilir. Uydurma bir yontem adi gondermek sessizce yok sayilirdi.
    /// </remarks>
    protected override Query? GetUnsubQuery(SocketConnection connection) => null;

    private CallResult HandleUpdate(
        SocketConnection connection,
        DateTime receiveTime,
        string? originalData,
        T message)
    {
        _handler(new DataEvent<T>(
            BinanceTRExchange.ExchangeName,
            message,
            receiveTime,
            originalData!));

        return CallResult.Ok();
    }
}

/// <summary>
/// Kullanici akisina abone olma sorgusu.
/// </summary>
/// <remarks>
/// Yanit <c>{ "subscriptionId": …, "expirationTime": … }</c> biciminde gelir ve olay
/// turu tasimadigi icin ayri bir yonlendirme kimligiyle taninir.
/// </remarks>
internal class BinanceTRUserStreamQuery : Query<BinanceTRListenTokenResponse>
{
    public BinanceTRUserStreamQuery(string listenToken)
        : base(new BinanceTRListenTokenRequest(listenToken), authenticated: true)
    {
        MessageRouter = MessageRouter.CreateForQuery<BinanceTRListenTokenResponse>(
            BinanceTRSocketMessageHandler.SubscriptionIdentifier,
            HandleResponse);
    }

    private static CallResult<BinanceTRListenTokenResponse> HandleResponse(
        SocketConnection connection,
        DateTime receiveTime,
        string? originalData,
        BinanceTRListenTokenResponse message)
        => CallResult.Ok(message, originalData);
}
