namespace TRCrypto.BtcTurk.Objects.Models;

/// <summary>Borsanin desteklediği pariteler, varliklar ve sunucu saati.</summary>
[SerializationModel]
public record BtcTurkExchangeInfo
{
    /// <summary>["<c>timeZone</c>"] Sunucu saat dilimi; BtcTurk her zaman <c>UTC</c> bildirir.</summary>
    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; init; }

    /// <summary>["<c>serverTime</c>"] Sunucu saati (UTC). Kaynakta milisaniye epoch olarak gelir.</summary>
    [JsonPropertyName("serverTime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime ServerTime { get; init; }

    /// <summary>["<c>symbols</c>"] Islem gorebilen pariteler.</summary>
    [JsonPropertyName("symbols")]
    public IReadOnlyList<BtcTurkSymbol> Symbols { get; init; } = [];

    /// <summary>["<c>currencies</c>"] Desteklenen varliklar.</summary>
    [JsonPropertyName("currencies")]
    public IReadOnlyList<BtcTurkCurrency> Currencies { get; init; } = [];

    /// <summary>["<c>currencyOperationBlocks</c>"] Varlik bazinda yatirma/cekme kisitlari.</summary>
    [JsonPropertyName("currencyOperationBlocks")]
    public IReadOnlyList<BtcTurkCurrencyOperationBlock> CurrencyOperationBlocks { get; init; } = [];
}

/// <summary>Tek bir parite hakkinda islem kurallari ve olcek bilgisi.</summary>
[SerializationModel]
public record BtcTurkSymbol
{
    /// <summary>["<c>id</c>"] Parite kimligi.</summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>["<c>name</c>"] Native sembol adi, ornegin <c>BTCTRY</c>.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>["<c>nameNormalized</c>"] Normalize edilmis ad, ornegin <c>BTC_TRY</c>.</summary>
    [JsonPropertyName("nameNormalized")]
    public string NameNormalized { get; init; } = string.Empty;

    /// <summary>
    /// ["<c>numerator</c>"] Base varlik, ornegin <c>BTC</c>.
    /// Sembol adindan ayristirmaya gerek yoktur; borsa bu alani ayrica bildirir.
    /// </summary>
    [JsonPropertyName("numerator")]
    public string Numerator { get; init; } = string.Empty;

    /// <summary>["<c>denominator</c>"] Quote varlik, ornegin <c>TRY</c>.</summary>
    [JsonPropertyName("denominator")]
    public string Denominator { get; init; } = string.Empty;

    /// <summary>["<c>numeratorScale</c>"] Miktar icin ondalik basamak sayisi.</summary>
    [JsonPropertyName("numeratorScale")]
    public int NumeratorScale { get; init; }

    /// <summary>["<c>denominatorScale</c>"] Fiyat icin ondalik basamak sayisi.</summary>
    [JsonPropertyName("denominatorScale")]
    public int DenominatorScale { get; init; }

    /// <summary>["<c>status</c>"] Paritenin islem durumu.</summary>
    [JsonPropertyName("status")]
    public SymbolStatus Status { get; init; }

    /// <summary>["<c>hasFraction</c>"] Kesirli miktara izin verilip verilmedigi.</summary>
    [JsonPropertyName("hasFraction")]
    public bool HasFraction { get; init; }

    /// <summary>["<c>filters</c>"] Fiyat ve miktar kisitlari.</summary>
    [JsonPropertyName("filters")]
    public IReadOnlyList<BtcTurkSymbolFilter> Filters { get; init; } = [];

    /// <summary>["<c>orderMethods</c>"] Bu paritede kullanilabilen emir yontemleri.</summary>
    [JsonPropertyName("orderMethods")]
    public IReadOnlyList<OrderMethod> OrderMethods { get; init; } = [];

    /// <summary>["<c>displayFormat</c>"] Arayuzde kullanilan sayi bicimi deseni.</summary>
    [JsonPropertyName("displayFormat")]
    public string? DisplayFormat { get; init; }

    /// <summary>["<c>commissionFromNumerator</c>"] Komisyonun base varliktan alinip alinmadigi.</summary>
    [JsonPropertyName("commissionFromNumerator")]
    public bool CommissionFromNumerator { get; init; }

    /// <summary>["<c>order</c>"] Goruntuleme sirasi.</summary>
    [JsonPropertyName("order")]
    public int Order { get; init; }

    /// <summary>["<c>priceRounding</c>"] Fiyat yuvarlamasinin acik olup olmadigi.</summary>
    [JsonPropertyName("priceRounding")]
    public bool PriceRounding { get; init; }

    /// <summary>["<c>isNew</c>"] Paritenin yeni eklenmis olup olmadigi.</summary>
    [JsonPropertyName("isNew")]
    public bool IsNew { get; init; }

    /// <summary>["<c>marketPriceWarningThresholdPercentage</c>"] Piyasa emri uyari esigi (yuzde).</summary>
    [JsonPropertyName("marketPriceWarningThresholdPercentage")]
    public decimal MarketPriceWarningThresholdPercentage { get; init; }

    /// <summary>["<c>maximumLimitOrderPrice</c>"] Izin verilen en yuksek limit emir fiyati.</summary>
    [JsonPropertyName("maximumLimitOrderPrice")]
    public decimal? MaximumLimitOrderPrice { get; init; }

    /// <summary>["<c>minimumLimitOrderPrice</c>"] Izin verilen en dusuk limit emir fiyati.</summary>
    [JsonPropertyName("minimumLimitOrderPrice")]
    public decimal? MinimumLimitOrderPrice { get; init; }

    /// <summary>["<c>maximumOrderAmount</c>"] Izin verilen en yuksek emir miktari.</summary>
    [JsonPropertyName("maximumOrderAmount")]
    public decimal? MaximumOrderAmount { get; init; }
}

/// <summary>Bir parite icin fiyat/miktar kisiti.</summary>
[SerializationModel]
public record BtcTurkSymbolFilter
{
    /// <summary>["<c>filterType</c>"] Kisit turu, ornegin <c>PRICE_FILTER</c>.</summary>
    [JsonPropertyName("filterType")]
    public string FilterType { get; init; } = string.Empty;

    /// <summary>["<c>minPrice</c>"] En dusuk fiyat.</summary>
    [JsonPropertyName("minPrice")]
    public decimal? MinPrice { get; init; }

    /// <summary>["<c>maxPrice</c>"] En yuksek fiyat.</summary>
    [JsonPropertyName("maxPrice")]
    public decimal? MaxPrice { get; init; }

    /// <summary>["<c>tickSize</c>"] Fiyat adimi.</summary>
    [JsonPropertyName("tickSize")]
    public decimal? TickSize { get; init; }

    /// <summary>["<c>minExchangeValue</c>"] En dusuk islem tutari.</summary>
    [JsonPropertyName("minExchangeValue")]
    public decimal? MinExchangeValue { get; init; }

    /// <summary>["<c>minAmount</c>"] En dusuk miktar.</summary>
    [JsonPropertyName("minAmount")]
    public decimal? MinAmount { get; init; }

    /// <summary>["<c>maxAmount</c>"] En yuksek miktar.</summary>
    [JsonPropertyName("maxAmount")]
    public decimal? MaxAmount { get; init; }
}

/// <summary>Desteklenen bir varlik ve yatirma/cekme kurallari.</summary>
[SerializationModel]
public record BtcTurkCurrency
{
    /// <summary>["<c>id</c>"] Varlik kimligi.</summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>["<c>symbol</c>"] Varlik sembolu, ornegin <c>TRY</c>.</summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>["<c>name</c>"] Varligin tam adi, ornegin <c>Türk Lirası</c>.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// ["<c>currencyType</c>"] Varlik turu. Bu alan sayesinde TRY'nin itibari para oldugu
    /// tahmin edilmez; borsa dogrudan bildirir.
    /// </summary>
    [JsonPropertyName("currencyType")]
    public CurrencyType CurrencyType { get; init; }

    /// <summary>["<c>precision</c>"] Ondalik hassasiyet.</summary>
    [JsonPropertyName("precision")]
    public int Precision { get; init; }

    /// <summary>["<c>minWithdrawal</c>"] En dusuk cekim tutari.</summary>
    [JsonPropertyName("minWithdrawal")]
    public decimal MinWithdrawal { get; init; }

    /// <summary>["<c>minDeposit</c>"] En dusuk yatirma tutari.</summary>
    [JsonPropertyName("minDeposit")]
    public decimal MinDeposit { get; init; }

    /// <summary>["<c>address</c>"] Adres uzunlugu kisitlari.</summary>
    [JsonPropertyName("address")]
    public BtcTurkAddressInfo? Address { get; init; }

    /// <summary>["<c>tag</c>"] Etiket (memo) kurallari.</summary>
    [JsonPropertyName("tag")]
    public BtcTurkTagInfo? Tag { get; init; }

    /// <summary>["<c>color</c>"] Arayuzde kullanilan renk kodu.</summary>
    [JsonPropertyName("color")]
    public string? Color { get; init; }

    /// <summary>["<c>isAddressRenewable</c>"] Yatirma adresi yenilenebilir mi.</summary>
    [JsonPropertyName("isAddressRenewable")]
    public bool IsAddressRenewable { get; init; }

    /// <summary>["<c>getAutoAddressDisabled</c>"] Otomatik adres uretimi kapali mi.</summary>
    [JsonPropertyName("getAutoAddressDisabled")]
    public bool GetAutoAddressDisabled { get; init; }

    /// <summary>["<c>isPartialWithdrawalEnabled</c>"] Kismi cekime izin veriliyor mu.</summary>
    [JsonPropertyName("isPartialWithdrawalEnabled")]
    public bool IsPartialWithdrawalEnabled { get; init; }

    /// <summary>["<c>isNew</c>"] Varligin yeni eklenmis olup olmadigi.</summary>
    [JsonPropertyName("isNew")]
    public bool IsNew { get; init; }
}

/// <summary>Bir varlik icin adres uzunlugu kisitlari.</summary>
[SerializationModel]
public record BtcTurkAddressInfo
{
    /// <summary>["<c>minLen</c>"] En kisa adres uzunlugu.</summary>
    [JsonPropertyName("minLen")]
    public int? MinLength { get; init; }

    /// <summary>["<c>maxLen</c>"] En uzun adres uzunlugu.</summary>
    [JsonPropertyName("maxLen")]
    public int? MaxLength { get; init; }
}

/// <summary>Bir varlik icin etiket (memo/tag) kurallari.</summary>
[SerializationModel]
public record BtcTurkTagInfo
{
    /// <summary>["<c>enable</c>"] Etiket kullanimi zorunlu mu.</summary>
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }

    /// <summary>["<c>name</c>"] Etiketin adi, ornegin <c>Memo</c>.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>["<c>minLen</c>"] En kisa etiket uzunlugu.</summary>
    [JsonPropertyName("minLen")]
    public int? MinLength { get; init; }

    /// <summary>["<c>maxLen</c>"] En uzun etiket uzunlugu.</summary>
    [JsonPropertyName("maxLen")]
    public int? MaxLength { get; init; }
}

/// <summary>Bir varlik icin gecerli yatirma/cekme kisiti.</summary>
[SerializationModel]
public record BtcTurkCurrencyOperationBlock
{
    /// <summary>["<c>currencySymbol</c>"] Varlik sembolu.</summary>
    [JsonPropertyName("currencySymbol")]
    public string CurrencySymbol { get; init; } = string.Empty;

    /// <summary>["<c>withdrawalDisabled</c>"] Cekim islemleri kapali mi.</summary>
    [JsonPropertyName("withdrawalDisabled")]
    public bool WithdrawalDisabled { get; init; }

    /// <summary>["<c>depositDisabled</c>"] Yatirma islemleri kapali mi.</summary>
    [JsonPropertyName("depositDisabled")]
    public bool DepositDisabled { get; init; }
}
