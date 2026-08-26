namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>Binance TR spot REST API'si.</summary>
public interface IBinanceTRRestClientSpotApi : IRestApiClient<BinanceTRCredentials>, IDisposable
{
    /// <summary>Piyasa verisi uclari.</summary>
    IBinanceTRRestClientSpotApiExchangeData ExchangeData { get; }
}
