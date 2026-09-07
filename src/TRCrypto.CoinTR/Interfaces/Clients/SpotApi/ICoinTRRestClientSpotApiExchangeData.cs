using TRCrypto.CoinTR.Objects.Models;

namespace TRCrypto.CoinTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Herkese acik piyasa verisi uclari.
/// </summary>
/// <remarks>Hicbiri API anahtari gerektirmez.</remarks>
public interface ICoinTRRestClientSpotApiExchangeData
{
    /// <summary>Sunucu saatini getirir.</summary>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Sunucu saati.</returns>
    Task<HttpResult<DateTime>> GetServerTimeAsync(CancellationToken ct = default);

    /// <summary>Islem yapilabilir pariteleri getirir.</summary>
    /// <remarks>
    /// Base ve quote varlik ayri alanlarda gelir; sembol adi ayristirilmaz. Komisyon
    /// oranlari parite basina bildirilir.
    /// </remarks>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Pariteler.</returns>
    Task<HttpResult<CoinTRSymbol[]>> GetSymbolsAsync(CancellationToken ct = default);

    /// <summary>Bir paritenin ya da tum paritelerin ozet fiyat bilgisini getirir.</summary>
    /// <remarks>Yanit tek parite istendiginde bile dizidir.</remarks>
    /// <param name="symbol">Native parite adi; verilmezse tum pariteler doner.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Ticker verisi.</returns>
    Task<HttpResult<CoinTRTicker[]>> GetTickersAsync(string? symbol = null, CancellationToken ct = default);

    /// <summary>Emir defterini getirir.</summary>
    /// <param name="symbol">Native parite adi.</param>
    /// <param name="limit">Kademe sayisi.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Emir defteri.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<HttpResult<CoinTROrderBook>> GetOrderBookAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default);

    /// <summary>Son islemleri getirir.</summary>
    /// <param name="symbol">Native parite adi.</param>
    /// <param name="limit">Dondurulecek en fazla islem sayisi.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Islemler.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<HttpResult<CoinTRTrade[]>> GetTradesAsync(
        string symbol,
        int? limit = null,
        CancellationToken ct = default);

    /// <summary>Mum verisini getirir.</summary>
    /// <remarks>Mumlar isimsiz dizi olarak gelir; sira anlamin tek tasiyicisidir.</remarks>
    /// <param name="symbol">Native parite adi.</param>
    /// <param name="interval">Mum araligi.</param>
    /// <param name="startTime">Bu andan sonraki mumlar.</param>
    /// <param name="endTime">Bu andan onceki mumlar.</param>
    /// <param name="limit">Dondurulecek en fazla mum sayisi.</param>
    /// <param name="ct">Iptal belirteci.</param>
    /// <returns>Mumlar.</returns>
    /// <exception cref="ArgumentException">Sembol bos ise firlatilir.</exception>
    Task<HttpResult<CoinTRKline[]>> GetKlinesAsync(
        string symbol,
        KlineInterval interval,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? limit = null,
        CancellationToken ct = default);
}
