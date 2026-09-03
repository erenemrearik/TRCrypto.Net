using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CryptoExchange.Net;
using CryptoExchange.Net.Clients;

namespace TRCrypto.BinanceTR;

/// <summary>
/// Binance TR istek imzalama saglayicisi.
/// </summary>
/// <remarks>
/// <para>
/// Imzalama global Binance kalibini izler: sorgu dizesi ile istek govdesi birlestirilir,
/// secret ile HMAC-SHA256 hesaplanir ve sonuc <b>onaltilik</b> metin olarak
/// <c>signature</c> parametresine eklenir. Anahtar <c>X-MBX-APIKEY</c> basligiyla gonderilir.
/// </para>
/// <para>
/// BtcTurk'ten iki fark: secret Base64 cozulmez ve imza Base64 degil hex kodlanir.
/// </para>
/// <para>
/// Sema resmi dokumantasyondan alinmis ve yayimlanmis test vektoruyle dogrulanmistir.
/// Canli bir hesaba karsi denenmesi API anahtari geldiginde yapilacaktir; bunun icin
/// <c>AuthenticationProbeTests</c> hazirdir.
/// </para>
/// </remarks>
public class BinanceTRAuthenticationProvider : AuthenticationProvider<BinanceTRCredentials, HMACCredential>
{
    /// <summary>Anahtarin gonderildigi baslik.</summary>
    internal const string ApiKeyHeader = "X-MBX-APIKEY";

    /// <summary>Zorunlu zaman damgasi parametresi.</summary>
    internal const string TimestampParameter = "timestamp";

    /// <summary>Imza parametresi.</summary>
    internal const string SignatureParameter = "signature";

    private readonly byte[] _secretBytes;

    /// <summary>Yeni bir imzalama saglayicisi olusturur.</summary>
    /// <param name="credentials">Kullanilacak kimlik bilgileri.</param>
    public BinanceTRAuthenticationProvider(BinanceTRCredentials credentials)
        : base(credentials, credentials)
    {
        _secretBytes = Encoding.UTF8.GetBytes(Credential.Secret);
    }

    /// <summary>Verilen yuk icin imzayi uretir.</summary>
    /// <param name="payload">Sorgu dizesi ve govdenin birlesimi.</param>
    /// <returns>Onaltilik kodlu HMAC-SHA256 imzasi.</returns>
    internal string CreateSignature(string payload)
    {
        using var hmac = new HMACSHA256(_secretBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

#if NET8_0_OR_GREATER
        return Convert.ToHexString(hash).ToLowerInvariant();
#else
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        return builder.ToString();
#endif
    }

    /// <inheritdoc />
    /// <remarks>
    /// Imzalanan yuk, sorgu dizesi ile govdenin parametre sirasina gore birlesimidir.
    /// Imza kendisi disindaki tum parametreler uzerinden hesaplandigi icin en sona eklenir;
    /// baska bir yere konulursa sunucu farkli bir yuk uzerinden dogrulama yapar ve istegi
    /// reddeder.
    /// </remarks>
    public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig)
    {
        if (!requestConfig.RequestDefinition.Authenticated)
            return;

        requestConfig.Headers ??= new Dictionary<string, string>();
        requestConfig.Headers[ApiKeyHeader] = Credential.Key;

        // Zaman damgasi sunucu saatiyle uyumlu olmalidir. Borsa istegi yalnizca
        // recvWindow icinde kabul eder; varsayilan pencere 5 saniyedir.
        var timestamp = DateTimeConverter
            .ConvertToMilliseconds(GetTimestamp(apiClient))!
            .Value
            .ToString(CultureInfo.InvariantCulture);

        var parameters = requestConfig.ParameterPosition == HttpMethodParameterPosition.InUri
            ? requestConfig.QueryParameters
            : requestConfig.BodyParameters;

        parameters!.Add(TimestampParameter, timestamp);

        var payload = BuildPayload(requestConfig);
        parameters.Add(SignatureParameter, CreateSignature(payload));
    }

    /// <summary>
    /// Imzalanacak yuku olusturur.
    /// </summary>
    /// <remarks>
    /// Sorgu dizesi ve govde, dokumantasyonda tarif edildigi gibi parametre sirasina gore
    /// pesipese eklenir. Parametreler yeniden siralanmaz: sunucu istegin uzerindeki sirayi
    /// kullanir, alfabetik siralama imzayi bozar.
    /// <para>
    /// Dizeyi kutuphanenin istegi kurarken kullandigi yardimci uretir. Elle kurulan bir
    /// dize, kacislama ya da sayi bicimi farki yuzunden gonderilenden ayrisabilir ve imza
    /// sessizce gecersiz olur.
    /// </para>
    /// </remarks>
    private static string BuildPayload(RestRequestConfiguration requestConfig)
    {
        var query = Serialize(requestConfig.QueryParameters, requestConfig.ArraySerialization);
        var body = Serialize(requestConfig.BodyParameters, requestConfig.ArraySerialization);

        if (query.Length == 0)
            return body;

        return body.Length == 0 ? query : query + "&" + body;
    }

    private static string Serialize(Parameters? parameters, ArrayParametersSerialization arraySerialization)
        => parameters == null || parameters.Count == 0
            ? string.Empty
            : parameters.CreateParamString(true, arraySerialization);
}
