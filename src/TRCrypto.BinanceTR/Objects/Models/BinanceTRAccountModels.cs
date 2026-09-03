using TRCrypto.BinanceTR.Enums;

namespace TRCrypto.BinanceTR.Objects.Models;

/// <summary>Spot hesabin izinleri, komisyon oranlari ve varlik bakiyeleri.</summary>
[SerializationModel]
public record BinanceTRAccount
{
    /// <summary>Piyasa yapici komisyon orani.</summary>
    [JsonPropertyName("makerCommission")]
    public decimal MakerCommission { get; init; }

    /// <summary>Piyasa alici komisyon orani.</summary>
    [JsonPropertyName("takerCommission")]
    public decimal TakerCommission { get; init; }

    /// <summary>Alis tarafi komisyon orani.</summary>
    [JsonPropertyName("buyerCommission")]
    public decimal BuyerCommission { get; init; }

    /// <summary>Satis tarafi komisyon orani.</summary>
    [JsonPropertyName("sellerCommission")]
    public decimal SellerCommission { get; init; }

    /// <summary>Fiat pariteler icin piyasa yapici komisyon orani.</summary>
    [JsonPropertyName("fiatMakerCommission")]
    public decimal FiatMakerCommission { get; init; }

    /// <summary>Fiat pariteler icin piyasa alici komisyon orani.</summary>
    [JsonPropertyName("fiatTakerCommission")]
    public decimal FiatTakerCommission { get; init; }

    /// <summary>Hesap emir verebilir mi?</summary>
    /// <remarks>Borsa bu bayraklari 0 ya da 1 olarak dondurur.</remarks>
    [JsonPropertyName("canTrade")]
    [JsonConverter(typeof(BoolConverter))]
    public bool CanTrade { get; init; }

    /// <summary>Hesap cekim yapabilir mi?</summary>
    [JsonPropertyName("canWithdraw")]
    [JsonConverter(typeof(BoolConverter))]
    public bool CanWithdraw { get; init; }

    /// <summary>Hesap yatirim alabilir mi?</summary>
    [JsonPropertyName("canDeposit")]
    [JsonConverter(typeof(BoolConverter))]
    public bool CanDeposit { get; init; }

    /// <summary>Hesaptaki varliklar.</summary>
    [JsonPropertyName("accountAssets")]
    public List<BinanceTRAccountAsset> Assets { get; init; } = [];
}

/// <summary>Tek bir varligin bakiyesi.</summary>
[SerializationModel]
public record BinanceTRAccountAsset
{
    /// <summary>Varlik adi.</summary>
    [JsonPropertyName("asset")]
    public string Asset { get; init; } = string.Empty;

    /// <summary>Kullanilabilir miktar.</summary>
    [JsonPropertyName("free")]
    public decimal Available { get; init; }

    /// <summary>Acik emirlerde bloke miktar.</summary>
    [JsonPropertyName("locked")]
    public decimal Locked { get; init; }

    /// <summary>Toplam miktar.</summary>
    /// <remarks>Borsa toplami ayri bir alanda vermez; iki bilesenden hesaplanir.</remarks>
    [JsonIgnore]
    public decimal Total => Available + Locked;
}

/// <summary>Yeni bir emrin borsa tarafindan verilen kimligi.</summary>
/// <remarks>
/// Emir olusturma yaniti emrin tam halini dondurmez; ayrinti icin emir sorgulanmalidir.
/// </remarks>
[SerializationModel]
public record BinanceTRPlacedOrder
{
    /// <summary>Emir kimligi.</summary>
    [JsonPropertyName("orderId")]
    public long OrderId { get; init; }

    /// <summary>Emrin olusturuldugu an.</summary>
    [JsonPropertyName("createTime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreateTime { get; init; }
}

/// <summary>Bir emrin ayrintisi.</summary>
[SerializationModel]
public record BinanceTROrder
{
    /// <summary>Emir kimligi.</summary>
    [JsonPropertyName("orderId")]
    public long OrderId { get; init; }

    /// <summary>Bagli emir listesi kimligi; yoksa -1.</summary>
    [JsonPropertyName("orderListId")]
    public long OrderListId { get; init; }

    /// <summary>Cagiran tarafin verdigi kimlik.</summary>
    [JsonPropertyName("clientId")]
    public string? ClientOrderId { get; init; }

    /// <summary>Native parite adi, ornegin <c>BTC_TRY</c>.</summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>Emrin yonu.</summary>
    [JsonPropertyName("side")]
    public OrderSide Side { get; init; }

    /// <summary>Emrin turu.</summary>
    [JsonPropertyName("type")]
    public OrderType Type { get; init; }

    /// <summary>Emrin durumu.</summary>
    [JsonPropertyName("status")]
    public OrderStatus Status { get; init; }

    /// <summary>Limit fiyati; piyasa emirlerinde 0.</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    /// <summary>Emrin acilis miktari.</summary>
    [JsonPropertyName("origQty")]
    public decimal Quantity { get; init; }

    /// <summary>Quote varlik cinsinden acilis tutari.</summary>
    [JsonPropertyName("origQuoteQty")]
    public decimal QuoteQuantity { get; init; }

    /// <summary>Gerceklesen miktar.</summary>
    [JsonPropertyName("executedQty")]
    public decimal QuantityFilled { get; init; }

    /// <summary>Gerceklesen ortalama fiyat.</summary>
    [JsonPropertyName("executedPrice")]
    public decimal AverageFillPrice { get; init; }

    /// <summary>Quote varlik cinsinden gerceklesen tutar.</summary>
    [JsonPropertyName("executedQuoteQty")]
    public decimal QuoteQuantityFilled { get; init; }

    /// <summary>Emrin gecerlilik suresi.</summary>
    [JsonPropertyName("timeInForce")]
    public TimeInForce? TimeInForce { get; init; }

    /// <summary>Tetikleme fiyati.</summary>
    [JsonPropertyName("stopPrice")]
    public decimal? StopPrice { get; init; }

    /// <summary>Buzdagi emirlerde gorunen miktar.</summary>
    [JsonPropertyName("icebergQty")]
    public decimal? IcebergQuantity { get; init; }

    /// <summary>Emir defterde acik mi?</summary>
    [JsonPropertyName("isWorking")]
    [JsonConverter(typeof(BoolConverter))]
    public bool? IsWorking { get; init; }

    /// <summary>Emrin olusturuldugu an.</summary>
    [JsonPropertyName("createTime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreateTime { get; init; }
}

/// <summary>Emir listesi.</summary>
/// <remarks>Borsa listeyi <c>list</c> alani altinda sarmalar.</remarks>
[SerializationModel]
public record BinanceTROrderList
{
    /// <summary>Emirler.</summary>
    [JsonPropertyName("list")]
    public List<BinanceTROrder> Orders { get; init; } = [];
}

/// <summary>Hesabin gerceklesen bir islemi.</summary>
[SerializationModel]
public record BinanceTRUserTrade
{
    /// <summary>Islem kimligi.</summary>
    [JsonPropertyName("tradeId")]
    public long TradeId { get; init; }

    /// <summary>Islemin ait oldugu emrin kimligi.</summary>
    [JsonPropertyName("orderId")]
    public long OrderId { get; init; }

    /// <summary>Native parite adi.</summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>Gerceklesme fiyati.</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    /// <summary>Base varlik cinsinden miktar.</summary>
    [JsonPropertyName("qty")]
    public decimal Quantity { get; init; }

    /// <summary>Quote varlik cinsinden tutar.</summary>
    [JsonPropertyName("quoteQty")]
    public decimal QuoteQuantity { get; init; }

    /// <summary>Odenen komisyon.</summary>
    [JsonPropertyName("commission")]
    public decimal Commission { get; init; }

    /// <summary>Komisyonun alindigi varlik.</summary>
    [JsonPropertyName("commissionAsset")]
    public string? CommissionAsset { get; init; }

    /// <summary>Hesap bu islemde alici tarafta miydi?</summary>
    /// <remarks>Borsa bu bayragi 0 ya da 1 olarak dondurur.</remarks>
    [JsonPropertyName("isBuyer")]
    [JsonConverter(typeof(BoolConverter))]
    public bool IsBuyer { get; init; }

    /// <summary>Hesap bu islemde piyasa yapici miydi?</summary>
    [JsonPropertyName("isMaker")]
    [JsonConverter(typeof(BoolConverter))]
    public bool IsMaker { get; init; }

    /// <summary>Islemin gerceklestigi an.</summary>
    [JsonPropertyName("time")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }
}

/// <summary>Islem listesi.</summary>
/// <remarks>Borsa listeyi <c>list</c> alani altinda sarmalar.</remarks>
[SerializationModel]
public record BinanceTRUserTradeList
{
    /// <summary>Islemler.</summary>
    [JsonPropertyName("list")]
    public List<BinanceTRUserTrade> Trades { get; init; } = [];
}
