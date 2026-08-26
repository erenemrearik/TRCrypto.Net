using Microsoft.Extensions.Configuration;

namespace TRCrypto.BinanceTR.IntegrationTests;

/// <summary>
/// Canli testler icin kimlik bilgilerini okur.
/// </summary>
/// <remarks>
/// <para>
/// Degerler <c>dotnet user-secrets</c> ile saklanir; depo klasorunun tamamen disinda
/// tutulduklari icin yanlislikla commit edilmeleri mumkun degildir. CI ortaminda ayni
/// degerler ortam degiskeni olarak saglanir ve ayni kod yolu calisir.
/// </para>
/// <para>
/// Kimlik bilgisi bulunamadiginda testler <b>atlanir</b>, basarisiz olmaz: anahtari
/// olmayan bir katkici da tum birim testlerini calistirabilmelidir.
/// </para>
/// <para>
/// Kurulum: <c>docs/credentials/binance-tr.md</c>
/// </para>
/// </remarks>
internal static class TestCredentials
{
    private static readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddUserSecrets(typeof(TestCredentials).Assembly, optional: true)
        .AddEnvironmentVariables()
        .Build();

    /// <summary>API anahtari; tanimli degilse <c>null</c>.</summary>
    public static string? ApiKey =>
        Get("BinanceTR:ApiKey") ?? Get("BINANCETR_API_KEY");

    /// <summary>API secret; tanimli degilse <c>null</c>.</summary>
    /// <remarks>BtcTurk'ten farkli olarak bu deger Base64 kodlu degildir, ham metindir.</remarks>
    public static string? ApiSecret =>
        Get("BinanceTR:ApiSecret") ?? Get("BINANCETR_API_SECRET");

    /// <summary>Kimlik bilgisi tanimli mi?</summary>
    public static bool Available => !string.IsNullOrWhiteSpace(ApiKey)
                                    && !string.IsNullOrWhiteSpace(ApiSecret);

    /// <summary>Testlerin atlanma nedeni; kimlik bilgisi varsa <c>null</c>.</summary>
    public static string? SkipReason => Available
        ? null
        : "Binance TR kimlik bilgisi tanimli degil. Kurulum: docs/credentials/binance-tr.md";

    /// <summary>Kimlik bilgilerinden bir <see cref="BinanceTRCredentials"/> olusturur.</summary>
    /// <exception cref="InvalidOperationException">Kimlik bilgisi tanimli degilse.</exception>
    public static BinanceTRCredentials Create()
    {
        if (!Available)
            throw new InvalidOperationException(SkipReason);

        return new BinanceTRCredentials(ApiKey!, ApiSecret!);
    }

    private static string? Get(string key)
    {
        var value = _configuration[key];
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
