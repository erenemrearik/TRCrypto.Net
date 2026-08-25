using System.Text.Json;

namespace TRCrypto.BtcTurk.Objects.Internal;

/// <summary>BtcTurk WebSocket mesaj kodlari.</summary>
internal static class BtcTurkSocketMessageType
{
    /// <summary>Sunucudan gelen sonuc/onay mesaji.</summary>
    public const int Result = 100;

    /// <summary>Abonelik acma/kapama istegi.</summary>
    public const int Subscription = 151;

    /// <summary>Tum pariteler icin ticker.</summary>
    public const int TickerAll = 401;

    /// <summary>Tek parite icin ticker.</summary>
    public const int TickerPair = 402;

    /// <summary>Islem listesi.</summary>
    /// <remarks>Resmi model listesinde yer almaz; canli akista bu kod gelir.</remarks>
    public const int TradeList = 421;

    /// <summary>Tek islem.</summary>
    public const int TradeSingle = 422;

    /// <summary>Emir defteri tam goruntusu.</summary>
    public const int OrderBookFull = 431;

    /// <summary>Emir defteri farki.</summary>
    public const int OrderBookDifference = 432;

    /// <summary>Baglanti aninda gonderilen surum bilgisi.</summary>
    /// <remarks>Resmi dokumantasyonda yer almaz.</remarks>
    public const int Version = 991;
}

/// <summary>BtcTurk WebSocket kanal adlari.</summary>
internal static class BtcTurkSocketChannel
{
    public const string Ticker = "ticker";
    public const string Trade = "trade";
    public const string OrderBook = "orderbook";
}

/// <summary>
/// Abonelik acma/kapama istegi.
/// </summary>
/// <remarks>
/// Kaynak bicimi iki elemanli bir dizidir: <c>[151, {...}]</c>. Serilestirme
/// <see cref="BtcTurkSocketRequestConverter"/> tarafindan yapilir.
/// </remarks>
[JsonConverter(typeof(BtcTurkSocketRequestConverter))]
internal record BtcTurkSocketRequest
{
    /// <summary>Mesaj kodu.</summary>
    public int Type { get; init; } = BtcTurkSocketMessageType.Subscription;

    /// <summary>Kanal adi.</summary>
    public string Channel { get; init; } = string.Empty;

    /// <summary>Olay adi; parite adidir.</summary>
    public string Event { get; init; } = string.Empty;

    /// <summary><c>true</c> abone ol, <c>false</c> abonelikten cik.</summary>
    public bool Join { get; init; }
}

/// <summary>
/// <see cref="BtcTurkSocketRequest"/> nesnesini <c>[151, {...}]</c> bicimine yazar.
/// </summary>
internal class BtcTurkSocketRequestConverter : JsonConverter<BtcTurkSocketRequest>
{
    /// <inheritdoc />
    public override BtcTurkSocketRequest Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Bu tip yalnizca istek gondermek icin kullanilir.");

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, BtcTurkSocketRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Type);

        writer.WriteStartObject();
        writer.WriteNumber("type", value.Type);
        writer.WriteString("channel", value.Channel);
        writer.WriteString("event", value.Event);
        writer.WriteBoolean("join", value.Join);
        writer.WriteEndObject();

        writer.WriteEndArray();
    }
}

/// <summary>Sunucunun abonelik istegine verdigi yanit (kod 100).</summary>
[SerializationModel]
internal record BtcTurkSocketResult
{
    /// <summary>["<c>type</c>"] Mesaj kodu.</summary>
    [JsonPropertyName("type")]
    public int Type { get; init; }

    /// <summary>["<c>ok</c>"] Istegin kabul edilip edilmedigi.</summary>
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    /// <summary>
    /// ["<c>message</c>"] Sonuc aciklamasi.
    /// </summary>
    /// <remarks>
    /// Abonelik onaylarinda <c>join|kanal:olay</c> bicimindedir; hangi aboneligin
    /// onaylandigini ayirt etmek icin kullanilir.
    /// </remarks>
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
