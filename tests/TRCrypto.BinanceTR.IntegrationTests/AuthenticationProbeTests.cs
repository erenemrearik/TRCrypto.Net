using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace TRCrypto.BinanceTR.IntegrationTests;

/// <summary>
/// Yazilmis imzalama semasinin borsa tarafindan kabul edilip edilmedigini olcer.
/// </summary>
/// <remarks>
/// <para>
/// <b>Bu testler kutuphanenin imzalama kodunu calistirmaz.</b> Kod bilincli olarak devre
/// disidir ve cagrildiginda istisna firlatir; burada sema elle uygulanip dogrudan borsaya
/// gonderilir. Amac, kodu etkinlestirmeden once semanin dogru oldugunu kanitlamaktir —
/// dogrulanmamis bir imzalama, isteklerin nedeni belirsiz sekilde reddedilmesine yol acar.
/// </para>
/// <para>
/// BtcTurk'te socket giris imzasi ayni yontemle bulunmustu: aday semalar canli hesaba
/// karsi denenip yalnizca kabul edilen sabitlenmisti.
/// </para>
/// <para>
/// Anahtar ve secret hicbir cikti satirinda yer almaz.
/// </para>
/// </remarks>
public class AuthenticationProbeTests
{
    private const string BaseAddress = "https://www.binance.tr";
    private const string AccountPath = "/open/v1/account/spot";

    private readonly ITestOutputHelper _output;

    public AuthenticationProbeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableFact]
    public async Task Imzali_hesap_istegi_kabul_ediliyor_mu()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var (statusCode, code, message) = await SendSignedAsync(recvWindowMs: 5000);

        _output.WriteLine($"HTTP {statusCode} · code={code} · msg={message}");

        // Borsa hatalari HTTP 200 icinde de dondurebilir; basari olcutu zarftaki koddur.
        Assert.True(
            code == 0,
            $"Imzali istek reddedildi (code={code}, msg='{message}'). " +
            "Sema yanlis olabilir; 3701 anahtar/IP/izin, zaman damgasi hatasi ise saat kaymasi anlamina gelir.");
    }

    [SkippableFact]
    public async Task Genis_pencere_ile_de_ayni_sonuc_aliniyor()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        // Dar pencereyle reddedilen bir istek genis pencereyle gecerse sorun imzada degil
        // saat kaymasindadir; bu ikisini birbirinden ayirir.
        var (_, narrow, _) = await SendSignedAsync(recvWindowMs: 5000);
        var (_, wide, wideMessage) = await SendSignedAsync(recvWindowMs: 60000);

        _output.WriteLine($"recvWindow 5000 → code={narrow}; recvWindow 60000 → code={wide}");

        Assert.False(
            narrow != 0 && wide == 0,
            "Istek yalnizca genis pencereyle kabul edildi: imzalama dogru, sistem saati kaymis. " +
            "Cozum: docs/credentials/binance-tr.md bolum 6.");

        Assert.True(wide == 0, $"Imzali istek genis pencereyle de reddedildi: '{wideMessage}'");
    }

    /// <summary>
    /// Yazili semayi elle uygulayarak imzali bir hesap istegi gonderir.
    /// </summary>
    /// <remarks>
    /// Sema: sorgu dizesi secret ile HMAC-SHA256'dan gecirilir, sonuc <b>onaltilik</b>
    /// metin olarak <c>signature</c> parametresine eklenir; anahtar <c>X-MBX-APIKEY</c>
    /// basligiyla gonderilir. Secret Base64 cozulmez.
    /// </remarks>
    private static async Task<(int StatusCode, int Code, string Message)> SendSignedAsync(int recvWindowMs)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var query = string.Create(
            CultureInfo.InvariantCulture,
            $"timestamp={timestamp}&recvWindow={recvWindowMs}");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(TestCredentials.ApiSecret!));
        var signature = Convert
            .ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(query)))
            .ToLowerInvariant();

        using var http = new HttpClient { BaseAddress = new Uri(BaseAddress) };
        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"{AccountPath}?{query}&signature={signature}");
        request.Headers.Add("X-MBX-APIKEY", TestCredentials.ApiKey!);

        using var response = await http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        return ((int)response.StatusCode, ReadCode(body), ReadMessage(body));
    }

    private static int ReadCode(string body)
    {
        using var document = System.Text.Json.JsonDocument.Parse(body);
        return document.RootElement.TryGetProperty("code", out var code) && code.TryGetInt32(out var value)
            ? value
            : -1;
    }

    private static string ReadMessage(string body)
    {
        using var document = System.Text.Json.JsonDocument.Parse(body);
        return document.RootElement.TryGetProperty("msg", out var message)
            ? message.GetString() ?? string.Empty
            : string.Empty;
    }
}
