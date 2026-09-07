namespace TRCrypto.CoinTR.Enums;

/// <summary>Bir islemin ya da emrin yonu.</summary>
/// <remarks>
/// CoinTR bu bilgiyi kucuk harfli metin olarak tasir. Binance TR sayi kullanir; ayni
/// kavram borsalar arasinda uc farkli bicimde gelir.
/// </remarks>
[JsonConverter(typeof(EnumConverter<OrderSide>))]
public enum OrderSide
{
    /// <summary>["<c>buy</c>"] Alis.</summary>
    [Map("buy", "BUY", "Buy")]
    Buy,

    /// <summary>["<c>sell</c>"] Satis.</summary>
    [Map("sell", "SELL", "Sell")]
    Sell
}
