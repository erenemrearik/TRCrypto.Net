using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;

namespace TRCrypto.BinanceTR.Objects.Sockets;

/// <summary>
/// Tek bir akisa yapilan abonelik.
/// </summary>
/// <remarks>
/// <para>
/// Binance TR akislari baglanti adresinde secilir (<c>/ws/&lt;akis&gt;</c>); ayri bir
/// abonelik mesaji gonderilmez. Bu nedenle abonelik ve abonelikten cikma sorgulari
/// bostur ve baglantinin kendisi aboneligi temsil eder.
/// </para>
/// <para>
/// Akis adindaki sembol <b>kucuk harf ve alt cizgisiz</b> olmalidir; yanlis bicim
/// hata uretmez, yalnizca hicbir mesaj gelmez.
/// </para>
/// </remarks>
/// <typeparam name="T">Gelen mesajin tipi.</typeparam>
internal class BinanceTRStreamSubscription<T> : Subscription
{
    private readonly Action<DataEvent<T>> _handler;
    private readonly string _symbol;

    /// <param name="logger">Gunluk.</param>
    /// <param name="stream">Akis adi, ornegin <c>btctry@ticker</c>.</param>
    /// <param name="typeIdentifier">
    /// Mesajlarin yonlendirme kimligi; akisin olay turudur (<c>24hrTicker</c>, <c>trade</c> …).
    /// Olay turu tasimayan akislarda sabit bir goruntu kimligi kullanilir.
    /// </param>
    /// <param name="symbol">Cagirana bildirilecek parite adi.</param>
    /// <param name="handler">Guncelleme geldiginde cagrilacak islev.</param>
    public BinanceTRStreamSubscription(
        ILogger logger,
        string stream,
        string typeIdentifier,
        string symbol,
        Action<DataEvent<T>> handler)
        : base(logger, authenticated: false)
    {
        _handler = handler;
        _symbol = symbol;

        Topic = stream;
        MessageRouter = MessageRouter.CreateForEvent<T>(typeIdentifier, HandleUpdate);
    }

    /// <inheritdoc />
    /// <remarks>Akis adres uzerinden secildigi icin ek bir sorgu gonderilmez.</remarks>
    protected override Query? GetSubQuery(SocketConnection connection) => null;

    /// <inheritdoc />
    protected override Query? GetUnsubQuery(SocketConnection connection) => null;

    private CallResult HandleUpdate(
        SocketConnection connection,
        DateTime receiveTime,
        string? originalData,
        T message)
    {
        var dataEvent = new DataEvent<T>(
            BinanceTRExchange.ExchangeName,
            message,
            receiveTime,
            originalData!)
            .WithSymbol(_symbol);

        _handler(dataEvent);

        return CallResult.Ok();
    }
}
