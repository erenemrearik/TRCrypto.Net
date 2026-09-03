using System.Text.Json;
using TRCrypto.BtcTurk.Clients.MessageHandlers;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models.Socket;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Tum pariteleri tek abonelikle veren ticker akisini dogrular.
/// </summary>
/// <remarks>
/// Borsa bu akisi <c>401</c> mesaj koduyla gonderir ve tek parite akisindan
/// (<c>402</c>) farkli olarak govde bir liste tasir.
/// </remarks>
public class AllTickerTests
{
    private static T ParseBody<T>(string fixture)
    {
        using var document = JsonDocument.Parse(FixtureLoader.Load(fixture));
        return JsonSerializer.Deserialize<T>(document.RootElement[1].GetRawText(), BtcTurkJsonOptions.Default)!;
    }

    [Fact]
    public void Tum_pariteler_tek_govdede_cozulur()
    {
        var update = ParseBody<BtcTurkSocketTickerList>("socket-ticker-all.json");

        Assert.Equal(2, update.Items.Count);
        Assert.Equal("BTCTRY", update.Items[0].Symbol);
        Assert.Equal("ETHTRY", update.Items[1].Symbol);
    }

    [Fact]
    public void Liste_ogeleri_tek_parite_akisiyla_ayni_modeli_kullanir()
    {
        var update = ParseBody<BtcTurkSocketTickerList>("socket-ticker-all.json");
        var btc = update.Items[0];

        // Ogelerin alan semasi 402 ile aynidir; ayri bir model gerekmez.
        Assert.Equal(3913991m, btc.BestBidPrice);
        Assert.Equal(3917599m, btc.BestAskPrice);
        Assert.Equal(3913951m, btc.LastPrice);
        Assert.Equal(34.29454149m, btc.Volume);
        Assert.Equal(4.94m, btc.DailyChangePercentage);
        Assert.Equal("BTC", btc.NumeratorSymbol);
        Assert.Equal("TRY", btc.DenominatorSymbol);
    }

    [Fact]
    public void Mesaj_kodu_tek_parite_akisindan_farklidir()
    {
        // Iki akis ayri kodlar kullanir; ayni abonelige yonlendirilirlerse tekil
        // guncellemeler liste modeline cozulmeye calisilir ve akis kirilir.
        Assert.NotEqual(BtcTurkSocketMessageType.TickerPair, BtcTurkSocketMessageType.TickerAll);
        Assert.Equal(401, BtcTurkSocketMessageType.TickerAll);
        Assert.Equal(402, BtcTurkSocketMessageType.TickerPair);
    }

    [Fact]
    public void Liste_mesaji_olay_adiyla_yonlendirilir()
    {
        var handler = new BtcTurkSocketMessageHandler();
        var update = new BtcTurkSocketUpdate<BtcTurkSocketTickerList>
        {
            Type = BtcTurkSocketMessageType.TickerAll,
            Data = new BtcTurkSocketTickerList { Event = BtcTurkSocketEvent.All }
        };

        // Yonlendirme eslemesi govde tipine gore kayitlidir. Liste tipi icin esleme
        // kayitli degilse konu adi null doner, mesaj hicbir abonelige ulasmaz ve akis
        // hata vermeden sessiz kalir. Bu tam olarak yasanmis bir hatadir.
        Assert.Equal("all", handler.GetTopicFilter(update));
    }

    [Fact]
    public void Olay_adi_govdeden_okunur()
    {
        var update = ParseBody<BtcTurkSocketTickerList>("socket-ticker-all.json");

        // Kimlik istemcide tutulan bir alandan degil, mesajin kendisinden gelmelidir.
        Assert.Equal("all", update.Event);
    }

    [Fact]
    public void Abonelik_olay_adi_sabittir()
    {
        // Bos olay adi da borsa tarafindan ONAYLANIR ama hicbir guncelleme uretmez.
        // Deger canli baglantida olculmustur; degistirilirse akis sessizce durur.
        Assert.Equal("all", BtcTurkSocketEvent.All);
    }
}
