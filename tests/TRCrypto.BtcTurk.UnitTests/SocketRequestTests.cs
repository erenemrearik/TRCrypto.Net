using System.Text.Json;
using TRCrypto.BtcTurk.Objects.Internal;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Giden WebSocket mesajlarinin borsanin bekledigi bicimde uretildigini dogrular.
/// </summary>
/// <remarks>
/// Beklenen bicim canli bir baglantida denenerek dogrulanmistir; sunucu bu mesaja
/// <c>[100,{"type":100,"ok":true,"message":"join|ticker:BTCTRY"}]</c> yaniti vermistir.
/// </remarks>
public class SocketRequestTests
{
    [Fact]
    public void Abonelik_istegi_dizi_bicimimde_uretilir()
    {
        var request = new BtcTurkSocketRequest
        {
            Channel = "ticker",
            Event = "BTCTRY",
            Join = true
        };

        var json = JsonSerializer.Serialize(request);

        Assert.Equal(
            """[151,{"type":151,"channel":"ticker","event":"BTCTRY","join":true}]""",
            json);
    }

    [Fact]
    public void Abonelikten_cikma_istegi_join_alanini_false_yapar()
    {
        var request = new BtcTurkSocketRequest
        {
            Channel = "trade",
            Event = "BTCTRY",
            Join = false
        };

        var json = JsonSerializer.Serialize(request);

        Assert.Equal(
            """[151,{"type":151,"channel":"trade","event":"BTCTRY","join":false}]""",
            json);
    }

    [Fact]
    public void Onay_mesaji_cozulur()
    {
        const string json = """{"type":100,"ok":true,"message":"join|ticker:BTCTRY"}""";

        var result = JsonSerializer.Deserialize<BtcTurkSocketResult>(json, BtcTurkJsonOptions.Default)!;

        Assert.True(result.Ok);
        Assert.Equal(100, result.Type);
        Assert.Equal("join|ticker:BTCTRY", result.Message);
    }

    [Fact]
    public void Basarisiz_onay_mesaji_da_cozulur()
    {
        const string json = """{"type":100,"ok":false,"message":"invalid channel"}""";

        var result = JsonSerializer.Deserialize<BtcTurkSocketResult>(json, BtcTurkJsonOptions.Default)!;

        Assert.False(result.Ok);
        Assert.Equal("invalid channel", result.Message);
    }
}
