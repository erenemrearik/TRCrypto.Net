using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CryptoExchange.Net.Clients;

namespace TRCrypto.BtcTurk;

/// <summary>
/// BtcTurk istek imzalama saglayicisi.
/// </summary>
/// <remarks>
/// <para>Imzalama zinciri (resmi dokumantasyondan dogrulanmistir):</para>
/// <list type="number">
///   <item><description>Mesaj = <c>apiKey + stamp</c>; <c>stamp</c> UTC milisaniye nonce'udur.</description></item>
///   <item><description>Secret Base64 cozulur ve HMAC-SHA256 anahtari olarak kullanilir.</description></item>
///   <item><description>Uretilen digest Base64 kodlanarak <c>X-Signature</c> basligina yazilir.</description></item>
/// </list>
/// <para>
/// Ikinci adim atlanirsa imza sessizce yanlis olur ve borsa yalnizca genel bir kimlik
/// dogrulama hatasi dondurur. Bu nedenle zincir sabit test vektorleriyle dogrulanmaktadir.
/// </para>
/// </remarks>
public class BtcTurkAuthenticationProvider : AuthenticationProvider<BtcTurkCredentials, HMACCredential>
{
    private readonly byte[] _secretBytes;

    /// <summary>Yeni bir imzalama saglayicisi olusturur.</summary>
    /// <param name="credentials">Kullanilacak kimlik bilgileri.</param>
    /// <exception cref="ArgumentException">
    /// Secret gecerli bir Base64 dizisi degilse firlatilir.
    /// </exception>
    public BtcTurkAuthenticationProvider(BtcTurkCredentials credentials)
        : base(credentials, credentials)
    {
        try
        {
            _secretBytes = Convert.FromBase64String(Credential.Secret);
        }
        catch (FormatException ex)
        {
            // Hata mesaji secret'in kendisini ICERMEZ; yalnizca bicim sorununu bildirir.
            throw new ArgumentException(
                "BtcTurk API secret'i gecerli bir Base64 dizisi olmalidir. " +
                "Degeri borsanin panelinde gorundugu gibi, degistirmeden verin.",
                nameof(credentials),
                ex);
        }
    }

    /// <summary>
    /// Verilen nonce icin istek imzasini uretir.
    /// </summary>
    /// <param name="stamp">UTC milisaniye cinsinden nonce.</param>
    /// <returns>Base64 kodlu HMAC-SHA256 imzasi.</returns>
    internal string CreateSignature(string stamp)
    {
        var message = Encoding.UTF8.GetBytes(Credential.Key + stamp);

        using var hmac = new HMACSHA256(_secretBytes);
        return Convert.ToBase64String(hmac.ComputeHash(message));
    }

    /// <inheritdoc />
    public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig)
    {
        if (!requestConfig.RequestDefinition.Authenticated)
            return;

        // Nonce sunucu saatiyle uyumlu olmalidir; buyuk sapma isteklerin reddedilmesine yol acar.
        var stamp = GetMillisecondTimestamp(apiClient);

        requestConfig.Headers ??= new Dictionary<string, string>();
        requestConfig.Headers["X-PCK"] = Credential.Key;
        requestConfig.Headers["X-Stamp"] = stamp;
        requestConfig.Headers["X-Signature"] = CreateSignature(stamp);
    }

    /// <summary>
    /// Sunucu saat farkini hesaba katan milisaniye zaman damgasi uretir.
    /// </summary>
    private string GetMillisecondTimestamp(RestApiClient apiClient)
        => DateTimeConverter
            .ConvertToMilliseconds(GetTimestamp(apiClient))!
            .Value
            .ToString(CultureInfo.InvariantCulture);
}
