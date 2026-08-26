using System.Text.Json;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;
using TRCrypto.BinanceTR.Objects.Internal;

namespace TRCrypto.BinanceTR.Clients.MessageHandlers;

/// <summary>
/// Gelen akis mesajlarini ilgili abonelige yonlendirir.
/// </summary>
/// <remarks>
/// <para>
/// Yonlendirme kimligi <b>mesajin kendisinden</b> turetilir. Cogu akis olay turunu
/// <c>e</c> alaninda tasir; tasimayan akislar (kismi emir defteri goruntusu, en iyi
/// alis/satis) sabit bir kimlige duser.
/// </para>
/// <para>
/// Kimligin baglantiya degil mesaja bagli olmasi onemlidir: aksi halde ayni istemci
/// uzerinden birden fazla akisa abone olundugunda son abonelik oncekilerin kimligini
/// ezer ve yalnizca sonuncusu mesaj alir.
/// </para>
/// </remarks>
internal class BinanceTRSocketMessageHandler : JsonSocketMessageHandler
{
    /// <summary>Olay turu tasimayan goruntu mesajlari icin kullanilan kimlik.</summary>
    internal const string SnapshotIdentifier = "snapshot";

    public override JsonSerializerOptions Options { get; } = BinanceTRJsonOptions.Default;

    /// <inheritdoc />
    protected override MessageTypeDefinition[] TypeEvaluators { get; } =
    [
        new MessageTypeDefinition
        {
            ForceIfFound = true,
            Fields = [new PropertyFieldReference("e")],
            TypeIdentifierCallback = x => x.FieldValue("e")!
        },

        // Kismi emir defteri goruntusu ve en iyi alis/satis akislari olay turu tasimaz;
        // ikisi de sira numarasi alaniyla taninir.
        new MessageTypeDefinition
        {
            Fields = [new PropertyFieldReference("lastUpdateId")],
            StaticIdentifier = SnapshotIdentifier
        }
    ];
}
