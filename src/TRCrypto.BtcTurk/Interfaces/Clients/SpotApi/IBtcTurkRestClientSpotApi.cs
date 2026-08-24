using CryptoExchange.Net.Interfaces.Clients;

namespace TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

/// <summary>BtcTurk spot REST API'si.</summary>
public interface IBtcTurkRestClientSpotApi : IRestApiClient<BtcTurkCredentials>, IDisposable
{
    /// <summary>Piyasa verisi uclari.</summary>
    IBtcTurkRestClientSpotApiExchangeData ExchangeData { get; }

    /// <summary>
    /// Borsadan bagimsiz (shared) yuzey. Ayni kodun farkli borsalarla calismasini saglar.
    /// </summary>
    IBtcTurkRestClientSpotApiShared SharedClient { get; }
}
