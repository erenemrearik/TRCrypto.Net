using TRCrypto.BinanceTR.Enums;

namespace TRCrypto.BinanceTR.Objects.Models.Socket;

/// <summary>
/// Kullanici akisindaki hesap bakiyesi guncellemesi.
/// </summary>
/// <remarks>
/// Yalnizca degisen varliklar gonderilir; yanit hesabin tamamini tasimaz.
/// </remarks>
[SerializationModel]
public record BinanceTRStreamAccountUpdate
{
    /// <summary>["<c>e</c>"] Olay turu; bu akista <c>outboundAccountPosition</c>.</summary>
    [JsonPropertyName("e")]
    public string Event { get; init; } = string.Empty;

    /// <summary>["<c>E</c>"] Olayin uretildigi an.</summary>
    [JsonPropertyName("E")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime EventTime { get; init; }

    /// <summary>["<c>u</c>"] Hesabin son guncellenme ani.</summary>
    [JsonPropertyName("u")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime LastUpdateTime { get; init; }

    /// <summary>["<c>B</c>"] Degisen varlik bakiyeleri.</summary>
    [JsonPropertyName("B")]
    public List<BinanceTRStreamBalance> Balances { get; init; } = [];
}

/// <summary>Kullanici akisindaki tek bir varlik bakiyesi.</summary>
[SerializationModel]
public record BinanceTRStreamBalance
{
    /// <summary>["<c>a</c>"] Varlik adi.</summary>
    [JsonPropertyName("a")]
    public string Asset { get; init; } = string.Empty;

    /// <summary>["<c>f</c>"] Kullanilabilir miktar.</summary>
    [JsonPropertyName("f")]
    public decimal Available { get; init; }

    /// <summary>["<c>l</c>"] Acik emirlerde bloke miktar.</summary>
    [JsonPropertyName("l")]
    public decimal Locked { get; init; }

    /// <summary>Toplam miktar.</summary>
    [JsonIgnore]
    public decimal Total => Available + Locked;
}

/// <summary>
/// Kullanici akisindaki emir guncellemesi.
/// </summary>
/// <remarks>
/// Alan adlari tek harfe kisaltilmistir ve harf buyuklugu anlam tasir: <c>s</c> parite,
/// <c>S</c> yon; <c>l</c> son gerceklesen miktar, <c>L</c> son gerceklesen fiyat.
/// <para>
/// <b>Yon, tur ve durum burada metin olarak gelir</b>, REST tarafinda ayni bilgiler sayi
/// olarak tasinir (D-42). Ayni enum iki tarafta farkli bicimde cozumlenir.
/// </para>
/// </remarks>
[SerializationModel]
public record BinanceTRStreamOrderUpdate
{
    /// <summary>["<c>e</c>"] Olay turu; bu akista <c>executionReport</c>.</summary>
    [JsonPropertyName("e")]
    public string Event { get; init; } = string.Empty;

    /// <summary>["<c>E</c>"] Olayin uretildigi an.</summary>
    [JsonPropertyName("E")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime EventTime { get; init; }

    /// <summary>["<c>s</c>"] Native parite adi.</summary>
    [JsonPropertyName("s")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>["<c>c</c>"] Cagiran tarafin verdigi kimlik.</summary>
    [JsonPropertyName("c")]
    public string? ClientOrderId { get; init; }

    /// <summary>["<c>S</c>"] Emrin yonu.</summary>
    [JsonPropertyName("S")]
    public OrderSide Side { get; init; }

    /// <summary>["<c>o</c>"] Emrin turu.</summary>
    [JsonPropertyName("o")]
    public OrderType Type { get; init; }

    /// <summary>["<c>f</c>"] Emrin gecerlilik suresi.</summary>
    [JsonPropertyName("f")]
    public TimeInForce? TimeInForce { get; init; }

    /// <summary>["<c>q</c>"] Emrin acilis miktari.</summary>
    [JsonPropertyName("q")]
    public decimal Quantity { get; init; }

    /// <summary>["<c>p</c>"] Limit fiyati.</summary>
    [JsonPropertyName("p")]
    public decimal Price { get; init; }

    /// <summary>["<c>P</c>"] Tetikleme fiyati.</summary>
    [JsonPropertyName("P")]
    public decimal? StopPrice { get; init; }

    /// <summary>["<c>x</c>"] Bu olayin tetikleyicisi.</summary>
    [JsonPropertyName("x")]
    public string? ExecutionType { get; init; }

    /// <summary>["<c>X</c>"] Emrin guncel durumu.</summary>
    [JsonPropertyName("X")]
    public OrderStatus Status { get; init; }

    /// <summary>["<c>i</c>"] Emir kimligi.</summary>
    [JsonPropertyName("i")]
    public long OrderId { get; init; }

    /// <summary>["<c>l</c>"] Bu olayda gerceklesen miktar.</summary>
    [JsonPropertyName("l")]
    public decimal LastQuantityFilled { get; init; }

    /// <summary>["<c>z</c>"] Emirde toplam gerceklesen miktar.</summary>
    [JsonPropertyName("z")]
    public decimal QuantityFilled { get; init; }

    /// <summary>["<c>L</c>"] Bu olayda gerceklesen fiyat.</summary>
    [JsonPropertyName("L")]
    public decimal LastPriceFilled { get; init; }

    /// <summary>["<c>n</c>"] Bu olayda odenen komisyon.</summary>
    [JsonPropertyName("n")]
    public decimal? Fee { get; init; }

    /// <summary>["<c>N</c>"] Komisyonun alindigi varlik.</summary>
    [JsonPropertyName("N")]
    public string? FeeAsset { get; init; }

    /// <summary>["<c>T</c>"] Islem ani.</summary>
    [JsonPropertyName("T")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime TradeTime { get; init; }

    /// <summary>["<c>t</c>"] Islem kimligi; islem yoksa -1.</summary>
    [JsonPropertyName("t")]
    public long TradeId { get; init; }

    /// <summary>["<c>w</c>"] Emir defterde acik mi?</summary>
    [JsonPropertyName("w")]
    public bool IsWorking { get; init; }

    /// <summary>["<c>m</c>"] Hesap bu islemde piyasa yapici miydi?</summary>
    [JsonPropertyName("m")]
    public bool IsMaker { get; init; }

    /// <summary>["<c>O</c>"] Emrin olusturuldugu an.</summary>
    [JsonPropertyName("O")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreateTime { get; init; }

    /// <summary>["<c>Z</c>"] Emirde toplam gerceklesen tutar.</summary>
    [JsonPropertyName("Z")]
    public decimal QuoteQuantityFilled { get; init; }

    /// <summary>["<c>Y</c>"] Bu olayda gerceklesen tutar.</summary>
    [JsonPropertyName("Y")]
    public decimal LastQuoteQuantityFilled { get; init; }
}
