namespace TRCrypto.BinanceTR.Objects.Models;

/// <summary>
/// Toplu emir iptalinin sonucu.
/// </summary>
/// <remarks>
/// Yanit tek bir basari degeri tasimaz: bazi emirler iptal edilirken bazilari edilemez.
/// Sonucu bir butun olarak basarili saymak, iptal edilmemis emirlerin gozden kacmasina
/// ve acik pozisyonun sanilandan buyuk kalmasina yol acar.
/// </remarks>
[SerializationModel]
public record BinanceTRBatchCancelResult
{
    /// <summary>["<c>succeedIds</c>"] Iptal edilen emirlerin kimlikleri.</summary>
    [JsonPropertyName("succeedIds")]
    public List<long> SucceededOrderIds { get; init; } = [];

    /// <summary>["<c>failedIds</c>"] Iptal edilemeyen emirler ve nedenleri.</summary>
    [JsonPropertyName("failedIds")]
    public List<BinanceTRBatchCancelFailure> FailedOrders { get; init; } = [];
}

/// <summary>Toplu iptalde basarisiz olan tek bir emir.</summary>
[SerializationModel]
public record BinanceTRBatchCancelFailure
{
    /// <summary>["<c>orderId</c>"] Iptal edilemeyen emrin kimligi.</summary>
    [JsonPropertyName("orderId")]
    public long OrderId { get; init; }

    /// <summary>["<c>code</c>"] Basarisizlik kodu.</summary>
    [JsonPropertyName("code")]
    public int Code { get; init; }

    /// <summary>["<c>message</c>"] Basarisizlik aciklamasi.</summary>
    /// <remarks>
    /// Bu ic nesne <c>message</c> adini kullanir; zarfin kendisi cogu ucta <c>msg</c>
    /// kullanir. Iki ad ayni yanitin icinde bir arada bulunabilir.
    /// </remarks>
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}

/// <summary>
/// Kullanici akisina abone olmak icin kullanilan dinleme tokeni.
/// </summary>
/// <remarks>
/// Token kendiliginden yenilenmez. Suresi dolmadan yenisi alinip yeniden abone olunmali,
/// aksi halde akis hata vermeden durur. Bu yuzden bitis ani modelde tasinir.
/// </remarks>
[SerializationModel]
public record BinanceTRListenToken
{
    /// <summary>["<c>token</c>"] Abonelik mesajinda gonderilecek token.</summary>
    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;

    /// <summary>["<c>expirationTime</c>"] Tokenin gecerliligini yitirecegi an.</summary>
    [JsonPropertyName("expirationTime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime ExpirationTime { get; init; }
}
