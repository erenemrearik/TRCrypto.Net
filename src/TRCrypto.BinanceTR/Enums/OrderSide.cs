namespace TRCrypto.BinanceTR.Enums;

/// <summary>Bir islemin ya da emrin yonu.</summary>
[JsonConverter(typeof(EnumConverter<OrderSide>))]
public enum OrderSide
{
    /// <summary>["<c>BUY</c>"] Alis.</summary>
    [Map("BUY", "buy", "0")]
    Buy,

    /// <summary>["<c>SELL</c>"] Satis.</summary>
    [Map("SELL", "sell", "1")]
    Sell
}
