using System.Globalization;
using System.Text.Json;

namespace TRCrypto.BtcTurk.Objects.Internal;

/// <summary>Zarf tipinden bagimsiz olarak durum bilgisine erisim saglar.</summary>
internal interface IBtcTurkResponse
{
    /// <summary>Istegin is mantigi acisindan basarili olup olmadigi.</summary>
    bool Success { get; }

    /// <summary>Hata mesaji.</summary>
    string? Message { get; }

    /// <summary>BtcTurk durum kodu.</summary>
    string? Code { get; }
}

/// <summary>
/// BtcTurk API'sinin tum yanitlarini saran zarf.
/// </summary>
/// <remarks>
/// BtcTurk is mantigi hatalarini HTTP 200 icinde <c>"success": false</c> olarak dondurur;
/// hata HTTP durum koduna yansimaz. Bu nedenle <see cref="Success"/> her yanitta
/// kontrol edilmeli ve <c>false</c> ise sonuc basarisiz olarak yuzeye cikarilmalidir.
/// </remarks>
/// <typeparam name="T">Zarfin tasidigi veri tipi.</typeparam>
[SerializationModel]
internal record BtcTurkResponse<T> : IBtcTurkResponse
{
    /// <summary>["<c>success</c>"] Istegin is mantigi acisindan basarili olup olmadigi.</summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>["<c>message</c>"] Hata mesaji; basarili yanitlarda bos gelir.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>["<c>code</c>"] Durum kodu.</summary>
    /// <remarks>
    /// Tip uclar arasinda tutarli degildir: cogu uc sayi dondururken emir defteri ucu
    /// <c>"SUCCESS"</c> gibi bir metin dondurur. Bu nedenle deger metin olarak tasinir.
    /// </remarks>
    [JsonPropertyName("code")]
    [JsonConverter(typeof(BtcTurkCodeConverter))]
    public string? Code { get; init; }

    /// <summary>["<c>data</c>"] Yanit govdesi; hata durumunda <c>null</c> olabilir.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }
}

/// <summary>
/// Hem sayi hem metin olarak gelebilen durum kodunu metne cevirir.
/// </summary>
internal class BtcTurkCodeConverter : JsonConverter<string?>
{
    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out var number)
                ? number.ToString(CultureInfo.InvariantCulture)
                : reader.GetDecimal().ToString(CultureInfo.InvariantCulture),
            _ => null
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}
