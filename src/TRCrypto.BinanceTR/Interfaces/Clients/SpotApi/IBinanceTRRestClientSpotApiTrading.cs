using TRCrypto.BinanceTR.Enums;
using TRCrypto.BinanceTR.Objects.Models;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Emir uclari.
/// </summary>
/// <remarks>
/// Tum uclar API anahtari ister ve imzalanir. Emir vermek icin anahtarin alim satim
/// izni acik olmalidir; cekim izni hicbir uc icin gerekmez ve acilmamalidir.
/// </remarks>
public interface IBinanceTRRestClientSpotApiTrading
{
    /// <summary>
    /// Yeni bir emir olusturur.
    /// </summary>
    /// <remarks>
    /// Yanit yalnizca emir kimligini ve olusturulma anini tasir; emrin durumu icin
    /// <see cref="GetOrderAsync"/> cagrilmalidir.
    /// </remarks>
    /// <param name="symbol">Native parite adi, ornegin <c>BTC_TRY</c>.</param>
    /// <param name="side">Emrin yonu.</param>
    /// <param name="type">Emrin turu.</param>
    /// <param name="quantity">Base varlik cinsinden miktar.</param>
    /// <param name="quoteQuantity">
    /// Quote varlik cinsinden tutar. Piyasa alis emirlerinde miktar yerine kullanilabilir.
    /// </param>
    /// <param name="price">Limit fiyati. Limit emirlerinde zorunludur.</param>
    /// <param name="clientOrderId">Cagiran tarafin verdigi kimlik.</param>
    /// <param name="stopPrice">Tetikleme fiyati.</param>
    /// <param name="icebergQuantity">Buzdagi emirlerde gorunen miktar.</param>
    /// <param name="timeInForce">Emrin gecerlilik suresi.</param>
    /// <param name="receiveWindow">Istegin gecerli sayilacagi sure (ms).</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Olusturulan emrin kimligi.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<HttpResult<BinanceTRPlacedOrder>> PlaceOrderAsync(
        string symbol,
        OrderSide side,
        OrderType type,
        decimal? quantity = null,
        decimal? quoteQuantity = null,
        decimal? price = null,
        string? clientOrderId = null,
        decimal? stopPrice = null,
        decimal? icebergQuantity = null,
        TimeInForce? timeInForce = null,
        long? receiveWindow = null,
        CancellationToken ct = default);

    /// <summary>
    /// Tek bir emri getirir.
    /// </summary>
    /// <remarks>Emir kimligi ya da cagiran tarafin verdigi kimlikten biri verilmelidir.</remarks>
    /// <param name="orderId">Emir kimligi.</param>
    /// <param name="clientOrderId">Cagiran tarafin verdigi kimlik.</param>
    /// <param name="receiveWindow">Istegin gecerli sayilacagi sure (ms).</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Emrin ayrintisi.</returns>
    /// <exception cref="ArgumentException">Iki kimlik de verilmediyse firlatilir.</exception>
    Task<HttpResult<BinanceTROrder>> GetOrderAsync(
        long? orderId = null,
        string? clientOrderId = null,
        long? receiveWindow = null,
        CancellationToken ct = default);

    /// <summary>
    /// Bir paritenin emirlerini getirir.
    /// </summary>
    /// <param name="symbol">Native parite adi.</param>
    /// <param name="side">Yalnizca bu yondeki emirler.</param>
    /// <param name="type">Yalnizca bu turdeki emirler.</param>
    /// <param name="startTime">Bu andan sonraki emirler.</param>
    /// <param name="endTime">Bu andan onceki emirler.</param>
    /// <param name="fromId">Bu emir kimliginden itibaren.</param>
    /// <param name="limit">Dondurulecek en fazla emir sayisi.</param>
    /// <param name="receiveWindow">Istegin gecerli sayilacagi sure (ms).</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Emirler.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<HttpResult<BinanceTROrderList>> GetOrdersAsync(
        string symbol,
        OrderSide? side = null,
        OrderType? type = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        long? fromId = null,
        int? limit = null,
        long? receiveWindow = null,
        CancellationToken ct = default);

    /// <summary>
    /// Bir emri iptal eder.
    /// </summary>
    /// <param name="orderId">Emir kimligi.</param>
    /// <param name="clientOrderId">Cagiran tarafin verdigi kimlik.</param>
    /// <param name="receiveWindow">Istegin gecerli sayilacagi sure (ms).</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Iptal edilen emrin son hali.</returns>
    /// <exception cref="ArgumentException">Iki kimlik de verilmediyse firlatilir.</exception>
    Task<HttpResult<BinanceTROrder>> CancelOrderAsync(
        long? orderId = null,
        string? clientOrderId = null,
        long? receiveWindow = null,
        CancellationToken ct = default);

    /// <summary>
    /// Hesabin gerceklesen islemlerini getirir.
    /// </summary>
    /// <param name="symbol">Native parite adi.</param>
    /// <param name="orderId">Yalnizca bu emre ait islemler.</param>
    /// <param name="startTime">Bu andan sonraki islemler.</param>
    /// <param name="endTime">Bu andan onceki islemler.</param>
    /// <param name="fromId">Bu islem kimliginden itibaren.</param>
    /// <param name="limit">Dondurulecek en fazla islem sayisi; varsayilan 500, en fazla 1000.</param>
    /// <param name="receiveWindow">Istegin gecerli sayilacagi sure (ms).</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Islemler.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<HttpResult<BinanceTRUserTradeList>> GetUserTradesAsync(
        string symbol,
        long? orderId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        long? fromId = null,
        int? limit = null,
        long? receiveWindow = null,
        CancellationToken ct = default);
}
