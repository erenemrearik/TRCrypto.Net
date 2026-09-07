using System.Collections.Concurrent;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using TRCrypto.BinanceTR.Clients;
using TRCrypto.BtcTurk.Clients;

namespace TRCrypto.Examples.Dashboard;

/// <summary>
/// Panonun beslendigi piyasa durumu.
/// </summary>
/// <remarks>
/// <para>
/// Butun veri <b>borsadan bagimsiz yuzeyden</b> gelir. Asagidaki kodun hicbir yerinde
/// native sembol bicimi, borsaya ozgu bir model ya da borsaya gore dallanan bir mantik
/// yoktur; iki borsa da ayni cagrilarla dinlenir. Panonun asil gosterdigi sey budur.
/// </para>
/// <para>
/// Yalnizca herkese acik veri kullanilir; hicbir kimlik bilgisi gerekmez.
/// </para>
/// </remarks>
internal sealed class MarketBoard : IAsyncDisposable
{
    /// <summary>Panoda izlenen varliklar. Hepsi Turk lirasi karsisinda listelenir.</summary>
    private static readonly string[] _assets = ["BTC", "ETH", "XRP", "SOL", "AVAX", "DOGE"];

    private readonly ConcurrentDictionary<string, Quote> _quotes = new();
    private readonly ConcurrentDictionary<string, VenueHealth> _health = new();
    private readonly List<UpdateSubscription> _subscriptions = [];
    private readonly ILogger<MarketBoard> _logger;

    private readonly ConcurrentDictionary<string, Func<string, string, TradingMode, DateTime?, string>> _nativeNames = new();

    private BtcTurkSocketClient? _btcTurk;
    private BinanceTRSocketClient? _binanceTR;

    public MarketBoard(ILogger<MarketBoard> logger)
    {
        _logger = logger;
    }

    /// <summary>Panonun acilis anindan beri gecen sure.</summary>
    public DateTime StartedAt { get; } = DateTime.UtcNow;

    /// <summary>Izlenen varliklar.</summary>
    public static IReadOnlyList<string> Assets => _assets;

    /// <summary>Panoda gosterilen borsalar.</summary>
    public static IReadOnlyList<string> Venues => ["BtcTurk", "Binance TR"];

    /// <summary>
    /// Iki borsadaki butun paritelere abone olur.
    /// </summary>
    /// <remarks>
    /// Abonelik dongusu borsaya gore dallanmaz: her ikisi de ayni
    /// <see cref="ITickerSocketClient"/> arayuzu uzerinden dinlenir.
    /// </remarks>
    public async Task StartAsync(CancellationToken ct)
    {
        _btcTurk = new BtcTurkSocketClient();
        _binanceTR = new BinanceTRSocketClient();

        var venues = new (string Name, ITickerSocketClient Client)[]
        {
            ("BtcTurk", _btcTurk.SpotApi.SharedClient),
            ("Binance TR", _binanceTR.SpotApi.SharedClient),
        };

        // Her borsanin ayni SharedSymbol icin urettigi native ad. Abone olan kod bunu
        // hic kullanmaz; panoda gostermek "ayni kod, farkli bicimler" fikrini gorunur
        // kilar. Akis govdesindeki ad iki borsada da ayni geldigi icin yetmiyor.
        _nativeNames["BtcTurk"] = _btcTurk.SpotApi.FormatSymbol;
        _nativeNames["Binance TR"] = _binanceTR.SpotApi.FormatSymbol;

        foreach (var (venue, client) in venues)
        {
            _health[venue] = new VenueHealth();

            foreach (var asset in _assets)
            {
                var symbol = new SharedSymbol(TradingMode.Spot, asset, "TRY");

                var result = await client
                    .SubscribeToTickerUpdatesAsync(
                        new SubscribeTickerRequest(symbol),
                        update => Record(venue, asset, update),
                        ct)
                    .ConfigureAwait(false);

                if (result.Success)
                {
                    _subscriptions.Add(result.Data);
                    _health[venue].Subscriptions++;

                    // Baglanti koptugunda ve geri geldiginde panoda gorunur olsun.
                    result.Data.ConnectionLost += () => _health[venue].Disconnects++;
                    result.Data.ConnectionRestored += _ => _health[venue].Reconnects++;
                }
                else
                {
                    _health[venue].LastError = result.Error?.ToString();
                    _logger.LogWarning(
                        "{Venue} {Asset} aboneligi kurulamadi: {Error}", venue, asset, result.Error);
                }
            }
        }
    }

    /// <summary>Gelen bir guncellemeyi kaydeder.</summary>
    private void Record(string venue, string asset, DataEvent<SharedSpotTicker> update)
    {
        var native = _nativeNames.TryGetValue(venue, out var format)
            ? format(asset, "TRY", TradingMode.Spot, null)
            : update.Data.Symbol;

        _quotes[Key(venue, asset)] = new Quote(
            update.Data.LastPrice,
            update.Data.ChangePercentage,
            update.Data.HighPrice,
            update.Data.LowPrice,
            update.Data.Volumes.QuantityInBaseAsset,
            native,
            update.ReceiveTime);

        _health[venue].Messages++;
        _health[venue].LastMessageAt = DateTime.UtcNow;
    }

    private static string Key(string venue, string asset) => venue + "/" + asset;

    /// <summary>Tarayiciya gonderilecek anlik goruntuyu uretir.</summary>
    public BoardSnapshot Snapshot()
    {
        var rows = _assets.Select(asset => new BoardRow(
            asset,
            Venues.Select(venue =>
            {
                _quotes.TryGetValue(Key(venue, asset), out var quote);
                return new BoardCell(
                    venue,
                    quote?.LastPrice,
                    quote?.ChangePercentage,
                    quote?.High,
                    quote?.Low,
                    quote?.Volume,
                    quote?.NativeSymbol,
                    quote is null ? null : (int)(DateTime.UtcNow - quote.ReceivedAt).TotalMilliseconds);
            }).ToArray())).ToArray();

        var health = Venues.Select(venue =>
        {
            var state = _health.GetValueOrDefault(venue) ?? new VenueHealth();
            return new VenueStatus(
                venue,
                state.Subscriptions,
                state.Messages,
                state.Disconnects,
                state.Reconnects,
                state.LastMessageAt is null
                    ? null
                    : (int)(DateTime.UtcNow - state.LastMessageAt.Value).TotalMilliseconds,
                state.LastError);
        }).ToArray();

        return new BoardSnapshot(rows, health, (int)(DateTime.UtcNow - StartedAt).TotalSeconds);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var subscription in _subscriptions)
            await subscription.CloseAsync().ConfigureAwait(false);

        _btcTurk?.Dispose();
        _binanceTR?.Dispose();
    }

    private sealed record Quote(
        decimal? LastPrice,
        decimal? ChangePercentage,
        decimal? High,
        decimal? Low,
        decimal? Volume,
        string NativeSymbol,
        DateTime ReceivedAt);

    private sealed class VenueHealth
    {
        public int Subscriptions;
        public long Messages;
        public int Disconnects;
        public int Reconnects;
        public DateTime? LastMessageAt;
        public string? LastError;
    }
}

/// <summary>Tarayiciya gonderilen anlik goruntu.</summary>
internal sealed record BoardSnapshot(BoardRow[] Rows, VenueStatus[] Venues, int UptimeSeconds);

/// <summary>Tek bir varligin butun borsalardaki satiri.</summary>
internal sealed record BoardRow(string Asset, BoardCell[] Cells);

/// <summary>Bir varligin tek bir borsadaki durumu.</summary>
internal sealed record BoardCell(
    string Venue,
    decimal? Price,
    decimal? ChangePercentage,
    decimal? High,
    decimal? Low,
    decimal? Volume,
    string? NativeSymbol,
    int? AgeMs);

/// <summary>Bir borsanin baglanti sagligi.</summary>
internal sealed record VenueStatus(
    string Venue,
    int Subscriptions,
    long Messages,
    int Disconnects,
    int Reconnects,
    int? LastMessageAgeMs,
    string? LastError);
