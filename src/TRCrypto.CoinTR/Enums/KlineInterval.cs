namespace TRCrypto.CoinTR.Enums;

/// <summary>Mum araligi.</summary>
/// <remarks>
/// Borsa aralik adini metin olarak bekler (<c>1min</c>, <c>1h</c>, <c>1day</c>).
/// Sayisal deger saniye cinsindendir ve borsadan bagimsiz yuzeyle esleme icin kullanilir.
/// </remarks>
[JsonConverter(typeof(EnumConverter<KlineInterval>))]
public enum KlineInterval
{
    /// <summary>Bir dakika.</summary>
    [Map("1min")]
    OneMinute = 60,

    /// <summary>Bes dakika.</summary>
    [Map("5min")]
    FiveMinutes = 60 * 5,

    /// <summary>On bes dakika.</summary>
    [Map("15min")]
    FifteenMinutes = 60 * 15,

    /// <summary>Otuz dakika.</summary>
    [Map("30min")]
    ThirtyMinutes = 60 * 30,

    /// <summary>Bir saat.</summary>
    [Map("1h")]
    OneHour = 60 * 60,

    /// <summary>Dort saat.</summary>
    [Map("4h")]
    FourHours = 60 * 60 * 4,

    /// <summary>Bir gun.</summary>
    [Map("1day")]
    OneDay = 60 * 60 * 24,

    /// <summary>Bir hafta.</summary>
    [Map("1week")]
    OneWeek = 60 * 60 * 24 * 7
}
