using System.Text;
using TRCrypto.BtcTurk.Clients.MessageHandlers;
using TRCrypto.BtcTurk.Objects.Internal;
using TRCrypto.BtcTurk.Objects.Models.Socket;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Gelen WebSocket mesajlarinin dogru abonelige yonlendirildigini dogrular.
/// </summary>
/// <remarks>
/// <para>
/// Yonlendirme iki asamalidir: once mesaj kodu (dizinin ilk elemani) okunur, sonra
/// govdedeki parite adiyla ilgili abonelik bulunur.
/// </para>
/// <para>
/// Bu testler gercek bir hatayi yakalar: alan derinligi bir eksik verildiginde mesaj
/// kodu hic okunamiyor, her mesaj "degerlendirilemedi" diye dusuruluyor ve abonelik
/// onayi beklenirken zaman asimina ugruyordu. Belirti (abonelik kurulmuyor) ile neden
/// (yonlendirme) arasindaki mesafe buyuk oldugu icin test dogrudan nedeni olcer.
/// </para>
/// </remarks>
public class SocketRoutingTests
{
    private static string? TypeIdentifierOf(string message)
    {
        var handler = new BtcTurkSocketMessageHandler();
        return handler.GetTypeIdentifier(Encoding.UTF8.GetBytes(message), null);
    }

    [Theory]
    [InlineData("""[402,{"PS":"BTCTRY","type":402}]""", "402")]
    [InlineData("""[421,{"symbol":"BTCTRY","type":421}]""", "421")]
    [InlineData("""[431,{"PS":"BTCTRY","type":431}]""", "431")]
    [InlineData("""[100,{"ok":true,"type":100}]""", "100")]
    public void Mesaj_kodu_dizinin_ilk_elemanindan_okunur(string message, string expected)
    {
        Assert.Equal(expected, TypeIdentifierOf(message));
    }

    [Fact]
    public void Dokumante_edilmemis_surum_mesaji_da_taninir()
    {
        // Sunucu baglanir baglanmaz bu mesaji gonderir. Taninmamasi, baglantinin
        // daha ilk mesajda sorunlu gorunmesine yol acar.
        Assert.Equal("991", TypeIdentifierOf("""[991,{"type":991,"current":"6.0.0"}]"""));
    }

    [Fact]
    public void Ticker_mesaji_parite_adiyla_yonlendirilir()
    {
        var handler = new BtcTurkSocketMessageHandler();
        var update = new BtcTurkSocketUpdate<BtcTurkSocketTicker>
        {
            Type = 402,
            Data = new BtcTurkSocketTicker { Symbol = "BTCTRY" }
        };

        Assert.Equal("BTCTRY", handler.GetTopicFilter(update));
    }

    [Fact]
    public void Emir_defteri_mesaji_parite_adiyla_yonlendirilir()
    {
        var handler = new BtcTurkSocketMessageHandler();
        var update = new BtcTurkSocketUpdate<BtcTurkSocketOrderBook>
        {
            Type = 431,
            Data = new BtcTurkSocketOrderBook { Symbol = "ETHTRY" }
        };

        Assert.Equal("ETHTRY", handler.GetTopicFilter(update));
    }

    [Theory]
    [InlineData("join|ticker:BTCTRY", "ticker:BTCTRY")]
    [InlineData("leave|orderbook:ETHTRY", "orderbook:ETHTRY")]
    public void Abonelik_onayindan_konu_adi_cikarilir(string message, string expected)
    {
        // Ayni anda birden fazla abonelik istegi gonderilebildiginden, hangi yanitin
        // hangi istege ait oldugu yalnizca bu metinden anlasilir.
        Assert.Equal(expected, BtcTurkSocketMessageHandler.ExtractTopic(message));
    }

    [Fact]
    public void Ayirici_icermeyen_onay_metni_oldugu_gibi_kullanilir()
    {
        Assert.Equal("beklenmeyen", BtcTurkSocketMessageHandler.ExtractTopic("beklenmeyen"));
    }

    [Fact]
    public void Bos_onay_metni_cokmeye_yol_acmaz()
    {
        Assert.Equal(string.Empty, BtcTurkSocketMessageHandler.ExtractTopic(null));
    }
}
