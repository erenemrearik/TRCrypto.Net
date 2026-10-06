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

    /// <summary>Borsadan bagimsiz yuzeyin ince taneli (V2) gorunumu.</summary>
    /// <remarks>
    /// <see cref="SharedClient"/> ile ayni nesnedir. Yeni kod icin tercih edilir: her islem
    /// ayri bir arayuzdur ve yalnizca ihtiyac duyulan yetenege baglanilabilir.
    /// </remarks>
    ICoinTRRestClientSpotSharedApi SharedApi { get; }
}
