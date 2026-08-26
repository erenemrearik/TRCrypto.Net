using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using TRCrypto.BtcTurk.Clients.MessageHandlers;
using TRCrypto.BtcTurk.Objects.Internal;

namespace TRCrypto.BtcTurk.Objects.Sockets;

/// <summary>
/// Abonelik acma ya da kapama istegi.
/// </summary>
/// <remarks>
/// Sunucu yanit olarak <c>[100, {"ok": true, "message": "join|kanal:olay"}]</c> gonderir.
/// Ayni anda birden fazla abonelik istegi gonderilebildiginden yanitlar, mesaj alanindaki
/// <c>kanal:olay</c> ekine gore ilgili istekle eslestirilir.
/// </remarks>
internal class BtcTurkSubscribeQuery : Query<BtcTurkSocketUpdate<BtcTurkSocketResult>>
{
    public BtcTurkSubscribeQuery(string channel, string symbol, bool join)
        : base(
            new BtcTurkSocketRequest { Channel = channel, Event = symbol, Join = join },
            authenticated: false)
    {
        // Onay mesajindaki bicim: "join|ticker:BTCTRY" ya da "leave|ticker:BTCTRY"
        var topicFilter = $"{channel}:{symbol}";

        MessageRouter = MessageRouter.CreateForQuery<BtcTurkSocketUpdate<BtcTurkSocketResult>>(
            BtcTurkSocketMessageHandler.TypeIdentifier(BtcTurkSocketMessageType.Result),
            topicFilter,
            HandleResponse);
    }

    private static CallResult<BtcTurkSocketUpdate<BtcTurkSocketResult>> HandleResponse(
        SocketConnection connection,
        DateTime receiveTime,
        string? originalData,
        BtcTurkSocketUpdate<BtcTurkSocketResult> message)
    {
        if (message.Data.Ok)
            return CallResult.Ok(message, originalData);

        // Sunucu istegi reddettiyse abonelik kurulmus sayilmamalidir; aksi halde
        // cagiran taraf hic gelmeyecek guncellemeleri bekler.
        var reason = message.Data.Message ?? "Abonelik istegi reddedildi.";
        return CallResult.Fail<BtcTurkSocketUpdate<BtcTurkSocketResult>>(
            new ServerError(new ErrorInfo(ErrorType.InvalidOperation, false, reason)),
            originalData);
    }
}
