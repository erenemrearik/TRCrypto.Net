using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;
using TRCrypto.CoinTR.Objects.Internal;

namespace TRCrypto.CoinTR.Clients.MessageHandlers;

/// <summary>
/// Gelen akis mesajlarini ilgili abonelige yonlendirir.
/// </summary>
/// <remarks>
/// <para>
/// Veri mesajlari kanal adiyla, abonelik onaylari ise olay adiyla taninir. Iki durumda da
/// kimlik <b>mesajin kendisinden</b> turetilir; istemcide tutulan bir alan kullanmak, ayni
/// baglantida birden fazla abonelik oldugunda sonuncusunun oncekileri ezmesine yol acardi.
/// </para>
/// <para>
/// Sunucu canlilik icin duz metin <c>pong</c> cercevesi gonderir. Bu JSON degildir ve
/// ayristirilmaya calisilirsa her seferinde hata uretir; yonlendirmeden once elenir.
/// </para>
/// </remarks>
internal class CoinTRSocketMessageHandler : JsonSocketMessageHandler
{
    /// <summary>Sunucunun gonderdigi duz metin canlilik cercevesi.</summary>
    internal const string PongFrame = "pong";

    /// <inheritdoc />
    public override JsonSerializerOptions Options { get; } = CoinTRJsonOptions.Default;

    public CoinTRSocketMessageHandler()
    {
        // Ayni kanala birden fazla parite icin abone olunabilir; mesajlar parite adiyla
        // ilgili abonelige yonlendirilir.
        AddTopicMapping<Objects.Models.Socket.CoinTRStreamUpdate<Objects.Models.Socket.CoinTRStreamTicker>>(
            x => x.Argument.InstrumentId);
        AddTopicMapping<Objects.Models.Socket.CoinTRStreamUpdate<Objects.Models.Socket.CoinTRStreamOrderBook>>(
            x => x.Argument.InstrumentId);
        AddTopicMapping<Objects.Models.Socket.CoinTRStreamUpdate<Objects.Models.Socket.CoinTRStreamTrade>>(
            x => x.Argument.InstrumentId);

        // Abonelik onayi hangi istege ait oldugunu arg icinde tasir.
        AddTopicMapping<Objects.Sockets.CoinTRSubscribeResponse>(
            x => x.Argument == null
                ? string.Empty
                : x.Argument.Channel + ":" + x.Argument.InstrumentId);
    }

    /// <inheritdoc />
    public override string? GetTypeIdentifier(
        ReadOnlySpan<byte> data, WebSocketMessageType? messageType)
    {
        // Duz metin canlilik cercevesi JSON degildir; ayristiriciya hic ulasmamalidir.
        if (IsPongFrame(data))
            return null;

        return base.GetTypeIdentifier(data, messageType);
    }

    /// <summary>Canlilik cercevesinin UTF-8 karsiligi.</summary>
    private static readonly byte[] _pongFrameBytes = Encoding.UTF8.GetBytes(PongFrame);

    /// <summary>Gelen cercevenin duz metin <c>pong</c> olup olmadigini soyler.</summary>
    internal static bool IsPongFrame(ReadOnlySpan<byte> data)
        => data.SequenceEqual(_pongFrameBytes);

    /// <inheritdoc />
    protected override MessageTypeDefinition[] TypeEvaluators { get; } =
    [
        // Abonelik onaylari once denenir. Onay mesaji hem <c>event</c> hem de
        // <c>arg.channel</c> tasir; kanal once bakilirsa onay veri aboneligine gider ve
        // istegi bekleyen sorgu hicbir zaman yanit almaz.
        new MessageTypeDefinition
        {
            ForceIfFound = true,
            Fields = [new PropertyFieldReference("event")],
            TypeIdentifierCallback = x => x.FieldValue("event")!
        },

        // Veri mesajlari kanal adiyla yonlendirilir. Kanal adi kok nesnede degil
        // <c>arg</c> nesnesinin icindedir; bu yuzden arama derinligi belirtilir.
        new MessageTypeDefinition
        {
            Fields = [new PropertyFieldReference("channel") { Depth = 2 }],
            TypeIdentifierCallback = x => x.FieldValue("channel")!
        }
    ];
}
