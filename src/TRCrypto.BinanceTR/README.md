# TRCrypto.BinanceTR

[![License](https://img.shields.io/badge/lisans-MIT-blue?style=flat-square)](https://github.com/erenemrearik/TRCrypto.Net/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%20|%209%20|%2010%20|%20standard2.0%20|%20standard2.1-512BD4?style=flat-square&logo=dotnet&logoColor=white)](#kurulum)
[![Durum](https://img.shields.io/badge/durum-REST%20+%20WebSocket%20+%20hesap-yellow?style=flat-square)](#desteklenen-uçlar)

Binance TR REST ve WebSocket API'leri için .NET client kütüphanesi.
[CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) üzerine kuruludur.

> **Resmi değildir.** Bu paket bağımsız bir çalışmadır; Binance TR ile bir bağlantısı yoktur.

## Kurulum

```bash
dotnet add package TRCrypto.BinanceTR
```

Hedef platformlar: `net8.0` · `net9.0` · `net10.0` · `netstandard2.0` · `netstandard2.1`

## Hızlı başlangıç

```csharp
using TRCrypto.BinanceTR.Clients;

var client = new BinanceTRRestClient();

var result = await client.SpotApi.ExchangeData.GetOrderBookAsync("BTC_TRY", limit: 5);
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}

Console.WriteLine($"En iyi alis: {result.Data.Bids[0].Price:N0}");
```

## Sembol formatı

> [!IMPORTANT]
> Binance TR sembollerinde **alt çizgi** kullanır: `BTC_TRY`.
> BtcTurk ise birleşik yazar (`BTCTRY`). İki borsa için tek bir biçimlendirme kullanılamaz.

```csharp
BinanceTRExchange.FormatSymbol("BTC", "TRY", TradingMode.Spot);  // "BTC_TRY"
BinanceTRExchange.FormatSymbol("BTC", "TL",  TradingMode.Spot);  // "BTC_TRY" - TL takma adi
```

Yanıtlarda base/quote ayrı alanlarda gelir; sembol adını ayrıştırmanız gerekmez.

## Bilinmesi gereken sınırlar

> [!WARNING]
> **Ticker verisi anahtarsız alınamıyor.** Global Binance'te herkese açık olan
> `/api/v3/*` uçları burada API anahtarı ister. Bu nedenle bu sürümde ticker desteği yok.

Ayrıca borsanın `market/trades` ve `market/klines` uçları şu an **başarılı görünüp boş
liste** döndürüyor (denenen tüm paritelerde). İşlem verisi için toplu işlemler ucu,
mum verisi için WebSocket akışı kullanılmalıdır. Ayrıntı:
[docs/vendor/binance-tr-capabilities.md](https://github.com/erenemrearik/TRCrypto.Net/blob/main/docs/vendor/binance-tr-capabilities.md)

## Emir defteri kademe sayısı

Borsa yalnızca şu değerleri kabul eder: **5, 10, 20, 50, 100, 500, 1000**.

Diğer değerler yanıltıcı bir `Incorrect Page number` hatasıyla reddedilir. Hata mesajı
sorunun limit olduğunu söylemez. Kütüphane bunu **ağa çıkmadan** reddeder.

## Hata yönetimi

Hatalar istisna olarak fırlatılmaz. **`Data`'ya erişmeden önce `Success` kontrol edin.**

Binance TR başarıyı `code == 0` ile bildirir; bir `success` alanı yoktur ve hatalar da
HTTP 200 içinde döner. Kütüphane bunu başarısız sonuca çevirir.

## Kimlik doğrulama

Bakiye, emir ve işlem geçmişi uçları API anahtarı ister. İmzalama şeması resmi
dokümantasyondan alındı ve yayımlanmış imza test vektörüyle doğrulandı.

```csharp
var client = new BinanceTRRestClient(options =>
{
    options.ApiCredentials = new BinanceTRCredentials(key, secret);
});

var account = await client.SpotApi.Account.GetAccountAsync();
```

> [!NOTE]
> Private uçlar henüz **canlı bir hesaba karşı çalıştırılmadı.** Şema doğru kabul
> ediliyor ancak bunun kanıtı bir anahtar bağlandığında elde edilecek; hazır bekleyen
> sonda testleri (`AuthenticationProbeTests`) bunu ilk çalıştırmada bildirir.

> [!WARNING]
> Sistem saatiniz sunucuyla uyumlu olmalıdır. Borsa imzalı istekleri yalnızca
> `recvWindow` içinde kabul eder ve varsayılan pencere **5000 ms**'dir. Bu, BtcTurk'ün
> toleransına göre dardır: birkaç saniyelik kayma orada sorun çıkarmazken burada tüm
> imzalı istekleri reddettirir.

Anahtar alma, izinler ve imzalama şemasının BtcTurk'ten farkları:
[docs/credentials/binance-tr.md](https://github.com/erenemrearik/TRCrypto.Net/blob/main/docs/credentials/binance-tr.md)

## Desteklenen uçlar

| Uç | Metod | Durum |
|---|---|---|
| `/open/v1/common/time` | `GetServerTimeAsync` | ✅ |
| `/open/v1/common/symbols` | `GetSymbolsAsync` | ✅ |
| `/open/v1/market/depth` | `GetOrderBookAsync` | ✅ |
| `/open/v1/market/agg-trades` | `GetAggregatedTradesAsync` | ✅ |
| Ticker (REST) | yok | ❌ Anahtarsız mümkün değil |
| Kline (REST) | yok | ⚠️ Borsa boş liste döndürüyor |
| `/open/v1/account/spot` | `Account.GetAccountAsync` | ✅ |
| `/open/v1/orders` (POST) | `Trading.PlaceOrderAsync` | ✅ |
| `/open/v1/orders` (GET) | `Trading.GetOrdersAsync` | ✅ |
| `/open/v1/orders/detail` | `Trading.GetOrderAsync` | ✅ |
| `/open/v1/orders/cancel` | `Trading.CancelOrderAsync` | ✅ |
| `/open/v1/orders/trades` | `Trading.GetUserTradesAsync` | ✅ |
| WS ticker | `SubscribeToTickerUpdatesAsync` | ✅ |
| WS trade · aggTrade | `SubscribeToTradeUpdatesAsync` vb. | ✅ |
| WS emir defteri | `SubscribeToOrderBookUpdatesAsync` | ✅ |
| WS kline | `SubscribeToKlineUpdatesAsync` | ✅ |
| SharedApis (bakiye ve emir) | `IBalanceRestClient` · `ISpotOrderRestClient` | ✅ |
| SharedApis (REST) | `ISpotSymbolRestClient` · `IOrderBookRestClient` · `IRecentTradeRestClient` | ✅ |
| SharedApis (socket) | `ITickerSocketClient` · `ITradeSocketClient` · `IOrderBookSocketClient` · `IKlineSocketClient` | ✅ |

## Bağımlılık enjeksiyonu

Tek çağrı hem REST hem WebSocket istemcisini kaydeder.

```csharp
builder.Services.AddTRCryptoBinanceTR();

public sealed class PriceService(IBinanceTRRestClient rest, IBinanceTRSocketClient socket)
{
    // Enjekte edilen istemcileri yeniden kullanın; socket istemcisi tekildir.
}
```

Resmi API belgeleri: [binance.tr/apidocs](https://www.binance.tr/apidocs)

## Lisans

MIT
