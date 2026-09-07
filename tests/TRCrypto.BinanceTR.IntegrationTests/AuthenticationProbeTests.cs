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
/// <b>Bu testler kutuphanenin imzalama kodunu calistirmaz.</b> Sema burada elle uygulanip
/// dogrudan borsaya gonderilir. Amac, kodu guvenle etkinlestirebilmek icin semanin canli
/// hesapta kabul edildigini kanitlamaktir.
/// </para>
/// <para>
/// Borsa reddin nedenini tek bir mesajda toplamaz; uc ayri kod kullanir ve bu kodlarin ne
/// anlama geldigi <see cref="BinanceTRErrorCodes"/> icinde, kontrollu denemeyle olculmustur.
/// Ayrim onemlidir: yanlis anahtar, kayan saat ve yanlis imza tamamen farkli islere yol acar.
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
    public async Task Anahtar_borsa_tarafindan_taniniyor()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var (_, code, message) = await SendSignedAsync(recvWindowMs: 5000);

        _output.WriteLine($"code={code} · {message}");

        // Bu kodlar anahtarin kendisiyle ilgilidir ve imzadan bagimsizdir. Anahtar
        // tanindiginda borsa imzayi degerlendirmeye gecer ve baska bir kod dondurur.
        Assert.False(
            code == BinanceTRErrorCodes.MissingKey,
            "Anahtar gonderilmedi ya da bos.");

        Assert.False(
            code == BinanceTRErrorCodes.UnknownKeyOrPermission,
            "Anahtar taninmiyor, IP listede degil ya da izinler eksik. Bu kod uc nedeni " +
            "birden kapsar; once IP kisitlamasini ve anahtarin okuma iznini kontrol edin.");
    }

    [SkippableFact]
    public async Task Imzali_hesap_istegi_kabul_ediliyor_mu()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var (statusCode, code, message) = await SendSignedAsync(recvWindowMs: 5000);

        _output.WriteLine($"HTTP {statusCode} · code={code} · {message}");

        Assert.True(
            code == 0,
            $"Imzali istek reddedildi (code={code}, msg='{message}'). {Diagnose(code)}");
    }

    [SkippableFact]
    public async Task Genis_pencere_ile_de_ayni_sonuc_aliniyor()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        // Dar pencereyle reddedilen bir istek genis pencereyle gecerse sorun imzada degil
        // saat kaymasindadir; bu iki nedeni birbirinden ayirir.
        var (_, narrow, _) = await SendSignedAsync(recvWindowMs: 5000);
        var (_, wide, wideMessage) = await SendSignedAsync(recvWindowMs: 60000);

        _output.WriteLine($"recvWindow 5000 → code={narrow}; recvWindow 60000 → code={wide}");

        Assert.False(
            narrow != 0 && wide == 0,
            "Istek yalnizca genis pencereyle kabul edildi: imzalama dogru, sistem saati kaymis. " +
            "Cozum: docs/credentials/binance-tr.md bolum 6.");

        Assert.True(wide == 0, $"Imzali istek genis pencereyle de reddedildi: '{wideMessage}'. {Diagnose(wide)}");
    }

    /// <summary>
    /// Bir ret kodunu, o kodun gercekte ne anlama geldigi olculerek yazilmis bir aciklamaya cevirir.
    /// </summary>
    private static string Diagnose(int code) => code switch
    {
        BinanceTRErrorCodes.MissingKey =>
            "Anahtar gonderilmemis.",

        BinanceTRErrorCodes.UnknownKeyOrPermission =>
            "Borsa anahtari tanimiyor, IP listede degil ya da izin eksik.",

        BinanceTRErrorCodes.InvalidSignature =>
            "Anahtar taniniyor ancak imza eslesmiyor. Kontrollu deneme, bu kodun yanlis " +
            "secret ile dogru secret arasinda ayrim yapmadigini gosterdi: ikisi de ayni " +
            "kodu uretir. En olasi neden, secret'in anahtarla ayni cifte ait olmamasidir; " +
            "anahtar ve secret'in birlikte uretildigi ciftten geldigini dogrulayin.",

        _ => "Beklenmeyen kod; borsanin hata listesine bakin."
    };

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
        foreach (var name in new[] { "msg", "message" })
        {
            if (document.RootElement.TryGetProperty(name, out var message))
                return message.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}

/// <summary>
/// Kimlik dogrulama reddinde donen kodlar.
/// </summary>
/// <remarks>
/// Anlamlari resmi dokumantasyonda yazmiyor; 7 Eylul 2026'da kontrollu denemeyle
/// olculdu. Bilerek gecersiz bir anahtarla ve gecerli anahtar ile gecersiz secret ile
/// ayni istek gonderildi ve donen kodlar karsilastirildi.
/// </remarks>
internal static class BinanceTRErrorCodes
{
    /// <summary>Anahtar hic gonderilmemis.</summary>
    public const int MissingKey = 3700;

    /// <summary>Anahtar taninmiyor, IP listede degil ya da izin eksik.</summary>
    public const int UnknownKeyOrPermission = 3701;

    /// <summary>
    /// Anahtar taniniyor ancak imza eslesmiyor.
    /// </summary>
    /// <remarks>
    /// Bu kod yalnizca imzayla ilgilidir. Anahtarin gecerliligini kanitlar: gecersiz bir
    /// anahtar <see cref="UnknownKeyOrPermission"/> uretir.
    /// </remarks>
    public const int InvalidSignature = 3702;
}
