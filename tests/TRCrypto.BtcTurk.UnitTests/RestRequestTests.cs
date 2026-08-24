using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing;
using TRCrypto.BtcTurk.Clients;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Uretilen HTTP isteginin metod/yol/parametrelerini ve yanit eslemesini dogrular
/// (spesifikasyon Bolum 15.1 "Request contract").
/// </summary>
public class RestRequestTests
{
    [Fact]
    public async Task ExchangeData_istekleri_dogru_uretilir()
    {
        var client = new BtcTurkRestClient(options => options.RateLimiterEnabled = false);

        var validator = new RestRequestValidator<BtcTurkRestClient>(
            client,
            "Endpoints/Spot/ExchangeData",
            "https://api.btcturk.com",
            IsAuthenticated,
            "data");

        await validator.ValidateAsync(
            c => c.SpotApi.ExchangeData.GetExchangeInfoAsync(),
            "GetExchangeInfo");

        await validator.ValidateAsync(
            c => c.SpotApi.ExchangeData.GetTickersAsync(),
            "GetTicker");

        await validator.ValidateAsync(
            c => c.SpotApi.ExchangeData.GetOrderBookAsync("BTCTRY", 3),
            "GetOrderBook");

        await validator.ValidateAsync(
            c => c.SpotApi.ExchangeData.GetTradesAsync("BTCTRY", 2),
            "GetTrades");
    }

    // BtcTurk imzali istekleri X-Signature basligi ile isaretler.
    private static bool IsAuthenticated(IHttpResult result)
        => result.RequestHeaders?.Any(h => h.Key == "X-Signature") == true;
}
