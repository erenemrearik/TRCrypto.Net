using TRCrypto.BtcTurk.Converters;

namespace TRCrypto.BtcTurk.Objects.Models;

/// <summary>
/// Hesaba ait gerceklesmis bir islem.
/// </summary>
/// <remarks>
/// Tutarlar isaretlidir: satis islemlerinde <see cref="Quantity"/>, <see cref="Fee"/> ve
/// <see cref="Tax"/> negatif gelir. Isaret, varligin hesaptan ciktigini belirtir; mutlak
/// degere cevirmek toplam hacim ya da komisyon hesabini sessizce yanlis yapar.
/// </remarks>
[SerializationModel]
public record BtcTurkUserTrade
{
    /// <summary>["<c>id</c>"] Islem kimligi.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>["<c>orderId</c>"] Islemin ait oldugu emir kimligi.</summary>
    [JsonPropertyName("orderId")]
    public long OrderId { get; init; }

    /// <summary>["<c>orderClientId</c>"] Cagiran tarafin verdigi emir kimligi.</summary>
    [JsonPropertyName("orderClientId")]
    public string? ClientOrderId { get; init; }

    /// <summary>["<c>timestamp</c>"] Islemin gerceklestigi an (UTC).</summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>numeratorSymbol</c>"] Base varlik, ornegin <c>BTC</c>.</summary>
    [JsonPropertyName("numeratorSymbol")]
    public string NumeratorSymbol { get; init; } = string.Empty;

    /// <summary>["<c>denominatorSymbol</c>"] Quote varlik, ornegin <c>TRY</c>.</summary>
    [JsonPropertyName("denominatorSymbol")]
    public string DenominatorSymbol { get; init; } = string.Empty;

    /// <summary>["<c>orderType</c>"] Islemin yonu.</summary>
    [JsonPropertyName("orderType")]
    public OrderSide Side { get; init; }

    /// <summary>["<c>price</c>"] Islem fiyati.</summary>
    [JsonPropertyName("price")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Price { get; init; }

    /// <summary>["<c>amount</c>"] Islem miktari; satista negatiftir.</summary>
    [JsonPropertyName("amount")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Quantity { get; init; }

    /// <summary>
    /// ["<c>preciseAmount</c>"] Islem miktarinin tam degeri.
    /// </summary>
    /// <remarks>
    /// <see cref="Quantity"/> yuvarlanmis bir gosterimdir; hassasiyet gerektiginde bu alan
    /// kullanilmalidir.
    /// </remarks>
    [JsonPropertyName("preciseAmount")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal PreciseQuantity { get; init; }

    /// <summary>["<c>fee</c>"] Islem komisyonu; kesinti oldugu icin negatiftir.</summary>
    [JsonPropertyName("fee")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Fee { get; init; }

    /// <summary>
    /// ["<c>tax</c>"] Islem uzerinden alinan vergi/kesinti; negatiftir.
    /// </summary>
    /// <remarks>
    /// Bu alan Turkiye'ye ozgudur ve diger borsalarda karsiligi yoktur. Borsadan bagimsiz
    /// islem modelinde temsil edilemedigi icin yalnizca bu native modelde bulunur; vergi
    /// hesabi yapan tuketiciler native API kullanmalidir.
    /// </remarks>
    [JsonPropertyName("tax")]
    [JsonConverter(typeof(BtcTurkDecimalConverter))]
    public decimal Tax { get; init; }
}
