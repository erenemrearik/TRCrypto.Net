using CryptoExchange.Net.Objects.Sockets;
using TRCrypto.BinanceTR.Objects.Models.Socket;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Kullanici akisi WebSocket API'si.
/// </summary>
/// <remarks>
/// <para>
/// Bu akis piyasa akisindan ayri bir sunucuda calisir ve dinleme tokeni ister. Token
/// <c>SpotApi.Account.GetListenTokenAsync</c> ile alinir.
/// </para>
/// <para>
/// <b>Token kendiliginden yenilenmez.</b> Suresi dolmadan yenisi alinip yeniden abone
/// olunmalidir; aksi halde akis hata vermeden durur. Bitis ani token ile birlikte
/// dondurulur.
/// </para>
/// </remarks>
public interface IBinanceTRSocketClientUserApi : ISocketApiClient<BinanceTRCredentials>, IDisposable
{
    /// <summary>Borsadan bagimsiz (shared) yuzey.</summary>
    IBinanceTRSocketClientUserApiShared SharedClient { get; }

    /// <summary>
    /// Hesap bakiyesindeki degisiklikleri dinler.
    /// </summary>
    /// <remarks>Yalnizca degisen varliklar gonderilir; mesaj hesabin tamamini tasimaz.</remarks>
    /// <param name="listenToken">Dinleme tokeni.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik; kapatmak icin <c>CloseAsync</c> cagrilir.</returns>
    /// <exception cref="ArgumentException">Token bos ise firlatilir.</exception>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToAccountUpdatesAsync(
        string listenToken,
        Action<DataEvent<BinanceTRStreamAccountUpdate>> onMessage,
        CancellationToken ct = default);

    /// <summary>
    /// Emirlerdeki degisiklikleri dinler.
    /// </summary>
    /// <remarks>
    /// Emir olusturma, gerceklesme ve iptal olaylari bu akistan gelir. Yon, tur ve durum
    /// burada <b>metin</b> olarak tasinir; REST tarafinda ayni bilgiler sayidir.
    /// </remarks>
    /// <param name="listenToken">Dinleme tokeni.</param>
    /// <param name="onMessage">Her guncellemede cagrilir.</param>
    /// <param name="ct">Abonelik iptal belirteci.</param>
    /// <returns>Abonelik; kapatmak icin <c>CloseAsync</c> cagrilir.</returns>
    /// <exception cref="ArgumentException">Token bos ise firlatilir.</exception>
    Task<WebSocketResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(
        string listenToken,
        Action<DataEvent<BinanceTRStreamOrderUpdate>> onMessage,
        CancellationToken ct = default);
}
