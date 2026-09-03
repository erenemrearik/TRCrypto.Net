namespace TRCrypto.BinanceTR.Enums;

/// <summary>Emir turu.</summary>
/// <remarks>
/// Borsa bu bilgiyi <b>sayi</b> olarak tasir. Global Binance metin kullanir; oradan
/// tasinan kod burada sessizce yanlis cozumlenir.
/// </remarks>
[JsonConverter(typeof(EnumConverter<OrderType>))]
public enum OrderType
{
    /// <summary>["<c>1</c>"] Limit emri.</summary>
    [Map("1", "LIMIT")]
    Limit = 1,

    /// <summary>["<c>2</c>"] Piyasa emri.</summary>
    [Map("2", "MARKET")]
    Market = 2,

    /// <summary>["<c>3</c>"] Zarar durdur.</summary>
    [Map("3", "STOP_LOSS")]
    StopLoss = 3,

    /// <summary>["<c>4</c>"] Zarar durdur limitli.</summary>
    [Map("4", "STOP_LOSS_LIMIT")]
    StopLossLimit = 4,

    /// <summary>["<c>5</c>"] Kar al.</summary>
    [Map("5", "TAKE_PROFIT")]
    TakeProfit = 5,

    /// <summary>["<c>6</c>"] Kar al limitli.</summary>
    [Map("6", "TAKE_PROFIT_LIMIT")]
    TakeProfitLimit = 6,

    /// <summary>["<c>7</c>"] Yalnizca piyasa yapici limit emri.</summary>
    [Map("7", "LIMIT_MAKER")]
    LimitMaker = 7
}

/// <summary>Emrin yasam dongusundeki durumu.</summary>
[JsonConverter(typeof(EnumConverter<OrderStatus>))]
public enum OrderStatus
{
    /// <summary>["<c>-2</c>"] Sistem emri isliyor.</summary>
    [Map("-2")]
    SystemProcessing = -2,

    /// <summary>["<c>0</c>"] Deftere yeni girdi.</summary>
    [Map("0", "NEW")]
    New = 0,

    /// <summary>["<c>1</c>"] Kismen gerceklesti.</summary>
    [Map("1", "PARTIALLY_FILLED")]
    PartiallyFilled = 1,

    /// <summary>["<c>2</c>"] Tamamen gerceklesti.</summary>
    [Map("2", "FILLED")]
    Filled = 2,

    /// <summary>["<c>3</c>"] Iptal edildi.</summary>
    [Map("3", "CANCELED")]
    Canceled = 3,

    /// <summary>["<c>4</c>"] Iptal bekliyor.</summary>
    [Map("4", "PENDING_CANCEL")]
    PendingCancel = 4,

    /// <summary>["<c>5</c>"] Reddedildi.</summary>
    [Map("5", "REJECTED")]
    Rejected = 5,

    /// <summary>["<c>6</c>"] Suresi doldu.</summary>
    [Map("6", "EXPIRED")]
    Expired = 6
}

/// <summary>Emrin ne kadar sure gecerli kalacagi.</summary>
[JsonConverter(typeof(EnumConverter<TimeInForce>))]
public enum TimeInForce
{
    /// <summary>["<c>1</c>"] Iptal edilene kadar gecerli.</summary>
    [Map("1", "GTC")]
    GoodTillCanceled = 1,

    /// <summary>["<c>2</c>"] Hemen gerceklesen kismi al, kalanini iptal et.</summary>
    [Map("2", "IOC")]
    ImmediateOrCancel = 2,

    /// <summary>["<c>3</c>"] Tamami gerceklesmezse iptal et.</summary>
    [Map("3", "FOK")]
    FillOrKill = 3,

    /// <summary>["<c>4</c>"] Yalnizca piyasa yapici olarak deftere gir.</summary>
    [Map("4", "GTX")]
    GoodTillCrossing = 4
}
