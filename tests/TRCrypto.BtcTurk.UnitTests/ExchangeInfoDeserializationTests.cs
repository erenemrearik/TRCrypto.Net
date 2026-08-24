using System.Text.Json;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models;
using TRCrypto.BtcTurk.Enums;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// BtcTurk exchangeinfo yanitinin dogru ayristirildigini dogrular.
/// Fixture, canli public API'den alinmis gercek bir yanittir (spesifikasyon Bolum 15.3).
/// </summary>
public class ExchangeInfoDeserializationTests
{
    private static BtcTurkResponse<BtcTurkExchangeInfo> Parse(string fixture)
        => JsonSerializer.Deserialize<BtcTurkResponse<BtcTurkExchangeInfo>>(
               FixtureLoader.Load(fixture), BtcTurkJsonOptions.Default)!;

    [Fact]
    public void Envelope_basarili_yaniti_dogru_okur()
    {
        var response = Parse("exchangeinfo.json");

        Assert.True(response.Success);
        Assert.Equal("0", response.Code);
        Assert.NotNull(response.Data);

        // Not: BtcTurk basarili yanitlarda "message" alanini bos string olarak dondurur,
        // dokumantasyonun belirttigi gibi null olarak degil. Canli yanitla dogrulanmistir.
        Assert.True(string.IsNullOrEmpty(response.Message),
            $"Beklenen bos mesaj, gelen: '{response.Message}'");
    }

    [Fact]
    public void ServerTime_milisaniye_epoch_olarak_cozulur()
    {
        var data = Parse("exchangeinfo.json").Data!;

        // Fixture'daki serverTime 2026 icinde bir ms-epoch degeridir.
        Assert.Equal(DateTimeKind.Utc, data.ServerTime.Kind);
        Assert.True(data.ServerTime > new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            $"serverTime saniye olarak yorumlanmis olabilir: {data.ServerTime:O}");
        Assert.Equal("UTC", data.TimeZone);
    }

    [Fact]
    public void Sembol_alanlari_tam_okunur()
    {
        var btcTry = Parse("exchangeinfo.json").Data!.Symbols.Single(x => x.Name == "BTCTRY");

        Assert.Equal(1, btcTry.Id);
        Assert.Equal("BTC_TRY", btcTry.NameNormalized);
        Assert.Equal("BTC", btcTry.Numerator);
        Assert.Equal("TRY", btcTry.Denominator);
        Assert.Equal(8, btcTry.NumeratorScale);
        Assert.Equal(0, btcTry.DenominatorScale);
        Assert.False(btcTry.HasFraction);
        Assert.Equal(SymbolStatus.Trading, btcTry.Status);
    }

    [Fact]
    public void Fiyat_alanlari_decimal_olarak_okunur_ve_hassasiyet_kaybetmez()
    {
        var btcTry = Parse("exchangeinfo.json").Data!.Symbols.Single(x => x.Name == "BTCTRY");
        var priceFilter = btcTry.Filters.Single(x => x.FilterType == "PRICE_FILTER");

        // string "0.0000000000001" -> decimal, double'a dusurulmeden
        Assert.Equal(0.0000000000001m, priceFilter.MinPrice);
        Assert.Equal(10000000m, priceFilter.MaxPrice);
        Assert.Equal(10m, priceFilter.TickSize);
        Assert.Equal(99.91m, priceFilter.MinExchangeValue);
        Assert.Null(priceFilter.MinAmount);
    }

    [Fact]
    public void Emir_metodlari_okunur()
    {
        var btcTry = Parse("exchangeinfo.json").Data!.Symbols.Single(x => x.Name == "BTCTRY");

        Assert.Equal(4, btcTry.OrderMethods.Count);
        Assert.Contains(OrderMethod.Limit, btcTry.OrderMethods);
        Assert.Contains(OrderMethod.StopLimit, btcTry.OrderMethods);
    }

    [Fact]
    public void Varlik_tipi_borsanin_bildirdigi_gibi_okunur()
    {
        var currencies = Parse("exchangeinfo.json").Data!.Currencies;

        // TRY'nin fiat oldugunu TAHMIN ETMIYORUZ - BtcTurk bunu currencyType ile bildiriyor.
        Assert.Equal(CurrencyType.Fiat, currencies.Single(x => x.Symbol == "TRY").CurrencyType);
        Assert.Equal(CurrencyType.Crypto, currencies.Single(x => x.Symbol == "BTC").CurrencyType);
        Assert.Equal("Türk Lirası", currencies.Single(x => x.Symbol == "TRY").Name);
    }

    [Fact]
    public void Bilinmeyen_enum_degeri_ayristirmayi_dusurmez()
    {
        // Borsa gelecekte yeni bir status/orderMethod eklerse kutuphane kirilmamali.
        const string json = """
            {"success":true,"message":null,"code":0,"data":{"timeZone":"UTC","serverTime":1641916253216,
             "symbols":[{"id":9,"name":"XYZTRY","nameNormalized":"XYZ_TRY","status":"HALTED_NEW_THING",
             "numerator":"XYZ","denominator":"TRY","numeratorScale":2,"denominatorScale":2,
             "hasFraction":false,"filters":[],"orderMethods":["LIMIT","QUANTUM_ORDER"],
             "displayFormat":"#,###","commissionFromNumerator":false,"order":1,
             "priceRounding":false,"isNew":false,"marketPriceWarningThresholdPercentage":0.25}],
             "currencies":[],"currencyOperationBlocks":[]}}
            """;

        var response = JsonSerializer.Deserialize<BtcTurkResponse<BtcTurkExchangeInfo>>(
            json, BtcTurkJsonOptions.Default)!;

        Assert.True(response.Success);
        var symbol = response.Data!.Symbols.Single();

        // Bilinmeyen deger bir istisnaya yol acmaz; tanimsiz bir enum degerine dusurulur.
        Assert.False(Enum.IsDefined(symbol.Status),
            $"Bilinmeyen status tanimli bir degere eslendi: {symbol.Status}");

        // Ayni listedeki BILINEN degerler dogru sekilde okunmaya devam eder.
        Assert.Contains(OrderMethod.Limit, symbol.OrderMethods);
        Assert.Contains(symbol.OrderMethods, x => !Enum.IsDefined(x));
    }

    [Fact]
    public void Eksik_ve_null_alanlar_ayristirmayi_dusurmez()
    {
        const string json = """
            {"success":true,"code":0,"data":{"serverTime":1641916253216,"symbols":[],"currencies":[]}}
            """;

        var response = JsonSerializer.Deserialize<BtcTurkResponse<BtcTurkExchangeInfo>>(
            json, BtcTurkJsonOptions.Default)!;

        Assert.True(response.Success);
        Assert.Empty(response.Data!.Symbols);
        Assert.Null(response.Data.TimeZone);
    }
}
