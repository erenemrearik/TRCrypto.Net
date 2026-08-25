using CryptoExchange.Net.Objects;
using TRCrypto.BtcTurk.Enums;
using TRCrypto.BtcTurk.Objects.Models;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>BtcTurk spot piyasa verisi uclari. Kimlik dogrulama gerektirmez.</summary>
public interface IBtcTurkRestClientSpotApiExchangeData
{
    /// <summary>
    /// Borsanin destekledigi pariteleri, varliklari ve sunucu saatini dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/public-endpoints/exchange-info/" /></remarks>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Borsa bilgisi.</returns>
    Task<HttpResult<BtcTurkExchangeInfo>> GetExchangeInfoAsync(CancellationToken ct = default);

    /// <summary>
    /// Sunucu saatini dondurur.
    /// </summary>
    /// <remarks>
    /// BtcTurk ayri bir sunucu saati ucu sunmaz; deger exchange info yanitindan alinir.
    /// </remarks>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Sunucu saati (UTC).</returns>
    Task<HttpResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default);

    /// <summary>
    /// Tum pariteler icin ozet fiyat bilgisini dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/public-endpoints/ticker/" /></remarks>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Ozet fiyat bilgileri.</returns>
    Task<HttpResult<IReadOnlyList<BtcTurkTicker>>> GetTickersAsync(CancellationToken ct = default);

    /// <summary>
    /// Tek bir parite icin ozet fiyat bilgisini dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/public-endpoints/ticker/" /></remarks>
    /// <param name="symbol">Native sembol adi, ornegin <c>BTCTRY</c>.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Ozet fiyat bilgisi.</returns>
    Task<HttpResult<BtcTurkTicker>> GetTickerAsync(string symbol, CancellationToken ct = default);

    /// <summary>
    /// Belirtilen quote varliga sahip paritelerin ozet fiyat bilgisini dondurur.
    /// </summary>
    /// <param name="quoteAsset">Quote varlik, ornegin <c>TRY</c>.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Ozet fiyat bilgileri.</returns>
    Task<HttpResult<IReadOnlyList<BtcTurkTicker>>> GetTickersByQuoteAssetAsync(
        string quoteAsset,
        CancellationToken ct = default);

    /// <summary>
    /// Bir parite icin emir defteri goruntusunu dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/public-endpoints/orderbook/" /></remarks>
    /// <param name="symbol">Native sembol adi, ornegin <c>BTCTRY</c>.</param>
    /// <param name="limit">Dondurulecek kademe sayisi; belirtilmezse borsa varsayilani (25) kullanilir.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Emir defteri.</returns>
    Task<HttpResult<BtcTurkOrderBook>> GetOrderBookAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default);

    /// <summary>
    /// Bir parite icin son islemleri dondurur.
    /// </summary>
    /// <remarks><see href="https://docs.btcturk.com/docs/public-endpoints/trades/" /></remarks>
    /// <param name="symbol">Native sembol adi, ornegin <c>BTCTRY</c>.</param>
    /// <param name="limit">Dondurulecek islem sayisi; en fazla 50 olabilir.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Son islemler.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="limit"/> 1 ile 50 araliginin disindaysa firlatilir.
    /// </exception>
    Task<HttpResult<IReadOnlyList<BtcTurkTrade>>> GetTradesAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default);

    /// <summary>
    /// Bir parite icin mum (OHLC) verisi dondurur.
    /// </summary>
    /// <remarks>
    /// <para><see href="https://docs.btcturk.com/docs/public-endpoints/get-kline-data/" /></para>
    /// <para>
    /// Bu uc ayri bir host uzerinde calisir ve diger uclarin standart yanit zarfini
    /// kullanmaz; ayrica zaman damgalarini saniye cinsinden dondurur.
    /// </para>
    /// </remarks>
    /// <param name="symbol">Native sembol adi, ornegin <c>BTCTRY</c>.</param>
    /// <param name="interval">Mum araligi.</param>
    /// <param name="startTime">Baslangic zamani.</param>
    /// <param name="endTime">Bitis zamani.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Zaman sirasini koruyan mum listesi.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<HttpResult<IReadOnlyList<BtcTurkKline>>> GetKlinesAsync(
        string symbol,
        KlineInterval interval,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken ct = default);
}
