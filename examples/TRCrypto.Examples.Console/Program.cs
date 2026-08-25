using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk;
using TRCrypto.BtcTurk.Clients;
using TRCrypto.BtcTurk.Enums;

// TRCrypto.BtcTurk - canli public API dogrulamasi.
// Bu ornek kimlik bilgisi GEREKTIRMEZ; yalnizca herkese acik piyasa verisi kullanir.

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("TRCrypto.BtcTurk - canli dogrulama");
Console.WriteLine(new string('=', 62));

// Istemci yeniden kullanilabilir; her istek icin yenisini olusturmayin.
var client = new BtcTurkRestClient();

// ─── 1) Native API: borsa bilgisi ────────────────────────────────────────────
var infoResult = await client.SpotApi.ExchangeData.GetExchangeInfoAsync();
if (!infoResult.Success)
{
    Console.WriteLine($"Basarisiz: {infoResult.Error}");
    return 1;
}

var info = infoResult.Data;
Console.WriteLine($"\n[1] Borsa bilgisi");
Console.WriteLine($"  Sunucu saati (UTC) : {info.ServerTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"  Saat farki         : {(DateTime.UtcNow - info.ServerTime).TotalSeconds:F1} sn");
Console.WriteLine($"  Parite / varlik    : {info.Symbols.Count} / {info.Currencies.Count}");

// ─── 2) Sembol bicimlendirme ─────────────────────────────────────────────────
var native = BtcTurkExchange.FormatSymbol("BTC", "TRY", TradingMode.Spot);
Console.WriteLine($"\n[2] Sembol bicimlendirme");
Console.WriteLine($"  BTC + TRY          -> {native}");
Console.WriteLine($"  BTC + TL (alias)   -> {BtcTurkExchange.FormatSymbol("BTC", "TL", TradingMode.Spot)}");

// ─── 3) Varlik turu borsadan gelir, tahmin edilmez ───────────────────────────
Console.WriteLine($"\n[3] Varlik turleri (borsanin bildirdigi)");
foreach (var asset in new[] { "TRY", "BTC", "USDT" })
{
    var currency = info.Currencies.FirstOrDefault(x => x.Symbol == asset);
    if (currency is not null)
        Console.WriteLine($"  {currency.Symbol,-5} -> {currency.CurrencyType,-7} ({currency.Name})");
}

// ─── 4) Native: ticker, emir defteri, islemler ───────────────────────────────
Console.WriteLine($"\n[4] Native piyasa verisi");

var tickerResult = await client.SpotApi.ExchangeData.GetTickerAsync(native);
if (tickerResult.Success)
{
    var t = tickerResult.Data;
    Console.WriteLine($"  Son fiyat          : {t.LastPrice:N0} TRY  ({t.DailyChangePercentage:+0.00;-0.00}%)");
    Console.WriteLine($"  Alis / satis       : {t.BestBidPrice:N0} / {t.BestAskPrice:N0}");
    Console.WriteLine($"  24s hacim          : {t.Volume:N4} BTC");
}

var bookResult = await client.SpotApi.ExchangeData.GetOrderBookAsync(native, limit: 3);
if (bookResult.Success)
{
    var book = bookResult.Data;
    var spread = book.Asks[0].Price - book.Bids[0].Price;
    Console.WriteLine($"  Emir defteri       : {book.Bids.Count} alis / {book.Asks.Count} satis kademesi");
    Console.WriteLine($"  Makas              : {spread:N0} TRY");
}

var tradesResult = await client.SpotApi.ExchangeData.GetTradesAsync(native, limit: 3);
if (tradesResult.Success)
{
    Console.WriteLine($"  Son islemler       :");
    foreach (var trade in tradesResult.Data)
        Console.WriteLine($"    {trade.Timestamp:HH:mm:ss}  {trade.Side,-4}  {trade.Quantity:N8} @ {trade.Price:N0}");
}

// ─── 5) Shared API: borsadan bagimsiz ────────────────────────────────────────
// Bu bolumdeki kodun hicbir yerinde "BTCTRY" gecmez; donusumu kutuphane yapar.
Console.WriteLine($"\n[5] Shared API (borsadan bagimsiz)");

var sharedSymbol = new SharedSymbol(TradingMode.Spot, "BTC", "TRY");

ISpotSymbolRestClient symbolClient = client.SpotApi.SharedClient;
var symbolsResult = await symbolClient.GetSpotSymbolsAsync(new GetSymbolsRequest());
if (symbolsResult.Success)
{
    Console.WriteLine($"  Shared parite      : {symbolsResult.Data.Length}");

    var catalog = symbolClient.SpotSymbolCatalog;
    if (catalog is not null && catalog.Assets.TryGetValue("TRY", out var tryAsset))
        Console.WriteLine($"  Katalog: TRY       -> {tryAsset.Type}");
}

ISpotTickerRestClient tickerClient = client.SpotApi.SharedClient;
var sharedTicker = await tickerClient.GetSpotTickerAsync(new GetTickerRequest(sharedSymbol));
if (sharedTicker.Success)
    Console.WriteLine($"  Shared ticker      : {sharedTicker.Data.LastPrice:N0} ({sharedTicker.Data.Symbol})");

IOrderBookRestClient bookClient = client.SpotApi.SharedClient;
var sharedBook = await bookClient.GetOrderBookAsync(new GetOrderBookRequest(sharedSymbol, 3));
if (sharedBook.Success)
    Console.WriteLine($"  Shared emir defteri: en iyi alis {sharedBook.Data.Bids[0].Price:N0}");

IRecentTradeRestClient tradeClient = client.SpotApi.SharedClient;
var sharedTrades = await tradeClient.GetRecentTradesAsync(new GetRecentTradesRequest(sharedSymbol, 3));
if (sharedTrades.Success && sharedTrades.Data.Length > 0)
    Console.WriteLine($"  Shared islem       : {sharedTrades.Data[0].Side} {sharedTrades.Data[0].Price:N0}");

// Native ve shared ayni fiyati vermelidir.
if (tickerResult.Success && sharedTicker.Success)
{
    var same = tickerResult.Data.LastPrice == sharedTicker.Data.LastPrice;
    Console.WriteLine($"  Native == Shared   : {same}");
}

// ─── 6) Capability discovery ─────────────────────────────────────────────────
var discovery = client.SpotApi.SharedClient.Discover();
Console.WriteLine($"\n[6] Discover()");
Console.WriteLine($"  Borsa              : {discovery.Exchange}");
Console.WriteLine($"  Islem turleri      : {string.Join(", ", client.SpotApi.SharedClient.SupportedTradingModes)}");

// ─── 7) Hata yollari: istisna firlatilmaz ────────────────────────────────────
Console.WriteLine($"\n[7] Hata yollari");

var unknownSymbol = await client.SpotApi.ExchangeData.GetTickerAsync("YOKBOYLEPARITE");
Console.WriteLine($"  Bilinmeyen parite  : Success={unknownSymbol.Success}");
if (!unknownSymbol.Success)
    Console.WriteLine($"    {unknownSymbol.Error}");

try
{
    // Borsa siniri 50; bu cagri aga hic cikmadan reddedilir.
    await client.SpotApi.ExchangeData.GetTradesAsync(native, limit: 999);
    Console.WriteLine("  Sinir asimi        : BEKLENMEDIK - istisna firlatilmadi");
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine($"  Sinir asimi        : aga cikmadan reddedildi");
}

// ─── 8) Kline: ayri host, saniye cinsinden zaman damgasi ─────────────────────
Console.WriteLine($"\n[8] Kline (graph-api.btcturk.com)");

var klineResult = await client.SpotApi.ExchangeData.GetKlinesAsync(
    native, KlineInterval.OneHour, DateTime.UtcNow.AddHours(-4), DateTime.UtcNow);

if (klineResult.Success)
{
    Console.WriteLine($"  Mum sayisi         : {klineResult.Data.Count}");
    foreach (var k in klineResult.Data.TakeLast(3))
        Console.WriteLine($"    {k.OpenTime:HH:mm}  A={k.OpenPrice,10:N0}  K={k.ClosePrice,10:N0}  H={k.Volume:N4}");
}
else
{
    Console.WriteLine($"  Basarisiz: {klineResult.Error}");
}

// Ayni veri, borsadan bagimsiz cagriyla
IKlineRestClient klineClient = client.SpotApi.SharedClient;
var sharedKlines = await klineClient.GetKlinesAsync(
    new GetKlinesRequest(sharedSymbol, SharedKlineInterval.OneHour,
        DateTime.UtcNow.AddHours(-4), DateTime.UtcNow));

if (sharedKlines.Success)
    Console.WriteLine($"  Shared kline       : {sharedKlines.Data.Length} mum");

// ─── 9) Emirlerin shared yuzeyi ──────────────────────────────────────────────
Console.WriteLine($"\n[9] Emir yuzeyi");

var orderClient = (ISpotOrderRestClient)client.SpotApi.SharedClient;
Console.WriteLine($"  Emir turleri       : {string.Join(", ", orderClient.SpotSupportedOrderTypes)}");

var tif = orderClient.SpotSupportedTimeInForce.Length == 0
    ? "desteklenmiyor"
    : string.Join(", ", orderClient.SpotSupportedTimeInForce);
Console.WriteLine($"  Time-in-force      : {tif}");

// Kimlik bilgisi olmadan cagri basarisiz olur; istisna firlatilmaz.
var openOrders = await orderClient.GetOpenSpotOrdersAsync(new GetOpenOrdersRequest(sharedSymbol));
Console.WriteLine($"  Kimliksiz sorgu    : Success={openOrders.Success} (istisna yok)");

Console.WriteLine("\nTamamlandi.");
return 0;
