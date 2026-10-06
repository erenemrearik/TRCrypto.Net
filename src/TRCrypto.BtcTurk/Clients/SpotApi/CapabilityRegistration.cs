using CryptoExchange.Net.SharedApis;

namespace TRCrypto.BtcTurk.Clients.SpotApi;

/// <summary>
/// Paylasilan istemcinin yetenek seceneklerini CryptoExchange.Net'e kaydeder.
/// </summary>
/// <remarks>
/// <para>
/// CryptoExchange.Net 13 ile istek dogrulamasi, her secenek nesnesinin hangi islem
/// turlerini destekledigini bilmesine dayanir. Bu bilgi yalnizca
/// <c>SharedApiBase.SetCapabilities</c> ile verilebilir; degeri dolduran metot kutuphanenin
/// icine kapalidir. Kaydedilmeyen bir secenek hicbir islem turunu desteklemiyor gorunur
/// ve her paylasilan cagri <c>TradingMode.Spot is not supported</c> hatasiyla reddedilir.
/// </para>
/// <para>
/// Paylasilan istemcilerimiz zaten bir API istemcisinden turedigi icin
/// <see cref="SharedApiBase"/>'den turemezler. Bu sinif ayni API istemcisi icin eksiksiz
/// yapilandirilmis bir <see cref="SharedApiBase"/>'dir: islem turleri, kimlik dogrulama ve
/// sembol bicimi istemcinin kendisinden alinir. Tek isi secenekleri kaydetmektir ve
/// tuketiciye hic verilmez.
/// </para>
/// <para>
/// Bu hata birim testlerinde gorunmedi, cunku testler yapisaldi; canli dogrulama yakaladi.
/// Artik <c>CapabilityAssert</c> her secenegin kayitli oldugunu denetler.
/// </para>
/// </remarks>
internal sealed class CapabilityRegistration : SharedApiBase
{
    private CapabilityRegistration(ISharedApi shared, IBaseApiClient apiClient)
        : base(
            shared.Transport,
            apiClient,
            shared.SupportedTradingModes,
            () => shared.Authenticated,
            shared.FormatSymbol)
    {
    }

    /// <summary>Verilen secenekleri istemcinin islem turleriyle kaydeder.</summary>
    /// <param name="shared">Seceneklerin ait oldugu paylasilan istemci.</param>
    /// <param name="apiClient">Ayni istemcinin API istemcisi yuzu.</param>
    /// <param name="capabilities">Kaydedilecek secenekler.</param>
    internal static void Register(ISharedApi shared, IBaseApiClient apiClient, CapabilityOptions[] capabilities)
        => new CapabilityRegistration(shared, apiClient).SetCapabilities(capabilities);

    /// <summary>Kullanilmaz; bu nesne tuketiciye verilmez.</summary>
    public override SharedClientInfo Discover()
        => throw new NotSupportedException(
            "Kayit nesnesi yalnizca secenekleri kaydeder; Discover paylasilan istemciden cagrilmalidir.");
}
