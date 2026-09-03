using System.Text;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using TRCrypto.BinanceTR.Clients;
using TRCrypto.BinanceTR.Clients.SpotApi;
using TRCrypto.BinanceTR;
using Xunit;

namespace TRCrypto.BinanceTR.UnitTests;

/// <summary>
/// Istek imzalamayi dogrular.
/// </summary>
/// <remarks>
/// <para>
/// Imza yanlis hesaplandiginda borsa istegi reddeder ama nedenini soylemez: ayni hata
/// kodu yanlis anahtar, listede olmayan IP ve eksik izin icin de doner. Bu yuzden
/// dogrulama sabit test vektoruyle yapilir.
/// </para>
/// <para>
/// Vektor Binance dokumantasyonunda yayimlanan ornektir; anahtar ve secret gercek degildir.
/// </para>
/// </remarks>
public class AuthenticationTests
{
    private const string SampleKey = "vmPUZE6mv9SD5VNHk4HlWFsOr6aKE2zvsw0MuIgwCIPy6utIco14y7Ju91duEh8A";
    private const string SampleSecret = "NhqPtmdSJYdKjVHjA7PZj4Mge3R5YNiP1e3UZjInClVN65XAbvqqM6A7H5fATj0j";

    private static BinanceTRAuthenticationProvider Provider()
        => new(new BinanceTRCredentials(SampleKey, SampleSecret));

    /// <summary>
    /// Imzalama, sunucu saat farkini istemciden okur; bu yuzden gercek bir istemci gerekir.
    /// </summary>
    private static RestApiClient ApiClient()
        => (RestApiClient)new BinanceTRRestClient(o => o.RateLimiterEnabled = false).SpotApi;

    [Fact]
    public void Imza_yayimlanmis_test_vektoruyle_ayni()
    {
        const string payload =
            "symbol=LTCBTC&side=BUY&type=LIMIT&timeInForce=GTC&quantity=1&price=0.1"
            + "&recvWindow=5000&timestamp=1499827319559";

        Assert.Equal(
            "c8db56825ae71d6d79447849e617115f4a920fa2acdcab2b053c4b2838bd6b71",
            Provider().CreateSignature(payload));
    }

    [Fact]
    public void Imza_onaltilik_ve_kucuk_harf()
    {
        var signature = Provider().CreateSignature("timestamp=1");

        Assert.Equal(64, signature.Length);
        Assert.Matches("^[0-9a-f]+$", signature);
    }

    [Fact]
    public void Secret_base64_cozulmez()
    {
        // BtcTurk'un secret'i Base64'tur ve cozulerek kullanilir. Ayni islemi burada
        // uygulamak sessizce yanlis imza uretir; iki borsa ayni yardimci kodu paylasamaz.
        var raw = new BinanceTRAuthenticationProvider(
            new BinanceTRCredentials(SampleKey, "dGVzdA=="));
        var literal = new BinanceTRAuthenticationProvider(
            new BinanceTRCredentials(SampleKey, "test"));

        Assert.NotEqual(literal.CreateSignature("timestamp=1"), raw.CreateSignature("timestamp=1"));
    }

    [Fact]
    public void Kimlik_dogrulama_gerektirmeyen_istek_imzalanmaz()
    {
        var config = Configuration(HttpMethod.Get, "/open/v1/common/symbols", authenticated: false);

        Provider().ProcessRequest(ApiClient(), config);

        Assert.DoesNotContain("X-MBX-APIKEY", config.Headers!.Keys);
        Assert.DoesNotContain(config.QueryParameters!, x => x.Key == "signature");
    }

    [Fact]
    public void Imzali_istek_anahtari_baslikta_gonderir()
    {
        var config = Configuration(HttpMethod.Get, "/open/v1/account/spot", authenticated: true);

        Provider().ProcessRequest(ApiClient(), config);

        Assert.Equal(SampleKey, config.Headers!["X-MBX-APIKEY"]);
    }

    [Fact]
    public void Imzali_istege_zaman_damgasi_ve_imza_eklenir()
    {
        var config = Configuration(HttpMethod.Get, "/open/v1/account/spot", authenticated: true);

        Provider().ProcessRequest(ApiClient(), config);

        Assert.Contains(config.QueryParameters!, x => x.Key == "timestamp");
        Assert.Contains(config.QueryParameters!, x => x.Key == "signature");
    }

    [Fact]
    public void Imza_kendisi_disindaki_tum_parametreler_uzerinden_hesaplanir()
    {
        var config = Configuration(HttpMethod.Get, "/open/v1/orders", authenticated: true);
        config.QueryParameters!.Add("symbol", "BTC_TRY");

        Provider().ProcessRequest(ApiClient(), config);

        // Gonderilen istegin imza disindaki parametreleri, imzalanan yukun kendisidir.
        // Bu ikisi ayrisirsa sunucu istegi reddeder ve nedenini soylemez.
        var signature = (string)config.QueryParameters["signature"]!;
        var signed = new Parameters(BinanceTRExchange.ParameterSettings);
        foreach (var parameter in config.QueryParameters.Where(x => x.Key != "signature"))
            signed.Add(parameter.Key, parameter.Value);

        Assert.Equal(
            Provider().CreateSignature(signed.CreateParamString(true, config.ArraySerialization)),
            signature);
    }

    [Fact]
    public void Imzalanan_yuk_govdeyi_de_kapsar()
    {
        // Emir olusturma parametreleri govdede gider; yalnizca sorgu dizesi imzalanirsa
        // emirlerin tamami reddedilir.
        var config = Configuration(
            HttpMethod.Post, "/open/v1/orders", authenticated: true,
            position: HttpMethodParameterPosition.InBody);
        config.BodyParameters!.Add("symbol", "BTC_TRY");
        config.BodyParameters.Add("side", "0");

        Provider().ProcessRequest(ApiClient(), config);

        var signature = (string)config.BodyParameters["signature"]!;
        var signed = new Parameters(BinanceTRExchange.ParameterSettings);
        foreach (var parameter in config.BodyParameters.Where(x => x.Key != "signature"))
            signed.Add(parameter.Key, parameter.Value);

        Assert.Equal(
            Provider().CreateSignature(signed.CreateParamString(true, config.ArraySerialization)),
            signature);
    }

    private static RestRequestConfiguration Configuration(
        HttpMethod method,
        string path,
        bool authenticated,
        HttpMethodParameterPosition position = HttpMethodParameterPosition.InUri)
        => new(
            new RequestDefinition(path, path, method) { Authenticated = authenticated },
            new Parameters(BinanceTRExchange.ParameterSettings),
            new Parameters(BinanceTRExchange.ParameterSettings),
            new Dictionary<string, string>(),
            position,
            RequestBodyFormat.FormData);

    [Fact]
    public void Parametreler_siralanir_ve_imza_ayni_sirayi_kullanir()
    {
        // Kutuphane parametreleri sirali tutar; istegin uzerindeki dize de bu sirayla
        // olusur. Imzayi elle kurulan bir dizeden hesaplamak, iki sirayi ayristirir.
        var config = Configuration(HttpMethod.Get, "/open/v1/orders", authenticated: true);
        config.QueryParameters!.Add("symbol", "BTC_TRY");
        config.QueryParameters.Add("limit", "10");

        Provider().ProcessRequest(ApiClient(), config);

        var keys = config.QueryParameters.Select(x => x.Key).ToArray();
        Assert.Equal(keys.OrderBy(x => x, StringComparer.Ordinal), keys);
    }
}
