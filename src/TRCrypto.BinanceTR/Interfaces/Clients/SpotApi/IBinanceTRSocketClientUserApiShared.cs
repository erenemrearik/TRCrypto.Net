using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

/// <summary>
/// Kullanici akisinin borsadan bagimsiz (shared) yuzeyi.
/// </summary>
/// <remarks>
/// Bu arayuzler bir dinleme tokeni ister. Token borsaya ozgu parametre olarak verilir;
/// verilmediginde abonelik kurulmaz ve hata bunu acikca soyler.
/// </remarks>
public interface IBinanceTRSocketClientUserApiShared :
    ISharedClient,
    IBalanceSocketClient,
    ISpotOrderSocketClient
{
}
