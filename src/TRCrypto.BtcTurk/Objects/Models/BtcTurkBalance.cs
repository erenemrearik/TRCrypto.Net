using TRCrypto.BtcTurk.Converters;

namespace TRCrypto.BtcTurk.Objects.Models;

/// <summary>Bir varlik icin hesap bakiyesi.</summary>
/// <remarks>
/// Bu ucun ondalik degerleri virgul ayiricili gelir; donusum
/// <see cref="BtcTurkDecimalConverter"/> tarafindan yapilir.
/// </remarks>
[SerializationModel]
public record BtcTurkBalance
{
    /// <summary>["<c>asset</c>"] Varlik sembolu, ornegin <c>TRY</c>.</summary>
    [JsonPropertyName("asset")]
    public string Asset { get; init; } = string.Empty;

    /// <summary>["<c>assetname</c>"] Varligin tam adi, ornegin <c>Türk Lirası</c>.</summary>
    [JsonPropertyName("assetname")]
    public string? AssetName { get; init; }

    /// <summary>["<c>balance</c>"] Toplam bakiye; serbest ve kilitli tutarin toplami.</summary>
    [JsonPropertyName("balance")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Total { get; init; }

    /// <summary>["<c>free</c>"] Kullanilabilir bakiye.</summary>
    [JsonPropertyName("free")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Available { get; init; }

    /// <summary>["<c>locked</c>"] Acik emirler ve cekim talepleri icin bloke edilmis tutar.</summary>
    [JsonPropertyName("locked")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Locked { get; init; }

    /// <summary>["<c>orderFund</c>"] Acik emirler icin bloke edilmis tutar.</summary>
    [JsonPropertyName("orderFund")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal OrderFund { get; init; }

    /// <summary>["<c>requestFund</c>"] Cekim talepleri icin bloke edilmis tutar.</summary>
    [JsonPropertyName("requestFund")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal RequestFund { get; init; }

    /// <summary>["<c>precision</c>"] Varligin ondalik hassasiyeti.</summary>
    [JsonPropertyName("precision")]
    public int Precision { get; init; }

    /// <summary>["<c>timestamp</c>"] Bakiyenin hesaplandigi an (UTC).</summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }
}
