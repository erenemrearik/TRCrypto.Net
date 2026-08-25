using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Enums;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>
/// BtcTurk emir uclari. Kimlik dogrulama gerektirir.
/// </summary>
/// <remarks>
/// <para>
/// Sorgulama uclari icin API anahtarinizda <c>Toplam Varlik</c>, emir olusturma ve iptal
/// icin <c>Al-Sat</c> izni acik olmalidir.
/// </para>
/// <para>
/// <b>Bu uclar gercek para hareketi yaratir.</b> Emir olusturma istekleri zaman asimina
/// ugradiginda otomatik olarak yeniden denenmez; emir borsada olusmus olabilecegi icin
/// once <see cref="GetOpenOrdersAsync"/> ya da <see cref="GetOrderAsync"/> ile durum
/// dogrulanmalidir.
/// </para>
/// </remarks>
public interface IBtcTurkRestClientSpotApiTrading
{
    /// <summary>
    /// Acik emirleri dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/private-endpoints/open-orders/" /></remarks>
    /// <param name="symbol">Native sembol adi; belirtilmezse tum pariteler dondurulur.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Alis ve satis olarak ayrilmis acik emirler.</returns>
    Task<HttpResult<BtcTurkOpenOrders>> GetOpenOrdersAsync(
        string? symbol = null,
        CancellationToken ct = default);

    /// <summary>
    /// Emir gecmisini dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/private-endpoints/all-orders/" /></remarks>
    /// <param name="symbol">Native sembol adi.</param>
    /// <param name="startTime">Baslangic zamani.</param>
    /// <param name="endTime">Bitis zamani.</param>
    /// <param name="fromOrderId">Bu kimlikten buyuk ya da esit emirler dondurulur.</param>
    /// <param name="page">Sayfa numarasi.</param>
    /// <param name="limit">Sayfa basina kayit sayisi; en fazla 1000, varsayilan 100.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Emirler.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="limit"/> 1 ile 1000 araliginin disindaysa firlatilir.
    /// </exception>
    Task<HttpResult<IReadOnlyList<BtcTurkOrder>>> GetOrdersAsync(
        string? symbol = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        long? fromOrderId = null,
        int? page = null,
        int? limit = null,
        CancellationToken ct = default);

    /// <summary>
    /// Tek bir emri kimligine gore dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/private-endpoints/get-single-order/" /></remarks>
    /// <param name="orderId">Emir kimligi.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Emir bilgisi.</returns>
    Task<HttpResult<BtcTurkOrder>> GetOrderAsync(long orderId, CancellationToken ct = default);

    /// <summary>
    /// Yeni bir emir olusturur.
    /// </summary>
    /// <remarks>
    /// <para><see href="https://docs.btcturk.com/docs/private-endpoints/submit-order/" /></para>
    /// <para>
    /// <b>Bu cagri gercek para hareketi yaratir.</b> Zaman asimi durumunda istek otomatik
    /// olarak yeniden denenmez; emir borsada olusmus olabilir.
    /// </para>
    /// </remarks>
    /// <param name="symbol">Native sembol adi, ornegin <c>BTCTRY</c>.</param>
    /// <param name="side">Emrin yonu.</param>
    /// <param name="method">Emrin yontemi.</param>
    /// <param name="quantity">Emir miktari (base varlik cinsinden).</param>
    /// <param name="price">Limit emirlerinde emir fiyati. Piyasa emirlerinde yok sayilir.</param>
    /// <param name="stopPrice">Stop emirlerinde tetikleme fiyati.</param>
    /// <param name="clientOrderId">Cagiran tarafin verdigi emir kimligi.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Olusturulan emir.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Miktar ya da fiyat pozitif degilse, ya da emir yonteminin gerektirdigi bir deger
    /// eksikse firlatilir.
    /// </exception>
    Task<HttpResult<BtcTurkOrderPlacement>> PlaceOrderAsync(
        string symbol,
        OrderSide side,
        OrderMethod method,
        decimal quantity,
        decimal? price = null,
        decimal? stopPrice = null,
        string? clientOrderId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Bir emri iptal eder.
    /// </summary>
    /// <remarks>
    /// <para><see href="https://docs.btcturk.com/docs/private-endpoints/cancel-order/" /></para>
    /// <para>
    /// <b>Basarili yanit iptalin tamamlandigi anlamina gelmez.</b> Borsa istegi aldigini
    /// bildirir; iptalin kesinlesmesi WebSocket uzerinden duyurulur. Emrin gercekten
    /// iptal edildigini dogrulamak icin durumunu ayrica sorgulayin.
    /// </para>
    /// </remarks>
    /// <param name="orderId">Iptal edilecek emrin kimligi.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Istegin borsa tarafindan kabul edilip edilmedigi.</returns>
    Task<HttpResult> CancelOrderAsync(long orderId, CancellationToken ct = default);
}
