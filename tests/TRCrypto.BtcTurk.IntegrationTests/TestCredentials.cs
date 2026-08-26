using Microsoft.Extensions.Configuration;

namespace TRCrypto.BtcTurk.IntegrationTests;

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
/// </remarks>
internal static class TestCredentials
{
    private static readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddUserSecrets(typeof(TestCredentials).Assembly, optional: true)
        .AddEnvironmentVariables()
        .Build();

    /// <summary>API public key; tanimli degilse <c>null</c>.</summary>
    public static string? ApiKey =>
        Get("BtcTurk:ApiKey") ?? Get("BTCTURK_API_KEY");

    /// <summary>Base64 kodlu API secret; tanimli degilse <c>null</c>.</summary>
    public static string? ApiSecret =>
        Get("BtcTurk:ApiSecret") ?? Get("BTCTURK_API_SECRET");

    /// <summary>Kimlik bilgisi tanimli mi?</summary>
    public static bool Available => !string.IsNullOrWhiteSpace(ApiKey)
                                    && !string.IsNullOrWhiteSpace(ApiSecret);

    /// <summary>
    /// Testlerin atlanma nedeni; kimlik bilgisi varsa <c>null</c>.
    /// </summary>
    /// <remarks>
    /// xUnit'in <c>Skip</c> ozelligi bu metni gosterir, boylece testin neden
    /// calismadigi rapor ciktisinda gorunur.
    /// </remarks>
    public static string? SkipReason => Available
        ? null
        : "BtcTurk kimlik bilgisi tanimli degil. Kurulum: docs/credentials/btcturk.md";

    /// <summary>Kimlik bilgilerinden bir <see cref="BtcTurkCredentials"/> olusturur.</summary>
    /// <exception cref="InvalidOperationException">Kimlik bilgisi tanimli degilse.</exception>
    public static BtcTurkCredentials Create()
    {
        if (!Available)
            throw new InvalidOperationException(SkipReason);

        return new BtcTurkCredentials(ApiKey!, ApiSecret!);
    }

    private static string? Get(string key)
    {
        var value = _configuration[key];
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
