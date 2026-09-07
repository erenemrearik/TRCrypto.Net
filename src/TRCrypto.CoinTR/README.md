# TRCrypto.CoinTR

[![License](https://img.shields.io/badge/lisans-MIT-blue?style=flat-square)](https://github.com/erenemrearik/TRCrypto.Net/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%20|%209%20|%2010%20|%20standard2.0%20|%20standard2.1-512BD4?style=flat-square&logo=dotnet&logoColor=white)](#kurulum)
[![Durum](https://img.shields.io/badge/durum-public%20REST%20+%20WebSocket-yellow?style=flat-square)](#desteklenen-uçlar)

CoinTR REST ve WebSocket API'leri için .NET client kütüphanesi.
[CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) üzerine kuruludur.

> **Resmi değildir.** Bu paket bağımsız bir çalışmadır; CoinTR ile bir bağlantısı yoktur.

## Kurulum

```bash
dotnet add package TRCrypto.CoinTR
```

Hedef platformlar: `net8.0` · `net9.0` · `net10.0` · `netstandard2.0` · `netstandard2.1`

## Hızlı başlangıç

```csharp
using TRCrypto.CoinTR.Clients;

var client = new CoinTRRestClient();

var result = await client.SpotApi.ExchangeData.GetTickersAsync("BTCTRY");
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}

var ticker = result.Data[0];
Console.WriteLine($"{ticker.Symbol}: {ticker.LastPrice:N0} (%{ticker.ChangePercentage:N2})");
```

## Sembol formatı

CoinTR sembolleri **birleşik ve büyük harf** yazar: `BTCTRY`. BtcTurk ile aynı biçimdir,
Binance TR alt çizgi kullanır (`BTC_TRY`), Paribu ise küçük harf ve alt çizgi (`btc_tl`).

```csharp
CoinTRExchange.FormatSymbol("BTC", "TRY", TradingMode.Spot);  // "BTCTRY"
CoinTRExchange.FormatSymbol("BTC", "TL",  TradingMode.Spot);  // "BTCTRY" - TL takma adi
```

Yanıtlarda base ve quote varlık ayrı alanlarda gelir; sembol adını ayrıştırmanız
gerekmez.

## Bilinmesi gereken üç ayrıntı

> [!IMPORTANT]
> **Değişim oranı kesirdir, yüzde değil.** `change24h` alanı `-0.00912` gönderir ve bu
> yüzde 0,912 düşüş demektir. Değeri doğrudan yüzde sanmak onu yüz kat küçük gösterir.
> Kütüphane bunu `ChangePercentage` üzerinden çevirir; ham oran `ChangeRatio` alanında
> kalır.

> [!IMPORTANT]
> **Bütün sayısal değerler metin olarak gelir**: fiyat, miktar, hacim, ondalık basamak
> sayısı ve komisyon oranı dahil. Kütüphane bunu kendi ayrıştırıcısında çözer.

> [!NOTE]
> Zarf başarıyı `code == "00000"` ile bildirir ve bu değer bir **metindir**. Bu, projedeki
> dördüncü farklı zarf biçimidir: BtcTurk mantıksal bir `success` alanı, Binance TR
> sayısal bir `code` kullanır, Paribu ise hiç zarf kullanmaz.

## Emir defteri kademe sayısı

REST tarafında `limit` gerçekten uygulanır: 5 istendiğinde 5 kademe döner.

WebSocket tarafında ise kademe sayısı **kanal adının parçasıdır** (`books5`). Borsa
yalnızca **1, 5 ve 15** için kanal tanımlar. Listede olmayan bir değerde abonelik onay
almaz ve hiçbir veri gelmez; hata da dönmez. Kütüphane bunu ağa çıkmadan reddeder.

## Hata yönetimi

Hatalar istisna olarak fırlatılmaz. **`Data`'ya erişmeden önce `Success` kontrol edin.**

Borsa başarısız işlemleri HTTP 200 içinde `"00000"` dışında bir `code` ile bildirir.
Kütüphane bunu başarısız sonuca çevirir.

## Kimlik doğrulama

Bu sürümde yalnızca herkese açık uçlar ve akışlar sunulur; hiçbiri anahtar istemez.

İmzalama sağlayıcısı hazırdır ancak private uçlar canlı bir hesapla henüz
denenmediğinden yayımlanmamıştır. Kimlik bilgisi burada **üç parçalıdır**: anahtar,
secret ve bir **parola**. İmzalanan değer `zaman damgası + METOT + yol + sorgu + gövde`
birleşimidir; dört borsa arasında yolu imzaya katan tek borsa budur.

> [!WARNING]
> Resmi dokümantasyondaki imzalama tarifi çalışmıyor. Kütüphanedeki şema çalışan bir
> entegrasyondan doğrulandı. Ayrıntı ve dört borsanın karşılaştırması:
> [docs/vendor/cointr-capabilities.md](https://github.com/erenemrearik/TRCrypto.Net/blob/main/docs/vendor/cointr-capabilities.md)

Anahtar alma ve izinler:
[docs/credentials/cointr.md](https://github.com/erenemrearik/TRCrypto.Net/blob/main/docs/credentials/cointr.md)

## Desteklenen uçlar

| Uç | Metod | Durum |
|---|---|---|
| `/api/v2/public/time` | `GetServerTimeAsync` | ✅ |
| `/api/v2/spot/public/symbols` | `GetSymbolsAsync` | ✅ |
| `/api/v2/spot/market/tickers` | `GetTickersAsync` | ✅ |
| `/api/v2/spot/market/orderbook` | `GetOrderBookAsync` | ✅ |
| `/api/v2/spot/market/fills` | `GetTradesAsync` | ✅ |
| `/api/v2/spot/market/candles` | `GetKlinesAsync` | ✅ |
| WS ticker | `SubscribeToTickerUpdatesAsync` | ✅ |
| WS emir defteri | `SubscribeToOrderBookUpdatesAsync` | ✅ |
| WS işlemler | `SubscribeToTradeUpdatesAsync` | ✅ |
| SharedApis (REST) | `ISpotSymbolRestClient` · `ISpotTickerRestClient` · `IOrderBookRestClient` · `IRecentTradeRestClient` · `IKlineRestClient` | ✅ |
| SharedApis (socket) | `ITickerSocketClient` · `ITradeSocketClient` · `IOrderBookSocketClient` | ✅ |
| Bakiye, emir ve işlem geçmişi | yok | ⏳ İmza canlı doğrulanmadan yayımlanmayacak |
| Vadeli işlemler | yok | Kapsam dışı |
| Yatırma ve çekme | yok | Kapsam dışı (ADR-007) |

## Bağımlılık enjeksiyonu

Tek çağrı hem REST hem WebSocket istemcisini kaydeder.

```csharp
builder.Services.AddTRCryptoCoinTR();

public sealed class PriceService(ICoinTRRestClient rest, ICoinTRSocketClient socket)
{
    // Enjekte edilen istemcileri yeniden kullanın; socket istemcisi tekildir.
}
```

Resmi API belgeleri: [cointr-ex.github.io/openapis](https://cointr-ex.github.io/openapis/)

## Lisans

MIT
