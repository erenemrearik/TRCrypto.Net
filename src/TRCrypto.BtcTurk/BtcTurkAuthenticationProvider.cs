using CryptoExchange.Net.Clients;

namespace TRCrypto.BtcTurk;

/// <summary>
/// BtcTurk istek imzalama saglayicisi.
/// </summary>
/// <remarks>
/// <para>
/// Imzalama zinciri (resmi dokumantasyondan dogrulanmistir):
/// </para>
/// <list type="number">
///   <item><description>Mesaj = <c>apiKey + stamp</c>; <c>stamp</c> UTC milisaniye nonce'udur.</description></item>
///   <item><description>Secret Base64 cozulur ve HMAC-SHA256 anahtari olarak kullanilir.</description></item>
///   <item><description>Uretilen digest Base64 kodlanarak <c>X-Signature</c> basligina yazilir.</description></item>
/// </list>
/// <para>
/// Ikinci adim atlanirsa imza sessizce yanlis olur; bu nedenle uygulama, resmi ornekten
/// turetilen sabit bir test vektoru ile dogrulanmadan tamamlanmis sayilmaz.
/// </para>
/// <para>
/// <b>Bu sinif henuz uygulanmamistir.</b> PR-001 yalnizca kimlik dogrulama gerektirmeyen
/// piyasa verisi uclarini kapsar (spesifikasyon Bolum 18.5); imzalama M2'de eklenecektir.
/// </para>
/// </remarks>
public class BtcTurkAuthenticationProvider : AuthenticationProvider<BtcTurkCredentials, HMACCredential>
{
    /// <summary>Yeni bir imzalama saglayicisi olusturur.</summary>
    /// <param name="credentials">Kullanilacak kimlik bilgileri.</param>
    public BtcTurkAuthenticationProvider(BtcTurkCredentials credentials)
        : base(credentials, credentials)
    {
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Imzalama henuz uygulanmadigi icin her zaman firlatilir. Kimlik dogrulama gerektiren
    /// uclar bu surumde mevcut degildir; yalnizca piyasa verisi uclari kullanilabilir.
    /// </exception>
    public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig)
        => throw new NotSupportedException(
            "BtcTurk istek imzalama bu surumde henuz uygulanmamistir. " +
            "Bu surum yalnizca kimlik dogrulama gerektirmeyen piyasa verisi uclarini destekler.");
}
