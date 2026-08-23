using System.Text.Json;

namespace TRCrypto.BtcTurk.Objects.Internal;

/// <summary>BtcTurk yanitlarini ayristirirken kullanilan ortak serilestirme ayarlari.</summary>
internal static class BtcTurkJsonOptions
{
    /// <summary>Varsayilan ayarlar.</summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };
}
