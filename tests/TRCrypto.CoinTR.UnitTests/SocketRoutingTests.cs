using System.Text;
using TRCrypto.CoinTR.Clients.MessageHandlers;
using Xunit;

namespace TRCrypto.CoinTR.UnitTests;

/// <summary>
/// Akis mesajlarinin dogru abonelige yonlendirildigini dogrular.
/// </summary>
/// <remarks>
/// CoinTR veri mesajlarinda kanal adi kok nesnede degil <c>arg</c> nesnesinin icindedir.
/// Yonlendirme kimligi yanlis derinlikten okunursa abonelik onay alir ama hicbir mesaj
/// ulasmaz; hata yoktur, veri yoktur. Bu yuzden derinlik testle sabitlenmistir.
/// </remarks>
public class SocketRoutingTests
{
    private static string? TypeIdentifierOf(string message)
    {
        var handler = new CoinTRSocketMessageHandler();
        return handler.GetTypeIdentifier(Encoding.UTF8.GetBytes(message), null);
    }

    [Theory]
    [InlineData("socket-ticker.json", "ticker")]
    [InlineData("socket-books.json", "books5")]
    [InlineData("socket-trade.json", "trade")]
    public void Veri_mesajlari_kanal_adiyla_yonlendirilir(string fixture, string expected)
    {
        Assert.Equal(expected, TypeIdentifierOf(FixtureLoader.Load(fixture)));
    }

    [Fact]
    public void Abonelik_onayi_olay_adiyla_yonlendirilir()
    {
        // Onay mesaji kanal adini da tasir; olay adi kanal adini ezmemelidir, cunku
        // onay sorgusuyla veri aboneligi ayri kimlikler bekler.
        Assert.Equal("subscribe", TypeIdentifierOf(FixtureLoader.Load("socket-subscribe-ack.json")));
    }

    [Fact]
    public void Farkli_kanallar_farkli_kimlik_uretir()
    {
        var ticker = TypeIdentifierOf(FixtureLoader.Load("socket-ticker.json"));
        var books = TypeIdentifierOf(FixtureLoader.Load("socket-books.json"));
        var trade = TypeIdentifierOf(FixtureLoader.Load("socket-trade.json"));

        Assert.NotEqual(ticker, books);
        Assert.NotEqual(ticker, trade);
        Assert.NotEqual(books, trade);
    }

    [Fact]
    public void Duz_metin_pong_cercevesi_ayristiriciya_ulasmaz()
    {
        // Sunucu canlilik cercevesini JSON olarak degil duz metin gonderir. Elenmezse
        // her cerceve icin ayristirma hatasi uretilir.
        Assert.True(CoinTRSocketMessageHandler.IsPongFrame(Encoding.UTF8.GetBytes("pong")));
        Assert.Null(TypeIdentifierOf("pong"));
    }

    [Fact]
    public void Json_mesajlar_pong_sanilmaz()
    {
        Assert.False(CoinTRSocketMessageHandler.IsPongFrame(
            Encoding.UTF8.GetBytes("""{"event":"subscribe"}""")));
    }
}
