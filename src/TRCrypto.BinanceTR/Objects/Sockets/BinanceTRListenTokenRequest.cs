namespace TRCrypto.BinanceTR.Objects.Sockets;

/// <summary>
/// Kullanici akisina abone olma istegi.
/// </summary>
/// <remarks>
/// Bu akis piyasa akisindan farkli bir sunucuda ve farkli bir protokolde calisir. Piyasa
/// tarafinda akis adi baglanti adresinin yoluna yazilir; burada baglanti kurulduktan sonra
/// yontem adi tasiyan bir mesaj gonderilir.
/// </remarks>
public record BinanceTRListenTokenRequest
{
    /// <summary>Abonelik yontemi.</summary>
    public const string SubscribeMethod = "userDataStream.subscribe.listenToken";

    /// <param name="listenToken">
    /// <c>/open/v1/user-listen-token</c> ucundan alinan token.
    /// </param>
    public BinanceTRListenTokenRequest(string listenToken)
    {
        Params = new BinanceTRListenTokenParams { ListenToken = listenToken };
    }

    /// <summary>["<c>id</c>"] Istegi yanitiyla eslestiren kimlik.</summary>
    /// <remarks>Sunucu yaniti bu kimligi geri gondermez; yine de her istekte benzersizdir.</remarks>
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>["<c>method</c>"] Cagrilan yontem.</summary>
    [JsonPropertyName("method")]
    public string Method { get; init; } = SubscribeMethod;

    /// <summary>["<c>params</c>"] Yontem parametreleri.</summary>
    [JsonPropertyName("params")]
    public BinanceTRListenTokenParams Params { get; init; }
}

/// <summary>Abonelik isteginin parametreleri.</summary>
public record BinanceTRListenTokenParams
{
    /// <summary>["<c>listenToken</c>"] Dinleme tokeni.</summary>
    [JsonPropertyName("listenToken")]
    public string ListenToken { get; init; } = string.Empty;
}

/// <summary>Abonelik isteginin yaniti.</summary>
[SerializationModel]
public record BinanceTRListenTokenResponse
{
    /// <summary>["<c>subscriptionId</c>"] Aboneligi tanimlayan numara.</summary>
    [JsonPropertyName("subscriptionId")]
    public long SubscriptionId { get; init; }

    /// <summary>["<c>expirationTime</c>"] Aboneligin sona erecegi an.</summary>
    /// <remarks>
    /// Sunucu bu degeri mikrosaniye olarak gonderiyor; dokumantasyondaki ornek
    /// <c>1749094553955907</c> gibi on alti haneli. Milisaniye varsayan bir cozumleyici
    /// tarihi binlerce yil ileri okur.
    /// </remarks>
    [JsonPropertyName("expirationTime")]
    public long ExpirationTimeMicroseconds { get; init; }

    /// <summary>Aboneligin sona erecegi an.</summary>
    [JsonIgnore]
    public DateTime ExpirationTime =>
        DateTimeOffset.FromUnixTimeMilliseconds(ExpirationTimeMicroseconds / 1000).UtcDateTime;
}
