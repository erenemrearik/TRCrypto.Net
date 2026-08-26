# TRCrypto.BtcTurk

[![License](https://img.shields.io/badge/lisans-MIT-blue?style=flat-square)](https://github.com/erenemrearik/TRCrypto.Net/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%20|%209%20|%2010%20|%20standard2.0%20|%20standard2.1-512BD4?style=flat-square&logo=dotnet&logoColor=white)](#kurulum)
[![CryptoExchange.Net](https://img.shields.io/badge/CryptoExchange.Net-12.5.0-orange?style=flat-square)](https://github.com/JKorf/CryptoExchange.Net)

BtcTurk REST ve WebSocket API'leri için .NET client kütüphanesi.
[CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) üzerine kuruludur.

> **Resmi değildir.** Bu paket bağımsız bir çalışmadır; BtcTurk ile bir bağlantısı yoktur.

## Kurulum

```bash
dotnet add package TRCrypto.BtcTurk
```

Hedef platformlar: `net8.0` · `net9.0` · `net10.0` · `netstandard2.0` · `netstandard2.1`

## Hızlı başlangıç

Piyasa verisi herkese açıktır; API anahtarı gerekmez.

```csharp
using TRCrypto.BtcTurk.Clients;

var client = new BtcTurkRestClient();

var result = await client.SpotApi.ExchangeData.GetTickerAsync("BTCTRY");
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}

Console.WriteLine($"BTC/TRY: {result.Data.LastPrice:N0}");
```

İstemci yeniden kullanılabilir ve iş parçacığı güvenlidir — her istek için yenisini
oluşturmayın.

## Native API

```csharp
var api = client.SpotApi.ExchangeData;

await api.GetExchangeInfoAsync();                    // pariteler, varliklar, sunucu saati
await api.GetServerTimeAsync();
await api.GetTickersAsync();                         // tum pariteler
await api.GetTickerAsync("BTCTRY");                  // tek parite
await api.GetTickersByQuoteAssetAsync("TRY");        // TRY pariteleri
await api.GetOrderBookAsync("BTCTRY", limit: 25);
await api.GetTradesAsync("BTCTRY", limit: 50);       // en fazla 50
await api.GetKlinesAsync("BTCTRY", KlineInterval.OneHour);
```

Kimlik doğrulama gerektirenler:

```csharp
await client.SpotApi.Account.GetBalancesAsync();
await client.SpotApi.Account.GetUserTradesAsync("BTCTRY");

await client.SpotApi.Trading.GetOpenOrdersAsync("BTCTRY");
await client.SpotApi.Trading.GetOrdersAsync("BTCTRY", limit: 100);
await client.SpotApi.Trading.GetOrderAsync(orderId);
```

## Gerçek zamanlı akışlar

```csharp
using TRCrypto.BtcTurk.Clients;

var socket = new BtcTurkSocketClient();

var sub = await socket.SpotApi.SubscribeToTickerUpdatesAsync("BTCTRY",
    update => Console.WriteLine(update.Data.LastPrice));

if (!sub.Success)
{
    Console.WriteLine(sub.Error);
    return;
}

// Kapanista abonelikleri kapatin
await sub.Data.CloseAsync();
```

Kanallar: ticker, işlemler ve emir defteri. Bağlantı koptuğunda kütüphane yeniden bağlanır
ve açık abonelikleri kendiliğinden yeniden kurar.

Emir defteri güncellemeleri bir **sıra numarası** taşır (`Sequence`); numarada atlama
görürseniz defteri geçersiz sayıp yeni bir görüntü almalısınız.

Handler'ları kısa tutun — ağır işi bir kuyruğa aktarın.

> Kullanıcıya özel akışlar (emir/bakiye bildirimleri) henüz uygulanmadı.

## Emir işlemleri

```csharp
var client = new BtcTurkRestClient(options =>
    options.ApiCredentials = new BtcTurkCredentials(apiKey, apiSecret));

var placed = await client.SpotApi.Trading.PlaceOrderAsync(
    "BTCTRY", OrderSide.Buy, OrderMethod.Limit, quantity: 0.001m, price: 3_000_000m);

if (placed.Success)
    Console.WriteLine(placed.Data.Id);

await client.SpotApi.Trading.CancelOrderAsync(placed.Data.Id);
```

> ⚠️ **Emir iptali eşzamansızdır.** Başarılı bir iptal yanıtı isteğin *alındığını* bildirir;
> kesinleşme WebSocket üzerinden duyurulur. Emrin gerçekten iptal edildiğini varsaymak
> yerine durumunu ayrıca sorgulayın.
>
> ⚠️ Emir oluşturma istekleri zaman aşımında **otomatik olarak yeniden denenmez** — emir
> borsada oluşmuş olabilir. Önce durumu doğrulayın.

Geçersiz emirler ağa çıkılmadan reddedilir: eksik fiyat, negatif miktar, stop emrinde
eksik tetikleme fiyatı.

## Shared API — borsadan bağımsız

Aynı kodun farklı borsalarla çalışmasını sağlar. Native sembol formatı hiç görünmez:

```csharp
using CryptoExchange.Net.SharedApis;

ISpotTickerRestClient tickers = client.SpotApi.SharedClient;

var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "TRY");
var result = await tickers.GetSpotTickerAsync(new GetTickerRequest(symbol));
```

**REST:** `ISpotSymbolRestClient` · `ISpotTickerRestClient` · `IOrderBookRestClient` ·
`IRecentTradeRestClient` · `IKlineRestClient` · `IBalanceRestClient` · `ISpotOrderRestClient`

**WebSocket:** `ITickerSocketClient` · `ITradeSocketClient` · `IOrderBookSocketClient`

Çalışma anında yetenek keşfi:

```csharp
var info = client.SpotApi.SharedClient.Discover();
```

## Sembol formatı

BtcTurk sembolleri birleşik ve büyük harf bekler: `BTCTRY`.

```csharp
BtcTurkExchange.FormatSymbol("BTC", "TRY", TradingMode.Spot);  // "BTCTRY"
BtcTurkExchange.FormatSymbol("BTC", "TL",  TradingMode.Spot);  // "BTCTRY" - TL takma adi
```

Yanıtlarda base/quote ayrı alanlar olarak gelir; sembol adını ayrıştırmanız gerekmez.

## Varlık türleri

Borsa her varlığın türünü kendisi bildirir; sembol adından tahmin edilmez.

```csharp
var info = await client.SpotApi.ExchangeData.GetExchangeInfoAsync();
var tryAsset = info.Data.Currencies.Single(x => x.Symbol == "TRY");

Console.WriteLine(tryAsset.CurrencyType);   // Fiat
```

## Hata yönetimi

Hatalar istisna olarak fırlatılmaz. **`Data`'ya erişmeden önce `Success` kontrol edin.**

BtcTurk iş mantığı hatalarını HTTP 200 içinde `"success": false` olarak döndürür; kütüphane
bunu başarısız sonuca çevirir ve borsanın kodu/mesajını `Error` içinde taşır.

## Bağımlılık enjeksiyonu

```csharp
builder.Services.AddTRCryptoBtcTurk();

public sealed class PriceService(IBtcTurkRestClient client)
{
    // enjekte edilen istemciyi yeniden kullanin
}
```

## İstek limitleri

Public uçlar IP bazlı sınırlıdır (ticker 600/dk, emir defteri 180/dk, OHLC 120/dk,
grafik API 600/10dk). WebSocket bağlantı isteği dakikada 15 ile sınırlıdır.
Kütüphane limitleri kendisi uygular.

## Kimlik doğrulama

Bakiye ve emir uçları API anahtarı gerektirir. Anahtar alma ve bağlama rehberi:
[docs/credentials/btcturk.md](https://github.com/erenemrearik/TRCrypto.Net/blob/main/docs/credentials/btcturk.md)

> Kütüphane hiçbir özelliği için **çekim (withdrawal) iznine** ihtiyaç duymaz.
> Anahtarınızda bu izni açmayın.

## Desteklenen uçlar

| Uç | Metod | Shared |
|---|---|---|
| `/api/v2/server/exchangeinfo` | `GetExchangeInfoAsync` · `GetServerTimeAsync` | `ISpotSymbolRestClient` |
| `/api/v2/ticker` | `GetTickersAsync` · `GetTickerAsync` | `ISpotTickerRestClient` |
| `/api/v2/ticker/currency` | `GetTickersByQuoteAssetAsync` | — |
| `/api/v2/orderbook` | `GetOrderBookAsync` | `IOrderBookRestClient` |
| `/api/v2/trades` | `GetTradesAsync` | `IRecentTradeRestClient` |
| Kline (graph-api) | `GetKlinesAsync` | `IKlineRestClient` |
| `/api/v1/users/balances` | `Account.GetBalancesAsync` | `IBalanceRestClient` |
| `/api/v1/users/transactions/trade` | `Account.GetUserTradesAsync` | `ISpotOrderRestClient` |
| `/api/v1/openOrders` | `Trading.GetOpenOrdersAsync` | `ISpotOrderRestClient` |
| `/api/v1/allOrders` | `Trading.GetOrdersAsync` | `ISpotOrderRestClient` |
| `/api/v1/order/{id}` | `Trading.GetOrderAsync` | `ISpotOrderRestClient` |
| `POST /api/v1/order` | `Trading.PlaceOrderAsync` | `ISpotOrderRestClient` |
| `DELETE /api/v1/order` | `Trading.CancelOrderAsync` | `ISpotOrderRestClient` |
| WebSocket: ticker · trade · orderbook | `SocketClient.SpotApi.SubscribeTo…` | `ITickerSocketClient` · `ITradeSocketClient` · `IOrderBookSocketClient` |
| Kullanıcı socket akışları | ⏳ | ⏳ |

Resmi API belgeleri: [docs.btcturk.com](https://docs.btcturk.com/)

## Lisans

MIT
