namespace TRCrypto.BtcTurk.Objects.Internal;

/// <summary>Zarf tipinden bagimsiz olarak durum bilgisine erisim saglar.</summary>
internal interface IBtcTurkResponse
{
    /// <summary>Istegin is mantigi acisindan basarili olup olmadigi.</summary>
    bool Success { get; }

    /// <summary>Hata mesaji.</summary>
    string? Message { get; }

    /// <summary>BtcTurk hata kodu.</summary>
    int Code { get; }
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

    /// <summary>["<c>message</c>"] Hata mesaji; basarili yanitlarda <c>null</c>.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>["<c>code</c>"] BtcTurk hata kodu; basarili yanitlarda <c>0</c>.</summary>
    [JsonPropertyName("code")]
    public int Code { get; init; }

    /// <summary>["<c>data</c>"] Yanit govdesi; hata durumunda <c>null</c> olabilir.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }
}
