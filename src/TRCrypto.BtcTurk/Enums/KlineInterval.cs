namespace TRCrypto.BtcTurk.Enums;

/// <summary>
/// Mum araligi.
/// </summary>
/// <remarks>
/// Sayisal degerler saniye cinsindendir ve istekte <c>resolution</c> parametresine
/// donusturulurken dakikaya cevrilir.
/// </remarks>
public enum KlineInterval
{
    /// <summary>Bir dakika.</summary>
    OneMinute = 60,

    /// <summary>On bes dakika.</summary>
    FifteenMinutes = 60 * 15,

    /// <summary>Otuz dakika.</summary>
    ThirtyMinutes = 60 * 30,

    /// <summary>Bir saat.</summary>
    OneHour = 60 * 60,

    /// <summary>Dort saat.</summary>
    FourHours = 60 * 60 * 4,

    /// <summary>Bir gun.</summary>
    OneDay = 60 * 60 * 24,

    /// <summary>Bir hafta.</summary>
    OneWeek = 60 * 60 * 24 * 7
}
