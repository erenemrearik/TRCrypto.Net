using System.Text.Json;

namespace TRCrypto.CoinTR.Objects.Internal;

/// <summary>Zarf tipinden bagimsiz olarak durum bilgisine erisim saglar.</summary>
internal interface ICoinTRResponse
{
    /// <summary>Borsa durum kodu; <c>"00000"</c> basarili demektir.</summary>
    string? Code { get; }

    /// <summary>Durum aciklamasi.</summary>
    string? Message { get; }

    /// <summary>Istegin basarili olup olmadigi.</summary>
    bool Success { get; }
}

/// <summary>
/// CoinTR API'sinin tum yanitlarini saran zarf.
/// </summary>
/// <remarks>
/// <para>
/// Basari <c>code</c> alaninin <c>"00000"</c> olmasiyla anlasilir ve bu deger bir
/// <b>metindir</b>, sayi degil. Alani <c>int</c> olarak okumak ayristirma hatasi verir.
/// </para>
/// <para>
/// Bu, projedeki dorduncu farkli zarf bicimidir: BtcTurk mantiksal bir <c>success</c>
/// alani, Binance TR sayisal bir <c>code</c>, Paribu ise hic zarf kullanmaz.
/// </para>
/// </remarks>
/// <typeparam name="T">Zarfin tasidigi veri tipi.</typeparam>
[SerializationModel]
internal record CoinTRResponse<T> : ICoinTRResponse
{
    /// <summary>Basarili istegin durum kodu.</summary>
    public const string SuccessCode = "00000";

    /// <summary>["<c>code</c>"] Durum kodu; <c>"00000"</c> basarili demektir.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>["<c>msg</c>"] Durum aciklamasi.</summary>
    [JsonPropertyName("msg")]
    public string? Message { get; init; }

    /// <summary>["<c>requestTime</c>"] Sunucunun istegi isledigi an.</summary>
    /// <remarks>Ayri bir sunucu saati ucu vardir; bu alan da ayni saati tasir.</remarks>
    [JsonPropertyName("requestTime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime RequestTime { get; init; }

    /// <summary>["<c>data</c>"] Yanit govdesi; hata durumunda <c>null</c>.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool Success => string.Equals(Code, SuccessCode, StringComparison.Ordinal);
}

/// <summary>CoinTR yanitlarini ayristirirken kullanilan ortak serilestirme ayarlari.</summary>
/// <remarks>
/// <para>
/// Alan eslesmesi <b>harf buyuklugune duyarlidir</b>. Akis govdelerinde tek harfli alanlar
/// yoktur ama <c>ts</c> ve <c>TS</c> gibi ayrimlar korunur; duyarsiz eslesme ileride
/// sessiz bir hataya yol acardi.
/// </para>
/// <para>
/// Bu borsada <b>butun sayisal degerler metin olarak gelir</b>: fiyat, miktar, hacim,
/// ondalik basamak sayisi ve komisyon orani dahil. Metinden sayi okuma acilmazsa her
/// yanit ayristirma hatasiyla duser.
/// </para>
/// </remarks>
internal static class CoinTRJsonOptions
{
    /// <summary>Varsayilan ayarlar.</summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNameCaseInsensitive = false,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };
}
