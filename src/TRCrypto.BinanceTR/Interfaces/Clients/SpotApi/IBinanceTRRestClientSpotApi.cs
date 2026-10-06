namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>Binance TR spot REST API'si.</summary>
public interface IBinanceTRRestClientSpotApi : IRestApiClient<BinanceTRCredentials>, IDisposable
{
    /// <summary>Piyasa verisi uclari.</summary>
    IBinanceTRRestClientSpotApiExchangeData ExchangeData { get; }

    /// <summary>
    /// Hesap bilgisi uclari. API anahtari gerektirir.
    /// </summary>
    IBinanceTRRestClientSpotApiAccount Account { get; }

    /// <summary>
    /// Emir uclari. API anahtari gerektirir.
    /// </summary>
    IBinanceTRRestClientSpotApiTrading Trading { get; }

    /// <summary>Borsadan bagimsiz (shared) yuzey.</summary>
    IBinanceTRRestClientSpotApiShared SharedClient { get; }

    /// <summary>Borsadan bagimsiz yuzeyin ince taneli (V2) gorunumu.</summary>
    /// <remarks>
    /// <see cref="SharedClient"/> ile ayni nesnedir. Yeni kod icin tercih edilir: her islem
    /// ayri bir arayuzdur ve yalnizca ihtiyac duyulan yetenege baglanilabilir.
    /// </remarks>
    IBinanceTRRestClientSpotSharedApi SharedApi { get; }
}
