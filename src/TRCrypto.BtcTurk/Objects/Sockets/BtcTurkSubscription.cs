using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using TRCrypto.BtcTurk.Clients.MessageHandlers;
using TRCrypto.BtcTurk.Objects.Internal;

namespace TRCrypto.BtcTurk.Objects.Sockets;

/// <summary>
/// Tek bir kanal/parite ciftine yapilan abonelik.
/// </summary>
/// <remarks>
/// Yeniden baglanma sonrasi abonelik sorgusu kutuphane tarafindan yeniden gonderilir;
/// bu sinif yalnizca istegi ve yonlendirmeyi tanimlar.
/// </remarks>
/// <typeparam name="T">Gelen guncellemenin govde tipi.</typeparam>
internal class BtcTurkSubscription<T> : Subscription
{
    private readonly string _channel;
    private readonly string _symbol;
    private readonly Action<DataEvent<T>> _handler;

    /// <param name="logger">Gunluk.</param>
    /// <param name="channel">Kanal adi.</param>
    /// <param name="symbol">Native parite adi.</param>
    /// <param name="messageTypes">Bu abonelige yonlendirilecek mesaj kodlari.</param>
    /// <param name="handler">Guncelleme geldiginde cagrilacak islev.</param>
    public BtcTurkSubscription(
        ILogger logger,
        string channel,
        string symbol,
        int[] messageTypes,
        Action<DataEvent<T>> handler)
        : base(logger, authenticated: false)
    {
        _channel = channel;
        _symbol = symbol;
        _handler = handler;

        Topic = $"{channel}:{symbol}";

        // Ayni kanal birden fazla mesaj kodu uretebilir (ornegin emir defteri icin
        // tam goruntu ve fark mesajlari); hepsi ayni abonelige yonlendirilir.
        var identifiers = messageTypes.Select(BtcTurkSocketMessageHandler.TypeIdentifier).ToArray();

        MessageRouter = MessageRouter.CreateForEvent<BtcTurkSocketUpdate<T>>(
            identifiers,
            [symbol],
            HandleUpdate);
    }

    /// <inheritdoc />
    protected override Query GetSubQuery(SocketConnection connection)
        => new BtcTurkSubscribeQuery(_channel, _symbol, join: true);

    /// <inheritdoc />
    protected override Query GetUnsubQuery(SocketConnection connection)
        => new BtcTurkSubscribeQuery(_channel, _symbol, join: false);

    private CallResult HandleUpdate(
        SocketConnection connection,
        DateTime receiveTime,
        string? originalData,
        BtcTurkSocketUpdate<T> message)
    {
        var dataEvent = new DataEvent<T>(
            BtcTurkExchange.ExchangeName,
            message.Data,
            receiveTime,
            originalData!)
            .WithSymbol(_symbol);

        _handler(dataEvent);

        return CallResult.Ok();
    }
}
