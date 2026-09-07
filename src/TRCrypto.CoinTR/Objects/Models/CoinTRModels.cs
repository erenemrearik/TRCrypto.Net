using System.Globalization;
using System.Text.Json;

namespace TRCrypto.CoinTR.Objects.Models;

/// <summary>Borsanin sunucu saati.</summary>
[SerializationModel]
public record CoinTRServerTime
{
    /// <summary>["<c>serverTime</c>"] Sunucu saati.</summary>
    [JsonPropertyName("serverTime")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime ServerTime { get; init; }
}

/// <summary>
/// Islem yapilabilir bir parite.
/// </summary>
/// <remarks>
/// Base ve quote varlik ayri alanlarda gelir; sembol adi ayristirilmaz. Komisyon oranlari
/// parite basina bildirilir, ki bu diger uc borsada bulunmayan bir bilgidir.
/// </remarks>
[SerializationModel]
public record CoinTRSymbol
{
    /// <summary>["<c>symbol</c>"] Native parite adi, ornegin <c>BTCTRY</c>.</summary>
    [JsonPropertyName("symbol")]
    public string Name { get; init; } = string.Empty;

    /// <summary>["<c>baseCoin</c>"] Base varlik.</summary>
    [JsonPropertyName("baseCoin")]
    public string BaseAsset { get; init; } = string.Empty;

    /// <summary>["<c>quoteCoin</c>"] Quote varlik.</summary>
    [JsonPropertyName("quoteCoin")]
    public string QuoteAsset { get; init; } = string.Empty;

    /// <summary>["<c>status</c>"] Paritenin durumu; islem gorenlerde <c>online</c>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>["<c>pricePrecision</c>"] Fiyattaki ondalik basamak sayisi.</summary>
    [JsonPropertyName("pricePrecision")]
    public int PriceDecimals { get; init; }

    /// <summary>["<c>quantityPrecision</c>"] Miktardaki ondalik basamak sayisi.</summary>
    [JsonPropertyName("quantityPrecision")]
    public int QuantityDecimals { get; init; }

    /// <summary>["<c>minTradeAmount</c>"] En az islem miktari.</summary>
    [JsonPropertyName("minTradeAmount")]
    public decimal MinTradeQuantity { get; init; }

    /// <summary>["<c>maxTradeAmount</c>"] En fazla islem miktari.</summary>
    [JsonPropertyName("maxTradeAmount")]
    public decimal MaxTradeQuantity { get; init; }

    /// <summary>["<c>makerFeeRate</c>"] Piyasa yapici komisyon orani.</summary>
    [JsonPropertyName("makerFeeRate")]
    public decimal MakerFeeRate { get; init; }

    /// <summary>["<c>takerFeeRate</c>"] Piyasa alici komisyon orani.</summary>
    [JsonPropertyName("takerFeeRate")]
    public decimal TakerFeeRate { get; init; }

    /// <summary>Parite islem goruyor mu?</summary>
    [JsonIgnore]
    public bool IsTrading => string.Equals(Status, "online", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Bir paritenin son 24 saatlik ozeti.</summary>
[SerializationModel]
public record CoinTRTicker
{
    /// <summary>["<c>symbol</c>"] Native parite adi.</summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>["<c>lastPr</c>"] Son islem fiyati.</summary>
    [JsonPropertyName("lastPr")]
    public decimal LastPrice { get; init; }

    /// <summary>["<c>open</c>"] 24 saat oncesindeki fiyat.</summary>
    [JsonPropertyName("open")]
    public decimal OpenPrice { get; init; }

    /// <summary>["<c>high24h</c>"] Son 24 saatteki en yuksek fiyat.</summary>
    [JsonPropertyName("high24h")]
    public decimal HighPrice { get; init; }

    /// <summary>["<c>low24h</c>"] Son 24 saatteki en dusuk fiyat.</summary>
    [JsonPropertyName("low24h")]
    public decimal LowPrice { get; init; }

    /// <summary>["<c>bidPr</c>"] En iyi alis fiyati.</summary>
    [JsonPropertyName("bidPr")]
    public decimal BestBidPrice { get; init; }

    /// <summary>["<c>askPr</c>"] En iyi satis fiyati.</summary>
    [JsonPropertyName("askPr")]
    public decimal BestAskPrice { get; init; }

    /// <summary>["<c>bidSz</c>"] En iyi alis miktari.</summary>
    [JsonPropertyName("bidSz")]
    public decimal BestBidQuantity { get; init; }

    /// <summary>["<c>askSz</c>"] En iyi satis miktari.</summary>
    [JsonPropertyName("askSz")]
    public decimal BestAskQuantity { get; init; }

    /// <summary>["<c>baseVolume</c>"] Base varlik cinsinden hacim.</summary>
    [JsonPropertyName("baseVolume")]
    public decimal Volume { get; init; }

    /// <summary>["<c>quoteVolume</c>"] Quote varlik cinsinden hacim.</summary>
    [JsonPropertyName("quoteVolume")]
    public decimal QuoteVolume { get; init; }

    /// <summary>["<c>ts</c>"] Verinin uretildigi an.</summary>
    [JsonPropertyName("ts")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// ["<c>change24h</c>"] Son 24 saatteki degisim orani.
    /// </summary>
    /// <remarks>
    /// Deger <b>kesirdir</b>, yuzde degil: <c>-0.00912</c> yuzde 0,912 dusus demektir.
    /// BtcTurk ve Paribu ayni bilgiyi yuzde olarak verir; dogrudan aktarmak degeri yuz kat
    /// kucuk gosterir. <see cref="ChangePercentage"/> donusumu yapar.
    /// </remarks>
    [JsonPropertyName("change24h")]
    public decimal ChangeRatio { get; init; }

    /// <summary>Son 24 saatteki degisim yuzdesi.</summary>
    [JsonIgnore]
    public decimal ChangePercentage => ChangeRatio * 100m;
}

/// <summary>Emir defterinin anlik goruntusu.</summary>
[SerializationModel]
public record CoinTROrderBook
{
    /// <summary>["<c>asks</c>"] Satis kademeleri.</summary>
    [JsonPropertyName("asks")]
    public List<CoinTROrderBookEntry> Asks { get; init; } = [];

    /// <summary>["<c>bids</c>"] Alis kademeleri.</summary>
    [JsonPropertyName("bids")]
    public List<CoinTROrderBookEntry> Bids { get; init; } = [];

    /// <summary>["<c>ts</c>"] Goruntunun alindigi an.</summary>
    [JsonPropertyName("ts")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Emir defterindeki tek bir kademe.
/// </summary>
/// <remarks>Borsa kademeleri <c>[fiyat, miktar]</c> dizisi olarak gonderir.</remarks>
[JsonConverter(typeof(CoinTROrderBookEntryConverter))]
public record CoinTROrderBookEntry : ISymbolOrderBookEntry
{
    /// <summary>Kademe fiyati.</summary>
    public decimal Price { get; init; }

    /// <summary>Kademedeki miktar.</summary>
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
internal class CoinTROrderBookEntryConverter : JsonConverter<CoinTROrderBookEntry>
{
    /// <inheritdoc />
    public override CoinTROrderBookEntry Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Emir defteri kademesi bir dizi olmalidir.");

        reader.Read();
        var price = ArrayReader.ReadDecimal(ref reader);

        reader.Read();
        var quantity = ArrayReader.ReadDecimal(ref reader);

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
        }

        return new CoinTROrderBookEntry { Price = price, Quantity = quantity };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer, CoinTROrderBookEntry value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Price.ToString(CultureInfo.InvariantCulture));
        writer.WriteStringValue(value.Quantity.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndArray();
    }
}

/// <summary>Isimsiz dizilerdeki degerleri okur.</summary>
/// <remarks>
/// CoinTR hem emir defteri kademelerini hem mumlari alan adi olmadan gonderir. Degerler
/// metin gelir ama sayi gelme ihtimaline karsi ikisi de kabul edilir.
/// </remarks>
internal static class ArrayReader
{
    public static decimal ReadDecimal(ref Utf8JsonReader reader)
        => reader.TokenType switch
        {
            JsonTokenType.String => decimal.Parse(
                reader.GetString()!, NumberStyles.Any, CultureInfo.InvariantCulture),
            JsonTokenType.Number => reader.GetDecimal(),
            _ => throw new JsonException("Dizi degeri sayi ya da metin olmalidir.")
        };

    public static long ReadInt64(ref Utf8JsonReader reader)
        => reader.TokenType switch
        {
            JsonTokenType.String => long.Parse(
                reader.GetString()!, NumberStyles.Any, CultureInfo.InvariantCulture),
            JsonTokenType.Number => reader.GetInt64(),
            _ => throw new JsonException("Dizi degeri sayi ya da metin olmalidir.")
        };
}

/// <summary>Gerceklesmis bir islem.</summary>
[SerializationModel]
public record CoinTRTrade
{
    /// <summary>["<c>symbol</c>"] Native parite adi.</summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    /// <summary>["<c>tradeId</c>"] Islem kimligi.</summary>
    [JsonPropertyName("tradeId")]
    public string TradeId { get; init; } = string.Empty;

    /// <summary>["<c>price</c>"] Gerceklesme fiyati.</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    /// <summary>["<c>size</c>"] Gerceklesme miktari.</summary>
    [JsonPropertyName("size")]
    public decimal Quantity { get; init; }

    /// <summary>
    /// ["<c>side</c>"] Islemin yonu.
    /// </summary>
    /// <remarks>
    /// Yon burada <b>metin</b> olarak gelir (<c>buy</c> ya da <c>sell</c>). Binance TR
    /// ayni bilgiyi sayi olarak tasir, BtcTurk ise socket akisinda sayisal bir alanda.
    /// </remarks>
    [JsonPropertyName("side")]
    public OrderSide Side { get; init; }

    /// <summary>["<c>ts</c>"] Islemin gerceklestigi an.</summary>
    [JsonPropertyName("ts")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Tek bir mum.
/// </summary>
/// <remarks>
/// Borsa mumlari <b>isimsiz dizi</b> olarak gonderir. Alan adi tasimadigi icin sira
/// anlamin tek tasiyicisidir ve testle sabitlenmistir.
/// </remarks>
[JsonConverter(typeof(CoinTRKlineConverter))]
public record CoinTRKline
{
    /// <summary>Mumun acilis ani.</summary>
    public DateTime OpenTime { get; init; }

    /// <summary>Acilis fiyati.</summary>
    public decimal OpenPrice { get; init; }

    /// <summary>En yuksek fiyat.</summary>
    public decimal HighPrice { get; init; }

    /// <summary>En dusuk fiyat.</summary>
    public decimal LowPrice { get; init; }

    /// <summary>Kapanis fiyati.</summary>
    public decimal ClosePrice { get; init; }

    /// <summary>Base varlik cinsinden hacim.</summary>
    public decimal Volume { get; init; }

    /// <summary>Quote varlik cinsinden hacim.</summary>
    public decimal QuoteVolume { get; init; }
}

/// <summary>
/// Isimsiz mum dizisini nesneye cevirir.
/// </summary>
/// <remarks>
/// Sira anlamin tek tasiyicisidir: acilis ani, acilis, en yuksek, en dusuk, kapanis,
/// base hacim, quote hacim. Yanlis sira hata vermez, yalnizca yanlis mum uretir; bu
/// yuzden sira testle sabitlenmistir.
/// </remarks>
internal class CoinTRKlineConverter : JsonConverter<CoinTRKline>
{
    /// <inheritdoc />
    public override CoinTRKline Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Mum bir dizi olmalidir.");

        reader.Read();
        var openTime = ArrayReader.ReadInt64(ref reader);

        var values = new decimal[6];
        for (var i = 0; i < values.Length; i++)
        {
            reader.Read();
            values[i] = ArrayReader.ReadDecimal(ref reader);
        }

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
        }

        return new CoinTRKline
        {
            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(openTime).UtcDateTime,
            OpenPrice = values[0],
            HighPrice = values[1],
            LowPrice = values[2],
            ClosePrice = values[3],
            Volume = values[4],
            QuoteVolume = values[5]
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CoinTRKline value, JsonSerializerOptions options)
        => throw new NotSupportedException("Mumlar yalnizca okunur.");
}
