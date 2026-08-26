using System.Globalization;
using System.Text.Json;

namespace TRCrypto.BinanceTR.Objects.Models;

/// <summary>Borsanin destekledigi pariteler.</summary>
[SerializationModel]
public record BinanceTRExchangeInfo
{
    /// <summary>["<c>list</c>"] Islem gorebilen pariteler.</summary>
    [JsonPropertyName("list")]
    public IReadOnlyList<BinanceTRSymbol> Symbols { get; init; } = [];
}

/// <summary>Tek bir parite hakkinda islem kurallari.</summary>
[SerializationModel]
public record BinanceTRSymbol
{
    /// <summary>["<c>symbol</c>"] Native sembol adi, ornegin <c>BTC_TRY</c>.</summary>
    /// <remarks>Binance TR sembollerinde alt cizgi kullanir.</remarks>
    [JsonPropertyName("symbol")]
    public string Name { get; init; } = string.Empty;

    /// <summary>["<c>baseAsset</c>"] Base varlik, ornegin <c>BTC</c>.</summary>
    /// <remarks>Ayri alan olarak geldigi icin sembol adi ayristirilmaz.</remarks>
    [JsonPropertyName("baseAsset")]
    public string BaseAsset { get; init; } = string.Empty;

    /// <summary>["<c>quoteAsset</c>"] Quote varlik, ornegin <c>TRY</c>.</summary>
    [JsonPropertyName("quoteAsset")]
    public string QuoteAsset { get; init; } = string.Empty;

    /// <summary>["<c>basePrecision</c>"] Miktar icin ondalik basamak sayisi.</summary>
    [JsonPropertyName("basePrecision")]
    public int BasePrecision { get; init; }

    /// <summary>["<c>quotePrecision</c>"] Fiyat icin ondalik basamak sayisi.</summary>
    [JsonPropertyName("quotePrecision")]
    public int QuotePrecision { get; init; }

    /// <summary>
    /// ["<c>type</c>"] Parite turu.
    /// </summary>
    /// <remarks>
    /// WebSocket akis adresi bu degere gore degisir; tur 1 ve tur 3 farkli hostlar kullanir.
    /// </remarks>
    [JsonPropertyName("type")]
    public int Type { get; init; }

    /// <summary>["<c>filters</c>"] Fiyat ve miktar kisitlari.</summary>
    [JsonPropertyName("filters")]
    public IReadOnlyList<BinanceTRSymbolFilter> Filters { get; init; } = [];
}

/// <summary>Bir parite icin fiyat ya da miktar kisiti.</summary>
/// <remarks>Yapisi global Binance ile aynidir.</remarks>
[SerializationModel]
public record BinanceTRSymbolFilter
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

    /// <summary>["<c>minQty</c>"] En dusuk miktar.</summary>
    [JsonPropertyName("minQty")]
    public decimal? MinQuantity { get; init; }

    /// <summary>["<c>maxQty</c>"] En yuksek miktar.</summary>
    [JsonPropertyName("maxQty")]
    public decimal? MaxQuantity { get; init; }

    /// <summary>["<c>stepSize</c>"] Miktar adimi.</summary>
    [JsonPropertyName("stepSize")]
    public decimal? StepSize { get; init; }

    /// <summary>["<c>applyToMarket</c>"] Kisitin piyasa emirlerine uygulanip uygulanmadigi.</summary>
    [JsonPropertyName("applyToMarket")]
    public bool ApplyToMarket { get; init; }
}

/// <summary>Bir parite icin acik emirlerin anlik goruntusu.</summary>
[SerializationModel]
public record BinanceTROrderBook
{
    /// <summary>
    /// ["<c>lastUpdateId</c>"] Goruntunun sira numarasi.
    /// </summary>
    /// <remarks>
    /// Fark mesajlarinin sirasini dogrulamak icin kullanilir; atlama tespit edilirse
    /// defter gecersiz sayilip yeni bir goruntu alinmalidir.
    /// </remarks>
    [JsonPropertyName("lastUpdateId")]
    public long LastUpdateId { get; init; }

    /// <summary>["<c>bids</c>"] Alis kademeleri.</summary>
    [JsonPropertyName("bids")]
    public IReadOnlyList<BinanceTROrderBookEntry> Bids { get; init; } = [];

    /// <summary>["<c>asks</c>"] Satis kademeleri.</summary>
    [JsonPropertyName("asks")]
    public IReadOnlyList<BinanceTROrderBookEntry> Asks { get; init; } = [];
}

/// <summary>Emir defterinde tek bir fiyat kademesi.</summary>
/// <remarks>Kaynakta <c>["fiyat", "miktar"]</c> bicimindeki iki elemanli bir dizidir.</remarks>
[JsonConverter(typeof(BinanceTROrderBookEntryConverter))]
public record BinanceTROrderBookEntry : ISymbolOrderBookEntry
{
    /// <summary>Kademe fiyati.</summary>
    public decimal Price { get; init; }

    /// <summary>Bu fiyattaki toplam miktar.</summary>
    public decimal Quantity { get; init; }

    decimal ISymbolOrderBookEntry.Price
    {
        get => Price;
        set => throw new NotSupportedException("Emir defteri kademesi degistirilemez.");
    }

    decimal ISymbolOrderBookEntry.Quantity
    {
        get => Quantity;
        set => throw new NotSupportedException("Emir defteri kademesi degistirilemez.");
    }
}

/// <summary>Iki elemanli fiyat/miktar dizisini nesneye cevirir.</summary>
internal class BinanceTROrderBookEntryConverter : JsonConverter<BinanceTROrderBookEntry>
{
    /// <inheritdoc />
    public override BinanceTROrderBookEntry Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Emir defteri kademesi bir dizi olmalidir.");

        reader.Read();
        var price = ReadDecimal(ref reader);

        reader.Read();
        var quantity = ReadDecimal(ref reader);

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
        }

        return new BinanceTROrderBookEntry { Price = price, Quantity = quantity };
    }

    private static decimal ReadDecimal(ref Utf8JsonReader reader)
        => reader.TokenType switch
        {
            JsonTokenType.String => decimal.Parse(
                reader.GetString()!, NumberStyles.Any, CultureInfo.InvariantCulture),
            JsonTokenType.Number => reader.GetDecimal(),
            _ => throw new JsonException("Emir defteri degeri sayi ya da metin olmalidir.")
        };

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer, BinanceTROrderBookEntry value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Price.ToString(CultureInfo.InvariantCulture));
        writer.WriteStringValue(value.Quantity.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndArray();
    }
}

/// <summary>Toplulastirilmis islem listesi.</summary>
[SerializationModel]
public record BinanceTRAggregatedTradeList
{
    /// <summary>["<c>list</c>"] Toplulastirilmis islemler.</summary>
    [JsonPropertyName("list")]
    public IReadOnlyList<BinanceTRAggregatedTrade> Trades { get; init; } = [];
}

/// <summary>
/// Ayni yonde ve ayni fiyatta gerceklesmis islemlerin toplulastirilmis kaydi.
/// </summary>
/// <remarks>
/// Alan adlari global Binance ile aynidir ve tek harfe kisaltilmistir.
/// </remarks>
[SerializationModel]
public record BinanceTRAggregatedTrade
{
    /// <summary>["<c>a</c>"] Toplulastirilmis islem kimligi.</summary>
    [JsonPropertyName("a")]
    public long Id { get; init; }

    /// <summary>["<c>p</c>"] Islem fiyati.</summary>
    [JsonPropertyName("p")]
    public decimal Price { get; init; }

    /// <summary>["<c>q</c>"] Islem miktari.</summary>
    [JsonPropertyName("q")]
    public decimal Quantity { get; init; }

    /// <summary>["<c>f</c>"] Toplulastirmadaki ilk islem kimligi.</summary>
    [JsonPropertyName("f")]
    public long FirstTradeId { get; init; }

    /// <summary>["<c>l</c>"] Toplulastirmadaki son islem kimligi.</summary>
    [JsonPropertyName("l")]
    public long LastTradeId { get; init; }

    /// <summary>["<c>T</c>"] Islemin gerceklestigi an (UTC).</summary>
    [JsonPropertyName("T")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>["<c>m</c>"] Alici tarafin piyasa yapici olup olmadigi.</summary>
    [JsonPropertyName("m")]
    public bool BuyerIsMaker { get; init; }

    /// <summary>
    /// Islemin yonu.
    /// </summary>
    /// <remarks>
    /// Kaynak yon alanini dogrudan vermez. Alici piyasa yapiciysa emri veren taraf
    /// saticidir; dolayisiyla yon satistir.
    /// </remarks>
    [JsonIgnore]
    public OrderSide Side => BuyerIsMaker ? OrderSide.Sell : OrderSide.Buy;
}
