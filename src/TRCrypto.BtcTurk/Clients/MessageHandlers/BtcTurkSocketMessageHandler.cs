using System.Globalization;
using System.Text.Json;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models.Socket;

namespace TRCrypto.BtcTurk.Clients.MessageHandlers;

/// <summary>
/// Gelen WebSocket mesajlarini tipine ve paritesine gore yonlendirir.
/// </summary>
/// <remarks>
/// BtcTurk mesajlari <c>[tip, govde]</c> bicimindedir. Yonlendirme dizinin ilk
/// elemanindaki sayisal koda gore yapilir; hangi aboneligin ilgilendigi ise govdedeki
/// parite adindan belirlenir.
/// </remarks>
internal class BtcTurkSocketMessageHandler : JsonSocketMessageHandler
{
    /// <inheritdoc />
    public override JsonSerializerOptions Options { get; } = BtcTurkJsonOptions.Default;

    public BtcTurkSocketMessageHandler()
    {
        // Ayni tipte birden fazla parite dinlenebildigi icin mesajlar parite adiyla
        // ilgili aboneliğe yonlendirilir.
        AddTopicMapping<BtcTurkSocketUpdate<BtcTurkSocketTicker>>(x => x.Data.Symbol);
        AddTopicMapping<BtcTurkSocketUpdate<BtcTurkSocketTradeUpdate>>(x => x.Data.Symbol);
        AddTopicMapping<BtcTurkSocketUpdate<BtcTurkSocketOrderBook>>(x => x.Data.Symbol);

        // Abonelik onaylari hangi istegin yanitlandigini yalnizca mesaj metninde tasir.
        AddTopicMapping<BtcTurkSocketUpdate<BtcTurkSocketResult>>(x => ExtractTopic(x.Data.Message));
    }

    /// <summary>
    /// Abonelik onayindaki <c>join|kanal:olay</c> metninden konu adini cikarir.
    /// </summary>
    /// <remarks>
    /// Ayni anda birden fazla abonelik istegi gonderilebildiginden, hangi yanitin hangi
    /// istege ait oldugu yalnizca bu metinden anlasilir.
    /// </remarks>
    internal static string ExtractTopic(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        var separator = message!.IndexOf('|');
        return separator < 0 ? message : message.Substring(separator + 1);
    }

    /// <inheritdoc />
    protected override MessageTypeDefinition[] TypeEvaluators { get; } =
    [
        new MessageTypeDefinition
        {
            // Mesaj kodu kok dizinin ilk elemanidir. Derinlik 1'dir: kok dizinin kendisi
            // 0. seviyededir, elemanlari bir alt seviyede okunur.
            Fields = [new ArrayFieldReference("type", 1, 0)],
            TypeIdentifierCallback = x => x.FieldValue("type")!
        }
    ];

    /// <summary>Bir mesaj kodunu yonlendirme kimligine cevirir.</summary>
    internal static string TypeIdentifier(int messageType)
        => messageType.ToString(CultureInfo.InvariantCulture);
}
