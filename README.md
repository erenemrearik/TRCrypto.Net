<div align="center">

# TRCrypto.Net

**Türkiye'deki kripto varlık platformları için .NET client ekosistemi**

[![CI](https://img.shields.io/github/actions/workflow/status/erenemrearik/TRCrypto.Net/ci.yml?branch=main&label=CI&logo=github&style=flat-square)](https://github.com/erenemrearik/TRCrypto.Net/actions/workflows/ci.yml)
[![Tests](https://img.shields.io/badge/testler-185%20geçiyor-brightgreen?style=flat-square&logo=xunit&logoColor=white)](tests/)
[![License](https://img.shields.io/badge/lisans-MIT-blue?style=flat-square)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%20|%209%20|%2010%20|%20standard2.0%20|%20standard2.1-512BD4?style=flat-square&logo=dotnet&logoColor=white)](#hedef-platformlar)

[![CryptoExchange.Net](https://img.shields.io/badge/CryptoExchange.Net-12.5.0-orange?style=flat-square)](https://github.com/JKorf/CryptoExchange.Net)
[![Durum](https://img.shields.io/badge/durum-geliştirme%20aşamasında-yellow?style=flat-square)](docs/DURUM.md)
[![NuGet](https://img.shields.io/badge/NuGet-henüz%20yayınlanmadı-lightgrey?style=flat-square&logo=nuget)](#durum)

[JKorf/CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) üzerine kuruludur.

</div>

> [!IMPORTANT]
> **Bu proje resmi değildir.** Bağımsız bir çalışmadır; BtcTurk, Binance TR, Paribu,
> Bitexen veya JKorf ile resmi bir bağlantısı yoktur. Borsaların API'lerinde yapacağı
> değişikliklerden veya hizmet kesintilerinden sorumlu değildir.

---

## Neden?

Türkiye borsalarının API yüzeyleri, kimlik doğrulama şemaları, sembol formatları ve
WebSocket protokolleri birbirinden farklıdır. Her geliştirici HttpClient, HMAC imzalama,
yeniden bağlanma ve model eşleme kodunu yeniden yazmak zorunda kalır.

TRCrypto her borsa için **iki yüzey** sunar:

| Yüzey | Ne işe yarar |
|---|---|
| **Native API** | Borsanın tüm özelliklerine ve özgün alanlarına erişim |
| **Shared API** | `CryptoExchange.Net.SharedApis` ile borsadan bağımsız kod |

---

## Durum

<table>
<tr><th>Paket</th><th>Kapsam</th><th>Durum</th></tr>
<tr>
  <td><code>TRCrypto.BtcTurk</code></td>
  <td>REST + WebSocket + SharedApis</td>
  <td><img src="https://img.shields.io/badge/geliştiriliyor-yellow?style=flat-square" alt="geliştiriliyor"></td>
</tr>
<tr>
  <td><code>TRCrypto.BinanceTR</code></td>
  <td>Public piyasa verisi (REST + WebSocket)</td>
  <td><img src="https://img.shields.io/badge/geliştiriliyor-yellow?style=flat-square" alt="geliştiriliyor"></td>
</tr>
<tr>
  <td><code>TRCrypto.Paribu</code></td><td>—</td>
  <td><img src="https://img.shields.io/badge/planlandı-lightgrey?style=flat-square" alt="planlandı"></td>
</tr>
<tr>
  <td><code>TRCrypto.Bitexen</code></td><td>—</td>
  <td><img src="https://img.shields.io/badge/planlandı-lightgrey?style=flat-square" alt="planlandı"></td>
</tr>
<tr>
  <td><code>TRCrypto.Clients</code></td><td>Toplu paket</td>
  <td><img src="https://img.shields.io/badge/planlandı-lightgrey?style=flat-square" alt="planlandı"></td>
</tr>
</table>

**BtcTurk'te çalışan:** piyasa verisi · mum verisi · bakiye · emir işlemleri · işlem
geçmişi · gerçek zamanlı akışlar — hepsi hem native hem SharedApis üzerinden.

**Binance TR:** pariteler, emir defteri, işlemler ve gerçek zamanlı akışlar (ticker dahil)
— hem native hem SharedApis üzerinden.

**Henüz yok:** BtcTurk özel socket akışları, Binance TR kimlik doğrulama, Paribu, Bitexen.
Ayrıntı: [docs/DURUM.md](docs/DURUM.md)

---

## Hızlı başlangıç

Piyasa verisi için kimlik bilgisi gerekmez.

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

### Gerçek zamanlı

```csharp
var socket = new BtcTurkSocketClient();

var sub = await socket.SpotApi.SubscribeToTickerUpdatesAsync("BTCTRY",
    update => Console.WriteLine(update.Data.LastPrice));

// ...
await sub.Data.CloseAsync();
```

### Borsadan bağımsız

Bu kodun hiçbir yerinde native sembol biçimi geçmez:

```csharp
using CryptoExchange.Net.SharedApis;

ISpotTickerRestClient tickers = client.SpotApi.SharedClient;
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "TRY");

var ticker = await tickers.GetSpotTickerAsync(new GetTickerRequest(symbol));
```

> [!TIP]
> İstemciler yeniden kullanılabilir ve iş parçacığı güvenlidir — her istek için yenisini
> oluşturmayın.

---

## Hata yönetimi

API hataları istisna olarak fırlatılmaz, sonuç nesnesi olarak döner.

> [!WARNING]
> `Data`'ya erişmeden önce **her zaman** `Success` kontrol edin. BtcTurk iş mantığı
> hatalarını HTTP 200 içinde `"success": false` olarak döndürür; kütüphane bunu başarısız
> bir sonuca çevirir.

Geçersiz girdiler ağa çıkılmadan reddedilir — eksik fiyat, negatif miktar, sınır aşımı.

---

## Belgeler

| Konu | Dosya |
|---|---|
| **Nerede kaldık, sonraki adım** | [docs/DURUM.md](docs/DURUM.md) |
| API anahtarı alma ve bağlama | [docs/credentials/](docs/credentials/) |
| BtcTurk endpoint envanteri | [docs/vendor/](docs/vendor/) |
| Teknik spesifikasyon + doğrulama ekleri | [docs/spec/](docs/spec/) |
| Katkı rehberi | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Güvenlik açığı bildirimi | [SECURITY.md](SECURITY.md) |
| Değişiklik günlüğü | [CHANGELOG.md](CHANGELOG.md) |

---

## Geliştirme

```bash
dotnet build -c Release
dotnet test  -c Release
dotnet run --project examples/TRCrypto.Examples.Console   # canli public API dogrulamasi
```

### Secret koruması

Depoda gerçek kimlik bilgisi bulunmaz. Katkı vermeden önce pre-commit kancasını
etkinleştirin:

```bash
git config core.hooksPath .githooks
```

Kanca hazırlanan değişikliklerde secret araması yapar.
Ayrıntı: [docs/credentials/README.md](docs/credentials/README.md)

---

## Hedef platformlar

`net8.0` · `net9.0` · `net10.0` · `netstandard2.0` · `netstandard2.1`

## Lisans

MIT — bkz. [LICENSE](LICENSE).
