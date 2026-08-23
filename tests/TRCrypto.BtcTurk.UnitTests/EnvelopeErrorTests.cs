using System.Text.Json;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// BtcTurk is mantigi hatalarini HTTP 200 icinde "success": false olarak dondurur.
/// Bu durumun basarili bir sonuc olarak yuzeye cikmamasi kritiktir (spesifikasyon Bolum 10.5).
/// </summary>
public class EnvelopeErrorTests
{
    [Fact]
    public void Http200_icindeki_is_hatasi_basarisiz_olarak_okunur()
    {
        var response = JsonSerializer.Deserialize<BtcTurkResponse<BtcTurkExchangeInfo>>(
            FixtureLoader.Load("exchangeinfo-error.json"), BtcTurkJsonOptions.Default)!;

        Assert.False(response.Success);
        Assert.Equal(1000, response.Code);
        Assert.Equal("SYSTEM_ERROR", response.Message);
        Assert.Null(response.Data);
    }

    [Fact]
    public void Bilinmeyen_hata_kodu_ham_haliyle_korunur()
    {
        const string json = """
            {"success":false,"message":"BRAND_NEW_ERROR","code":99999,"data":null}
            """;

        var response = JsonSerializer.Deserialize<BtcTurkResponse<BtcTurkExchangeInfo>>(
            json, BtcTurkJsonOptions.Default)!;

        // Bilinmeyen kod yutulmaz; consumer'a oldugu gibi tasinir.
        Assert.False(response.Success);
        Assert.Equal(99999, response.Code);
        Assert.Equal("BRAND_NEW_ERROR", response.Message);
    }
}
