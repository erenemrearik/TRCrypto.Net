# TRCrypto.Net

Türkiye'deki kripto varlık platformları için .NET client ekosistemi.
[CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) üzerine kuruludur.

> **Resmi değildir.** Bu proje bağımsız bir çalışmadır; BtcTurk, Binance TR, Paribu, Bitexen
> veya JKorf ile resmi bir bağlantısı yoktur. Borsaların API'lerinde yapacağı değişikliklerden
> veya hizmet kesintilerinden sorumlu değildir.

## Neden?

Türkiye borsalarının API yüzeyleri, kimlik doğrulama şemaları, sembol formatları ve WebSocket
protokolleri birbirinden farklıdır. Her geliştirici HttpClient, HMAC imzalama, yeniden bağlanma
ve model eşleme kodunu yeniden yazmak zorunda kalır.

TRCrypto her borsa için iki yüzey sunar:

- **Native API** — borsanın tüm özelliklerine ve özgün modellerine erişim
- **Shared API** — `CryptoExchange.Net.SharedApis` üzerinden borsadan bağımsız kod

## Durum

Proje geliştirmenin erken aşamasındadır.

| Paket | Kapsam | Durum |
|---|---|---|
| `TRCrypto.BtcTurk` | Public piyasa verisi (REST + SharedApis) | 🚧 Geliştiriliyor |
| `TRCrypto.BinanceTR` | — | ⏳ Planlandı |
| `TRCrypto.Paribu` | — | ⏳ Planlandı |
| `TRCrypto.Bitexen` | — | ⏳ Planlandı |
| `TRCrypto.Clients` | Toplu paket | ⏳ Planlandı |

**Şu an çalışan:** BtcTurk public piyasa verisi — pariteler, varlıklar, sunucu saati, ticker,
emir defteri, son işlemler; hem native hem SharedApis üzerinden.

**Henüz yok:** kimlik doğrulama, bakiye, emir işlemleri, WebSocket, OHLC/kline.

## Hızlı başlangıç

```csharp
using TRCrypto.BtcTurk.Clients;

// Piyasa verisi icin kimlik bilgisi gerekmez
var client = new BtcTurkRestClient();

var result = await client.SpotApi.ExchangeData.GetTickerAsync("BTCTRY");
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}

Console.WriteLine($"BTC/TRY: {result.Data.LastPrice:N0}");
```

Borsadan bağımsız (shared) kullanım — native sembol formatı hiç görünmez:

```csharp
using CryptoExchange.Net.SharedApis;

ISpotTickerRestClient tickers = client.SpotApi.SharedClient;
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "TRY");

var ticker = await tickers.GetSpotTickerAsync(new GetTickerRequest(symbol));
```

İstemci yeniden kullanılabilir ve iş parçacığı güvenlidir — her istek için yenisini oluşturmayın.

## Hata yönetimi

API hataları istisna olarak fırlatılmaz, sonuç nesnesi olarak döner. **`Data`'ya erişmeden
önce her zaman `Success` kontrol edin.**

BtcTurk iş mantığı hatalarını HTTP 200 içinde `"success": false` olarak döndürür; kütüphane
bunu başarısız bir sonuca çevirir.

## Belgeler

| Konu | Dosya |
|---|---|
| API anahtarı alma ve bağlama | [docs/credentials/](docs/credentials/) |
| BtcTurk endpoint envanteri | [docs/vendor/btcturk-capabilities.md](docs/vendor/btcturk-capabilities.md) |
| Teknik spesifikasyon | [docs/spec/](docs/spec/) |

### Projeye katkı

| Konu | Dosya |
|---|---|
| **Nerede kaldık, sonraki adım** | [docs/DURUM.md](docs/DURUM.md) |
| Katkı rehberi | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Güvenlik açığı bildirimi | [SECURITY.md](SECURITY.md) |
| Değişiklik günlüğü | [CHANGELOG.md](CHANGELOG.md) |

## Geliştirme

```bash
dotnet build -c Release
dotnet test  -c Release
dotnet run --project examples/TRCrypto.Examples.Console   # canli public API dogrulamasi
```

### Secret koruması

Depoda gerçek kimlik bilgisi bulunmaz. Katkı vermeden önce pre-commit kancasını etkinleştirin:

```bash
git config core.hooksPath .githooks
```

Kanca, hazırlanan değişikliklerde secret araması yapar. Ayrıntılar:
[docs/credentials/README.md](docs/credentials/README.md).

## Hedef platformlar

`net8.0` · `net9.0` · `net10.0` · `netstandard2.0` · `netstandard2.1`

## Lisans

MIT — bkz. [LICENSE](LICENSE).
