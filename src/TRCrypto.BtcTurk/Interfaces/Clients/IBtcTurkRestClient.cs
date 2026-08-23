using CryptoExchange.Net.Interfaces;
using TRCrypto.BtcTurk.Interfaces.Clients.SpotApi;

namespace TRCrypto.BtcTurk.Interfaces.Clients;

/// <summary>
/// BtcTurk REST API istemcisi.
/// </summary>
/// <remarks>
/// Istemci uzun omurlu ve is parcacigi guvenli kullanim icin tasarlanmistir;
/// her istek icin yeni bir ornek olusturmayin (spesifikasyon Bolum 10.1).
/// </remarks>
public interface IBtcTurkRestClient : IRestClient
{
    /// <summary>Spot API.</summary>
    IBtcTurkRestClientSpotApi SpotApi { get; }
}
