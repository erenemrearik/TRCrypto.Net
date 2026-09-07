using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using TRCrypto.CoinTR.Objects.Models.Socket;

namespace TRCrypto.CoinTR.Objects.Sockets;

/// <summary>
/// Tek bir kanal ve parite ciftine yapilan abonelik.
/// </summary>
/// <remarks>
/// Baglanti kurulduktan sonra bir abonelik mesaji gonderilir; akis adres yoluna
/// yazilmaz. Ayni baglanti uzerinden birden fazla kanala abone olunabilir, bu yuzden
/// yonlendirme kimligi kanal ve parite birlesiminden turetilir.
/// </remarks>
/// <typeparam name="T">Gelen guncellemenin govde tipi.</typeparam>
internal class CoinTRSubscription<T> : Subscription
{
    private readonly string _channel;
    private readonly string _symbol;
    private readonly Action<DataEvent<T>> _handler;

    /// <param name="logger">Gunluk.</param>
    /// <param name="channel">Kanal adi, ornegin <c>ticker</c>.</param>
    /// <param name="symbol">Native parite adi.</param>
    /// <param name="handler">Guncelleme geldiginde cagrilacak islev.</param>
    public CoinTRSubscription(
        ILogger logger,
        string channel,
        string symbol,
        Action<DataEvent<T>> handler)
        : base(logger, authenticated: false)
    {
        _channel = channel;
        _symbol = symbol;
        _handler = handler;

        Topic = channel + ":" + symbol;

        // Kimlik mesajin kendisinden turetilir. Istemcide tutulan bir alan kullanmak,
        // ayni baglantida birden fazla abonelik oldugunda sonuncusunun oncekileri
        // ezmesine yol acardi.
        MessageRouter = MessageRouter.CreateForEvent<CoinTRStreamUpdate<T>>(
            channel, [symbol], HandleUpdate);
    }

    /// <inheritdoc />
    protected override Query GetSubQuery(SocketConnection connection)
        => new CoinTRSubscribeQuery(_channel, _symbol, subscribe: true);

    /// <inheritdoc />
    protected override Query GetUnsubQuery(SocketConnection connection)
        => new CoinTRSubscribeQuery(_channel, _symbol, subscribe: false);

    private CallResult HandleUpdate(
        SocketConnection connection,
        DateTime receiveTime,
        string? originalData,
        CoinTRStreamUpdate<T> message)
    {
        // Govde her zaman dizidir, tek ogeli olsa bile.
        foreach (var item in message.Data)
        {
            _handler(new DataEvent<T>(
                CoinTRExchange.ExchangeName,
                item,
                receiveTime,
                originalData!)
                .WithSymbol(_symbol));
        }

        return CallResult.Ok();
    }
}

/// <summary>Abonelik acma ya da kapama istegi.</summary>
/// <remarks>
/// Sunucu yanit olarak <c>{"event":"subscribe","arg":{...}}</c> gonderir. Yanit hangi
/// abonelige ait oldugunu <c>arg</c> icindeki kanal ve parite ile bildirir.
/// </remarks>
internal class CoinTRSubscribeQuery : Query<CoinTRSubscribeResponse>
{
    public CoinTRSubscribeQuery(string channel, string symbol, bool subscribe)
        : base(
            new CoinTRSubscribeRequest
            {
                Operation = subscribe ? "subscribe" : "unsubscribe",
                Arguments =
                [
                    new CoinTRStreamArgument
                    {
                        InstrumentType = CoinTRSubscribeRequest.SpotInstrumentType,
                        Channel = channel,
                        InstrumentId = symbol
                    }
                ]
            },
            authenticated: false)
    {
        MessageRouter = MessageRouter.CreateForQuery<CoinTRSubscribeResponse>(
            subscribe ? "subscribe" : "unsubscribe",
            channel + ":" + symbol,
            HandleResponse);
    }

    private static CallResult<CoinTRSubscribeResponse> HandleResponse(
        SocketConnection connection,
        DateTime receiveTime,
        string? originalData,
        CoinTRSubscribeResponse message)
        => CallResult.Ok(message, originalData);
}

/// <summary>Abonelik istegi govdesi.</summary>
public record CoinTRSubscribeRequest
{
    /// <summary>Spot enstruman turu.</summary>
    public const string SpotInstrumentType = "SPOT";

    /// <summary>["<c>op</c>"] Islem; <c>subscribe</c> ya da <c>unsubscribe</c>.</summary>
    [JsonPropertyName("op")]
    public string Operation { get; init; } = string.Empty;

    /// <summary>["<c>args</c>"] Abone olunacak kanallar.</summary>
    [JsonPropertyName("args")]
    public List<CoinTRStreamArgument> Arguments { get; init; } = [];
}

/// <summary>Abonelik isteginin yaniti.</summary>
[SerializationModel]
public record CoinTRSubscribeResponse
{
    /// <summary>["<c>event</c>"] Olay adi; <c>subscribe</c> ya da <c>error</c>.</summary>
    [JsonPropertyName("event")]
    public string Event { get; init; } = string.Empty;

    /// <summary>["<c>arg</c>"] Istegin ait oldugu abonelik.</summary>
    [JsonPropertyName("arg")]
    public CoinTRStreamArgument? Argument { get; init; }

    /// <summary>["<c>msg</c>"] Hata durumunda aciklama.</summary>
    [JsonPropertyName("msg")]
    public string? Message { get; init; }
}
