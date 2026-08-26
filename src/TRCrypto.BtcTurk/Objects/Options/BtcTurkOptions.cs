namespace TRCrypto.BtcTurk.Objects.Options;

/// <summary>
/// <see cref="BtcTurkServiceCollectionExtensions.AddTRCryptoBtcTurk"/> icin secenekler.
/// </summary>
/// <remarks>
/// REST ve WebSocket istemcilerinin secenekleri ayridir; ikisi de gerektiginde tek tek
/// yapilandirilabilir. Cogu uygulama yalnizca <see cref="ApiCredentials"/> belirler.
/// </remarks>
public class BtcTurkOptions
{
    /// <summary>REST istemcisi secenekleri.</summary>
    public BtcTurkRestOptions Rest { get; } = new();

    /// <summary>WebSocket istemcisi secenekleri.</summary>
    public BtcTurkSocketOptions Socket { get; } = new();

    /// <summary>Her iki istemciye uygulanacak kimlik bilgisi.</summary>
    /// <remarks>
    /// Ayri ayri anahtar kullanmak gerekirse <see cref="Rest"/> ve <see cref="Socket"/>
    /// uzerindeki kimlik bilgisi dogrudan verilebilir; o durumda bu ozellik bos birakilir.
    /// </remarks>
    public BtcTurkCredentials? ApiCredentials { get; set; }
}
