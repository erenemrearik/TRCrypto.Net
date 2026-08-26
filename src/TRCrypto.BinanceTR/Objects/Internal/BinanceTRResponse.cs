using System.Text.Json;

namespace TRCrypto.BinanceTR.Objects.Internal;

/// <summary>Zarf tipinden bagimsiz olarak durum bilgisine erisim saglar.</summary>
internal interface IBinanceTRResponse
{
    /// <summary>Borsa durum kodu; <c>0</c> basarili demektir.</summary>
    int Code { get; }

    /// <summary>Durum aciklamasi.</summary>
    string? Message { get; }

    /// <summary>Istegin basarili olup olmadigi.</summary>
    bool Success { get; }
}

/// <summary>
/// Binance TR API'sinin tum yanitlarini saran zarf.
/// </summary>
/// <remarks>
/// <para>
/// Yapisi BtcTurk zarfindan tamamen farklidir: burada bir <c>success</c> alani yoktur,
/// basari <c>code == 0</c> ile anlasilir; mesaj alani <c>msg</c> adini tasir ve zarfin
/// kendisinde bir <c>timestamp</c> bulunur.
/// </para>
/// <para>
/// Hatalar da HTTP 200 icinde doner; bu nedenle her yanitta <see cref="Success"/>
/// kontrol edilmeli ve basarisiz durum yuzeye cikarilmalidir.
/// </para>
/// </remarks>
/// <typeparam name="T">Zarfin tasidigi veri tipi.</typeparam>
[SerializationModel]
internal record BinanceTRResponse<T> : IBinanceTRResponse
{
    /// <summary>["<c>code</c>"] Durum kodu; <c>0</c> basarili demektir.</summary>
    [JsonPropertyName("code")]
    public int Code { get; init; }

    /// <summary>["<c>msg</c>"] Durum aciklamasi.</summary>
    [JsonPropertyName("msg")]
    public string? Message { get; init; }

    /// <summary>["<c>timestamp</c>"] Yanitin uretildigi an (UTC).</summary>
    /// <remarks>Sunucu saati bu alandan okunur; ayri bir sunucu saati ucu yoktur.</remarks>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>data</c>"] Yanit govdesi; hata durumunda ve bazi uclarda <c>null</c>.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool Success => Code == 0;
}

/// <summary>Binance TR yanitlarini ayristirirken kullanilan ortak serilestirme ayarlari.</summary>
internal static class BinanceTRJsonOptions
{
    /// <summary>Varsayilan ayarlar.</summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };
}
