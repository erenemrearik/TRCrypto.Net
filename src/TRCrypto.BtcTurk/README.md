# TRCrypto.BtcTurk

BtcTurk REST API'si için .NET client kütüphanesi.
[CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) üzerine kuruludur.

> **Resmi değildir.** BtcTurk ile bir bağlantısı yoktur.

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

var result = await client.SpotApi.ExchangeData.GetExchangeInfoAsync();
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}

foreach (var symbol in result.Data.Symbols.Take(5))
    Console.WriteLine($"{symbol.Name}: {symbol.Numerator}/{symbol.Denominator}");
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
var try_ = result.Data.Currencies.Single(x => x.Symbol == "TRY");
Console.WriteLine(try_.CurrencyType);   // Fiat
```

## Hata yönetimi

Hatalar istisna olarak fırlatılmaz. **`Data`'ya erişmeden önce `Success` kontrol edin.**

BtcTurk iş mantığı hatalarını HTTP 200 içinde `"success": false` olarak döndürür; kütüphane
bunu başarısız sonuca çevirir ve borsanın hata kodu/mesajını `Error` içinde taşır.

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
Kütüphane bu limitleri kendisi uygular.

## Kimlik doğrulama

**Bu sürümde henüz yoktur.** Yalnızca kimlik doğrulama gerektirmeyen piyasa verisi uçları
desteklenir. Anahtar alma rehberi: [docs/credentials/btcturk.md](https://github.com/TRCryptoNet/TRCrypto.Net/blob/main/docs/credentials/btcturk.md)

## Desteklenen uçlar

| Uç | Metod | Durum |
|---|---|---|
| `/api/v2/server/exchangeinfo` | `GetExchangeInfoAsync` | ✅ |
| Sunucu saati (exchangeinfo'dan) | `GetServerTimeAsync` | ✅ |
| `/api/v2/ticker` | — | ⏳ |
| `/api/v2/orderbook` | — | ⏳ |
| `/api/v2/trades` | — | ⏳ |
| WebSocket | — | ⏳ |
| SharedApis | — | ⏳ |

Resmi API belgeleri: [docs.btcturk.com](https://docs.btcturk.com/)

## Lisans

MIT
