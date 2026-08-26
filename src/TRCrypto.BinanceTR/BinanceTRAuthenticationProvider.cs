using System.Globalization;
using System.Security.Cryptography;
using System.Text;
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
/// <b>Bu sinif henuz canli bir hesapla dogrulanmamistir.</b> Kimlik dogrulama gerektiren
/// uclar bu surumde sunulmamaktadir.
/// </para>
/// </remarks>
public class BinanceTRAuthenticationProvider : AuthenticationProvider<BinanceTRCredentials, HMACCredential>
{
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
    /// <exception cref="NotSupportedException">
    /// Imzalama canli bir hesapla dogrulanmadigi icin firlatilir. Bu surum yalnizca
    /// kimlik dogrulama gerektirmeyen uclari destekler.
    /// </exception>
    public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig)
    {
        if (!requestConfig.RequestDefinition.Authenticated)
            return;

        throw new NotSupportedException(
            "Binance TR istek imzalama henuz canli olarak dogrulanmadi. " +
            "Bu surum yalnizca kimlik dogrulama gerektirmeyen uclari destekler.");
    }
}
