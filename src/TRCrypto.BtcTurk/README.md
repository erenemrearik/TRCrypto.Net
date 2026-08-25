# TRCrypto.BtcTurk

BtcTurk REST API'si için .NET client kütüphanesi.
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

İstemci yeniden kullanılabilir ve iş parçacığı güvenlidir — her istek için yenisini oluşturmayın.

## Native API

```csharp
var api = client.SpotApi.ExchangeData;

await api.GetExchangeInfoAsync();                    // pariteler, varliklar, sunucu saati
await api.GetServerTimeAsync();                      // sunucu saati
await api.GetTickersAsync();                         // tum pariteler
await api.GetTickerAsync("BTCTRY");                  // tek parite
await api.GetTickersByQuoteAssetAsync("TRY");        // TRY paritelerinin tamami
await api.GetOrderBookAsync("BTCTRY", limit: 25);    // emir defteri
await api.GetTradesAsync("BTCTRY", limit: 50);       // son islemler (en fazla 50)
```

## Shared API — borsadan bağımsız

Aynı kodun farklı borsalarla çalışmasını sağlar. Native sembol formatı hiç görünmez:

```csharp
using CryptoExchange.Net.SharedApis;

ISpotTickerRestClient tickers = client.SpotApi.SharedClient;

var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "TRY");
var result = await tickers.GetSpotTickerAsync(new GetTickerRequest(symbol));

if (result.Success)
    Console.WriteLine(result.Data.LastPrice);
```

Uygulanan arayüzler: `ISpotSymbolRestClient` · `ISpotTickerRestClient` ·
`IOrderBookRestClient` · `IRecentTradeRestClient`

Bir borsanın hangi yetenekleri desteklediğini çalışma anında öğrenmek için:

```csharp
var info = client.SpotApi.SharedClient.Discover();
```

## Sembol formatı

BtcTurk sembolleri birleşik ve büyük harf bekler: `BTCTRY`.

```csharp
BtcTurkExchange.FormatSymbol("BTC", "TRY", TradingMode.Spot);  // "BTCTRY"
BtcTurkExchange.FormatSymbol("BTC", "TL",  TradingMode.Spot);  // "BTCTRY" - TL takma adi
```

Yanıtlarda base/quote ayrı alanlar olarak gelir (`Numerator` / `Denominator`); sembol adını
ayrıştırmanız gerekmez.

## Varlık türleri

Borsa her varlığın türünü kendisi bildirir; sembol adından tahmin edilmez.

```csharp
var info = await client.SpotApi.ExchangeData.GetExchangeInfoAsync();
var tryAsset = info.Data.Currencies.Single(x => x.Symbol == "TRY");

Console.WriteLine(tryAsset.CurrencyType);   // Fiat
```

Shared katmanında bu bilgi `SharedAssetType` olarak sunulur.

## Hata yönetimi

Hatalar istisna olarak fırlatılmaz. **`Data`'ya erişmeden önce `Success` kontrol edin.**

```csharp
var result = await client.SpotApi.ExchangeData.GetTickerAsync("YOKBOYLEPARITE");
// result.Success == false
// result.Error   == [ServerError.UnknownSymbol] ...
```

BtcTurk iş mantığı hatalarını HTTP 200 içinde `"success": false` olarak döndürür; kütüphane
bunu başarısız sonuca çevirir ve borsanın kodu/mesajını `Error` içinde taşır.

Geçersiz girdiler ağa çıkılmadan reddedilir (`ArgumentException` /
`ArgumentOutOfRangeException`) — örneğin 50'den fazla işlem istemek.

## Bağımlılık enjeksiyonu

```csharp
builder.Services.AddTRCryptoBtcTurk();

public sealed class PriceService(IBtcTurkRestClient client)
{
    // enjekte edilen istemciyi yeniden kullanin
}
```

## İstek limitleri

Public uçlar IP bazlı sınırlıdır (ticker 600/dk, emir defteri 180/dk, OHLC 120/dk).
Kütüphane limitleri kendisi uygular; gerekirse bekler.

## Kimlik doğrulama

**Bu sürümde henüz yoktur.** Yalnızca kimlik doğrulama gerektirmeyen piyasa verisi uçları
desteklenir. Anahtar alma ve bağlama rehberi:
[docs/credentials/btcturk.md](https://github.com/erenemrearik/TRCrypto.Net/blob/main/docs/credentials/btcturk.md)

## Desteklenen uçlar

| Uç | Metod | Shared |
|---|---|---|
| `/api/v2/server/exchangeinfo` | `GetExchangeInfoAsync` · `GetServerTimeAsync` | `ISpotSymbolRestClient` |
| `/api/v2/ticker` | `GetTickersAsync` · `GetTickerAsync` | `ISpotTickerRestClient` |
| `/api/v2/ticker/currency` | `GetTickersByQuoteAssetAsync` | — |
| `/api/v2/orderbook` | `GetOrderBookAsync` | `IOrderBookRestClient` |
| `/api/v2/trades` | `GetTradesAsync` | `IRecentTradeRestClient` |
| OHLC / kline | ⏳ | ⏳ |
| Bakiye · emir işlemleri | ⏳ (kimlik doğrulama gerekir) | ⏳ |
| WebSocket | ⏳ | ⏳ |

Resmi API belgeleri: [docs.btcturk.com](https://docs.btcturk.com/)

## Lisans

MIT
