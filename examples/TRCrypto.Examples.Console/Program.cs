using CryptoExchange.Net.SharedApis;
using TRCrypto.BtcTurk;
using TRCrypto.BtcTurk.Clients;

// TRCrypto.BtcTurk - canli public API dogrulamasi.
// Bu ornek kimlik bilgisi GEREKTIRMEZ; yalnizca herkese acik piyasa verisi kullanir.

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("TRCrypto.BtcTurk - canli dogrulama");
Console.WriteLine(new string('=', 60));

// Istemci yeniden kullanilabilir; her istek icin yenisini olusturmayin.
var client = new BtcTurkRestClient();

// --- 1) Native API: borsa bilgisi -------------------------------------------
var infoResult = await client.SpotApi.ExchangeData.GetExchangeInfoAsync();
if (!infoResult.Success)
{
    Console.WriteLine($"Basarisiz: {infoResult.Error}");
    return 1;
}

var info = infoResult.Data;
Console.WriteLine($"Sunucu saati (UTC) : {info.ServerTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Yerel saat  (UTC)  : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Saat farki         : {(DateTime.UtcNow - info.ServerTime).TotalSeconds:F1} sn");
Console.WriteLine($"Parite sayisi      : {info.Symbols.Count}");
Console.WriteLine($"Varlik sayisi      : {info.Currencies.Count}");

// --- 2) Sembol bicimlendirme ------------------------------------------------
var native = BtcTurkExchange.FormatSymbol("BTC", "TRY", TradingMode.Spot);
Console.WriteLine($"\nBTC + TRY          -> {native}");
Console.WriteLine($"BTC + TL (alias)   -> {BtcTurkExchange.FormatSymbol("BTC", "TL", TradingMode.Spot)}");

var btcTry = info.Symbols.FirstOrDefault(x => x.Name == native);
if (btcTry is not null)
{
    // numerator/denominator ayri alanlardir; sembol adi ayristirilmaz.
    Console.WriteLine($"  base/quote       : {btcTry.Numerator}/{btcTry.Denominator}");
    Console.WriteLine($"  miktar olcegi    : {btcTry.NumeratorScale}");
    Console.WriteLine($"  fiyat olcegi     : {btcTry.DenominatorScale}");
    Console.WriteLine($"  emir yontemleri  : {string.Join(", ", btcTry.OrderMethods)}");
}

// --- 3) Varlik turu borsadan gelir, tahmin edilmez --------------------------
Console.WriteLine("\nVarlik turleri (borsanin bildirdigi):");
foreach (var symbol in new[] { "TRY", "BTC", "USDT" })
{
    var currency = info.Currencies.FirstOrDefault(x => x.Symbol == symbol);
    if (currency is not null)
        Console.WriteLine($"  {currency.Symbol,-5} -> {currency.CurrencyType,-7} ({currency.Name})");
}

// --- 4) Hata yolu: gecersiz istek istisna firlatmaz -------------------------
var badClient = new BtcTurkRestClient(options =>
    options.Environment = BtcTurkEnvironment.CreateCustom(
        "hatali", "https://api.btcturk.com/gecersiz-yol", "wss://ws-feed-pro.btcturk.com"));

var badResult = await badClient.SpotApi.ExchangeData.GetExchangeInfoAsync();
Console.WriteLine($"\nGecersiz istek -> Success={badResult.Success} (istisna firlatilmadi)");
if (!badResult.Success)
    Console.WriteLine($"  Hata: {badResult.Error}");

Console.WriteLine("\nTamamlandi.");
return 0;
