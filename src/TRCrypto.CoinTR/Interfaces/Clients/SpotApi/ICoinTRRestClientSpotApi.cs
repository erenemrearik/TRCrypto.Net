namespace TRCrypto.CoinTR.Interfaces.Clients.SpotApi;

/// <summary>
/// CoinTR spot REST API'si.
/// </summary>
/// <remarks>
/// Bu surum yalnizca herkese acik piyasa verisi sunar. Imzalama yazilmistir ancak hesap ve
/// emir uclari, canli bir hesaba karsi dogrulanmadan eklenmeyecektir.
/// </remarks>
public interface ICoinTRRestClientSpotApi : IRestApiClient<CoinTRCredentials>, IDisposable
{
    /// <summary>Piyasa verisi uclari.</summary>
    ICoinTRRestClientSpotApiExchangeData ExchangeData { get; }

    /// <summary>Borsadan bagimsiz (shared) yuzey.</summary>
    ICoinTRRestClientSpotApiShared SharedClient { get; }
}
