using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoExchange.Net.Clients;

namespace TRCrypto.CoinTR;

/// <summary>
/// CoinTR istek imzalama saglayicisi.
/// </summary>
/// <remarks>
/// <para>
/// Imzalanan deger <c>zaman damgasi + METOT + yol + sorgu + govde</c> birlesimidir ve
/// sonuc <b>Base64</b> kodlanir. Dort borsa arasinda <b>yolu imzaya dahil eden tek borsa
/// budur</b>; yol atlandiginda imza sessizce gecersiz olur.
/// </para>
/// <para>
/// Kimlik bilgisi uc parcalidir. Parola da her istekte ayri bir baslikta gonderilir;
/// eksikse istek reddedilir.
/// </para>
/// <para>
/// <b>Bu sema resmi dokumantasyondan degil, calisan bir entegrasyondan alinmistir.</b>
/// Dokumantasyon iki asamali ve zaman dilimine bagli bir sema tarif ediyor; o sema
/// calismiyor. Ayrinti: <c>docs/vendor/cointr-capabilities.md</c>.
/// </para>
/// </remarks>
public class CoinTRAuthenticationProvider : AuthenticationProvider<CoinTRCredentials, HMACPassCredential>
{
    /// <summary>API anahtarinin gonderildigi baslik.</summary>
    internal const string KeyHeader = "ACCESS-KEY";

    /// <summary>Parolanin gonderildigi baslik.</summary>
    internal const string PassphraseHeader = "ACCESS-PASSPHRASE";

    /// <summary>Imzanin gonderildigi baslik.</summary>
    internal const string SignatureHeader = "ACCESS-SIGN";

    /// <summary>Zaman damgasinin gonderildigi baslik.</summary>
    internal const string TimestampHeader = "ACCESS-TIMESTAMP";

    private readonly byte[] _secretBytes;

    /// <summary>Yeni bir imzalama saglayicisi olusturur.</summary>
    /// <param name="credentials">Kullanilacak kimlik bilgileri.</param>
    public CoinTRAuthenticationProvider(CoinTRCredentials credentials)
        : base(credentials, credentials)
    {
        _secretBytes = Encoding.UTF8.GetBytes(Credential.Secret);
    }

    /// <summary>
    /// Verilen yuk icin imzayi uretir.
    /// </summary>
    /// <param name="payload">
    /// <c>zaman damgasi + METOT + yol + sorgu + govde</c> birlesimi.
    /// </param>
    /// <returns>Base64 kodlu HMAC-SHA256 imzasi.</returns>
    internal string CreateSignature(string payload)
    {
        using var hmac = new HMACSHA256(_secretBytes);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }

    /// <summary>
    /// Imzalanacak yuku olusturur.
    /// </summary>
    /// <remarks>
    /// Sorgu dizesi baştaki soru isaretiyle birlikte eklenir; sorgu yoksa bos metin
    /// kullanilir. Yol, <c>/api/v2</c> oneki dahil tam haliyle girer.
    /// </remarks>
    internal static string BuildPayload(string timestamp, string method, string path, string query, string body)
    {
        var builder = new StringBuilder()
            .Append(timestamp)
            .Append(method.ToUpperInvariant())
            .Append(path);

        if (!string.IsNullOrEmpty(query))
            builder.Append('?').Append(query);

        return builder.Append(body).ToString();
    }

    /// <inheritdoc />
    public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig)
    {
        if (!requestConfig.RequestDefinition.Authenticated)
            return;

        var timestamp = DateTimeConverter
            .ConvertToMilliseconds(GetTimestamp(apiClient))!
            .Value
            .ToString(CultureInfo.InvariantCulture);

        var query = requestConfig.QueryParameters is { Count: > 0 }
            ? requestConfig.QueryParameters.CreateParamString(true, requestConfig.ArraySerialization)
            : string.Empty;

        // Govde JSON olarak gonderilir ve imzaya ham haliyle girer. Imzalanan metin ile
        // gonderilen govde ayrisirsa imza sessizce gecersiz olur; ikisi de ayni
        // serilestiriciden uretilir.
        var body = requestConfig.BodyParameters is { Count: > 0 }
            ? JsonSerializer.Serialize(
                requestConfig.BodyParameters.ToDictionary(x => x.Key, x => x.Value))
            : string.Empty;

        var payload = BuildPayload(
            timestamp,
            requestConfig.RequestDefinition.Method.Method,
            requestConfig.RequestDefinition.Path,
            query,
            body);

        requestConfig.Headers ??= new Dictionary<string, string>();
        requestConfig.Headers[KeyHeader] = Credential.Key;
        requestConfig.Headers[PassphraseHeader] = Credential.Pass;
        requestConfig.Headers[SignatureHeader] = CreateSignature(payload);
        requestConfig.Headers[TimestampHeader] = timestamp;

        // Borsa hata mesajlarini bu basliga gore yerellestirir; Ingilizce sabitlenir ki
        // hata eslemesi dile bagli olmasin.
        requestConfig.Headers["locale"] = "en-US";
    }
}
