using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Interfaces.Clients.SpotApi;

namespace TRCrypto.BinanceTR.Clients.SpotApi;

/// <summary>
/// Kullanici akisinin borsadan bagimsiz yuzeyi.
/// </summary>
/// <remarks>
/// <para>
/// Bu akis bir dinleme tokeni ister, ama borsadan bagimsiz istek nesnelerinde boyle bir
/// alan yoktur. Token bu yuzden borsaya ozgu parametre olarak verilir:
/// </para>
/// <code>
/// var token = await rest.SpotApi.Account.GetListenTokenAsync();
/// socket.UserApi.SharedClient.SetDefaultExchangeParameter(
///     BinanceTRSocketClientUserApi.ListenTokenParameter, token.Data.Token);
/// </code>
/// <para>
/// Token verilmediginde abonelik kurulmaz ve hata bunu acikca soyler. Sessizce bos bir
/// akis dondurmek, sorunun ancak "veri gelmiyor" olarak fark edilmesine yol acardi.
/// </para>
/// </remarks>
internal partial class BinanceTRSocketClientUserApi : IBinanceTRSocketClientUserApiShared
{
    /// <summary>Dinleme tokeninin verildigi borsaya ozgu parametre adi.</summary>
    public const string ListenTokenParameter = "listenToken";

    /// <inheritdoc />
    public TradingMode[] SupportedTradingModes { get; } = [TradingMode.Spot];

    /// <inheritdoc />
    SharedTransport ISharedApi.Transport => SharedTransport.Socket;

    /// <summary>Bu istemcinin bildirdigi yetenekler.</summary>
    /// <remarks>
    /// Liste, asagida uygulanan her paylasilan arayuzun secenek nesnesini tam olarak icerir.
    /// Eksik bir giris <c>Discover()</c> ciktisindan o yetenegi dusurur; fazla bir giris
    /// dogrulanmamis bir yetenegi ilan eder. Ikisini de <c>SharedApiTests</c> yakalar.
    /// </remarks>
    IReadOnlyCollection<CapabilityOptions> ISharedApi.Capabilities => _capabilities;

    private CapabilityOptions[] _capabilities = [];

    /// <summary>Yetenekleri olusturur ve CryptoExchange.Net'e kaydeder.</summary>
    /// <remarks>
    /// Kurucudan cagrilir. Dogrulama ilk istekten once hazir olmalidir; liste tembel
    /// olusturulsaydi, hic okunmadan yapilan bir cagrida secenekler kaydedilmemis kalirdi.
    /// </remarks>
    private void RegisterCapabilities()
    {
        _capabilities =
        [
            ((IBalanceSocketClient)this).SubscribeBalanceOptions,
            ((ISpotOrderSocketClient)this).SubscribeSpotOrderOptions,
        ];

        CapabilityRegistration.Register(this, this, _capabilities);
    }

    /// <inheritdoc />
    public void SetDefaultExchangeParameter(string key, object value)
        => ExchangeParameters.SetStaticParameter(Exchange, key, value);

    /// <inheritdoc />
    public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

    /// <inheritdoc />
    public SharedClientInfo Discover() => SharedUtils.GetClientInfo(BinanceTRExchange.Metadata, this);

    /// <inheritdoc />
    public IBinanceTRSocketClientUserApiShared SharedClient => this;

    #region Balance

    SubscribeBalanceOptions IBalanceSocketClient.SubscribeBalanceOptions { get; }
        = new(BinanceTRExchange.ExchangeName, true)
        {
            RequestNotes =
                "Dinleme tokeni gerekir. Token SpotApi.Account.GetListenTokenAsync ile alinir " +
                "ve SetDefaultExchangeParameter(\"listenToken\", …) ile verilir."
        };

    async Task<WebSocketResult<UpdateSubscription>> IBalanceSocketClient.SubscribeToBalanceUpdatesAsync(
        SubscribeBalancesRequest request,
        Action<DataEvent<SharedBalance[]>> handler,
        CancellationToken ct)
    {
        var validationError = ((IBalanceSocketClient)this).SubscribeBalanceOptions
            .ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        if (!TryGetListenToken(request.ExchangeParameters, out var token, out var tokenError))
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, tokenError!);

        return await SubscribeToAccountUpdatesAsync(token!, update => handler(Convert(update,
            update.Data.Balances
                .Select(x => new SharedBalance(TradingMode.Spot, x.Asset, x.Available, x.Total))
                .ToArray())), ct).ConfigureAwait(false);
    }

    #endregion

    #region Spot orders

    SubscribeSpotOrderOptions ISpotOrderSocketClient.SubscribeSpotOrderOptions { get; }
        = new(BinanceTRExchange.ExchangeName, true)
        {
            RequestNotes =
                "Dinleme tokeni gerekir. Token SpotApi.Account.GetListenTokenAsync ile alinir " +
                "ve SetDefaultExchangeParameter(\"listenToken\", …) ile verilir."
        };

    // V1 arayuzu V2'ye delege eder. V2 modeli (SharedSpotOrderUpdate) V1 modelinden turer
    // ve ondan zengindir, bu yuzden birincil uygulama V2'dedir ve V1 ayni olayi daha dar
    // modelle gorur.
    async Task<WebSocketResult<UpdateSubscription>> ISpotOrderSocketClient.SubscribeToSpotOrderUpdatesAsync(
        SubscribeSpotOrderRequest request,
        Action<DataEvent<SharedSpotOrder[]>> handler,
        CancellationToken ct)
        => await ((ISubscribeSpotOrdersSocket)this).SubscribeToSpotOrderUpdatesAsync(
            request, x => handler(x.ToType<SharedSpotOrder[]>(x.Data)), ct).ConfigureAwait(false);

    SubscribeSpotOrderOptions ISubscribeSpotOrdersSocket.SubscribeSpotOrderOptions
        => ((ISpotOrderSocketClient)this).SubscribeSpotOrderOptions;

    async Task<WebSocketResult<UpdateSubscription>> ISubscribeSpotOrdersSocket.SubscribeToSpotOrderUpdatesAsync(
        SubscribeSpotOrderRequest request,
        Action<DataEvent<SharedSpotOrderUpdate[]>> handler,
        CancellationToken ct)
    {
        var validationError = ((ISpotOrderSocketClient)this).SubscribeSpotOrderOptions
            .ValidateRequest(request, this);
        if (validationError != null)
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, validationError);

        if (!TryGetListenToken(request.ExchangeParameters, out var token, out var tokenError))
            return WebSocketResult.Fail<UpdateSubscription>(Exchange, tokenError!);

        return await SubscribeToOrderUpdatesAsync(token!, update => handler(update.ToType(
            new[] { ToSharedOrderUpdate(update.Data) })), ct).ConfigureAwait(false);
    }

    /// <summary>Native emir guncellemesini borsadan bagimsiz karsiligina cevirir.</summary>
    /// <remarks>
    /// <para>
    /// Ucret emir duzeyinde degil, son dolumun (<see cref="SharedSpotOrderUpdate.LastTrade"/>)
    /// uzerinde tasinir. Borsa ucreti her dolum icin ayri bildirir; emir duzeyinde tutmak
    /// birden fazla dolumda yalnizca sonuncusunu gosterirdi. CryptoExchange.Net 13 emir
    /// duzeyindeki ucret alanini bu nedenle kullanimdan kaldirdi.
    /// </para>
    /// <para>
    /// <c>AveragePrice</c> doldurulmaz. Akisin <c>L</c> alani ortalama degil son dolumun
    /// fiyatidir ve onceki surumde yanlislikla ortalama yerine yaziliyordu; o deger artik
    /// <c>LastTrade.Price</c> icindedir.
    /// </para>
    /// </remarks>
    private static SharedSpotOrderUpdate ToSharedOrderUpdate(Objects.Models.Socket.BinanceTRStreamOrderUpdate update)
    {
        // Akis base ve quote varligi ayri alanlarda vermiyor; yalnizca native ad var.
        var symbol = new SharedSymbol(TradingMode.Spot, update.Symbol, string.Empty);
        var side = update.Side == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell;
        var orderId = update.OrderId.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return new SharedSpotOrderUpdate(
            symbol,
            update.Symbol,
            orderId,
            update.Type == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Limit,
            side,
            ToSharedStatus(update.Status),
            update.CreateTime)
        {
            ClientOrderId = update.ClientOrderId,
            OrderPrice = update.Price,
            OrderQuantity = new SharedOrderQuantity(update.Quantity),
            QuantityFilled = new SharedOrderQuantity(update.QuantityFilled, update.QuoteQuantityFilled),
            UpdateTime = update.EventTime,

            // Dolum olmayan guncellemelerde (yeni emir, iptal) son islem yoktur.
            LastTrade = update.LastQuantityFilled == 0 ? null : new SharedUserTrade(
                symbol,
                update.Symbol,
                orderId,
                update.TradeId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                side,
                new SharedOrderQuantity(update.LastQuantityFilled, update.LastQuoteQuantityFilled),
                update.LastPriceFilled,
                update.TradeTime)
            {
                ClientOrderId = update.ClientOrderId,
                Fee = update.Fee,
                FeeAsset = update.FeeAsset,
                Role = update.IsMaker ? SharedRole.Maker : SharedRole.Taker
            }
        };
    }

    #endregion

    /// <summary>
    /// Dinleme tokenini borsaya ozgu parametrelerden okur.
    /// </summary>
    /// <remarks>
    /// Once istekle birlikte verilen deger, sonra varsayilan olarak ayarlanmis deger
    /// aranir. Ikisi de yoksa abonelik kurulmaz.
    /// </remarks>
    private bool TryGetListenToken(
        ExchangeParameters? parameters,
        out string? token,
        out Error? error)
    {
        token = null;
        error = null;

        if (ExchangeParameters.HasValue(parameters, Exchange, ListenTokenParameter, typeof(string)))
            token = ExchangeParameters.GetValue<string>(parameters, Exchange, ListenTokenParameter);

        if (!string.IsNullOrWhiteSpace(token))
            return true;

        error = new InvalidOperationError(
            "Kullanici akisi dinleme tokeni gerektirir. Token " +
            "SpotApi.Account.GetListenTokenAsync ile alinir ve " +
            $"SetDefaultExchangeParameter(\"{ListenTokenParameter}\", token) ile verilir.");

        return false;
    }

    /// <summary>
    /// Borsanin durum degerini borsadan bagimsiz karsiligina cevirir.
    /// </summary>
    /// <remarks>
    /// Akista durum metin olarak gelir; REST tarafinda ayni bilgi sayidir. Iki taraf ayni
    /// enum'a cozumlenir, bu yuzden esleme burada da gerekir.
    /// </remarks>
    private static SharedOrderStatus ToSharedStatus(Enums.OrderStatus status)
        => status switch
        {
            Enums.OrderStatus.SystemProcessing => SharedOrderStatus.Open,
            Enums.OrderStatus.New => SharedOrderStatus.Open,
            Enums.OrderStatus.PartiallyFilled => SharedOrderStatus.Open,
            Enums.OrderStatus.PendingCancel => SharedOrderStatus.Open,
            Enums.OrderStatus.Filled => SharedOrderStatus.Filled,
            Enums.OrderStatus.Canceled => SharedOrderStatus.Canceled,
            Enums.OrderStatus.Rejected => SharedOrderStatus.Canceled,
            Enums.OrderStatus.Expired => SharedOrderStatus.Canceled,
            _ => SharedOrderStatus.Unknown
        };

    /// <summary>
    /// Native bir guncellemeyi, zaman ve kaynak bilgisini koruyarak borsadan bagimsiz
    /// karsiligina cevirir.
    /// </summary>
    private static DataEvent<TShared> Convert<TNative, TShared>(
        DataEvent<TNative> update,
        TShared data)
        => new DataEvent<TShared>(
            update.Exchange,
            data,
            update.ReceiveTime,
            update.OriginalData)
        {
            Symbol = update.Symbol,
            StreamId = update.StreamId,
            UpdateType = update.UpdateType
        };
}
