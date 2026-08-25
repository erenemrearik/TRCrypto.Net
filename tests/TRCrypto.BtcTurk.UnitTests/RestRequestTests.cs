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

    [Fact]
    public async Task Account_istekleri_imzalanarak_uretilir()
    {
        // Gercekci gorunumlu SAHTE kimlik bilgileri; hicbir hesaba ait degildir.
        var client = new BtcTurkRestClient(options =>
        {
            options.RateLimiterEnabled = false;
            options.ApiCredentials = new BtcTurkCredentials(
                "FAKE-public-key", "RkFLRS1zZWNyZXQtZm9yLXVuaXQtdGVzdHMtb25seQ==");
        });

        var validator = new RestRequestValidator<BtcTurkRestClient>(
            client,
            "Endpoints/Spot/Account",
            "https://api.btcturk.com",
            IsAuthenticated,
            "data");

        // Ucun kimlik dogrulamali oldugu fixture'da isaretlidir; validator uc imzalanmadiysa
        // basarisiz olur. Boylece imzalama zincirinin gercekten devreye girdigi dogrulanir.
        //
        // Yanit dogrulamasi atlanir: validator ham JSON metnini model degeriyle birebir
        // karsilastirir ve bu ucun virgul ayiricili ondalik bicimini ("27223,72...") tanimaz.
        // Yanit eslemesi BalanceTests icinde ayrica ve daha ayrintili dogrulanmaktadir.
        await validator.ValidateAsync(
            c => c.SpotApi.Account.GetBalancesAsync(),
            "GetBalances",
            skipResponseValidation: true);
    }

    // BtcTurk imzali istekleri X-Signature basligi ile isaretler.
    private static bool IsAuthenticated(IHttpResult result)
        => result.RequestHeaders?.Any(h => h.Key == "X-Signature") == true;
}
