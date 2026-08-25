# Proje Durumu — Nerede Kaldık?

> **Son güncelleme:** 25 Ağustos 2026
> Bu dosya, projeye ara verip döndüğünüzde ya da yeni biri katıldığında okunacak
> tek sayfalık özettir. Ayrıntı için ilgili belgelere bakın.

---

## Tek cümleyle

BtcTurk'ün **public piyasa verisi, bakiye ve emir uçları** çalışıyor; kimlik doğrulama
uygulandı. **WebSocket ve emirlerin borsadan bağımsız yüzeyi henüz yok**; diğer üç
borsaya başlanmadı.

---

## Tamamlananlar

### M0 — Proje temeli ✅

| Ne | Nerede |
|---|---|
| Solution, 5 hedef platform, merkezi paket yönetimi | `Directory.Build.props`, `Directory.Packages.props` |
| Deterministic Release build, SourceLink, snupkg | `src/Directory.Build.props` |
| Analyzer'lar warnings-as-errors | `.editorconfig` |
| CI (derle/test/paketle/secret tara) | `.github/workflows/ci.yml` |
| Secret koruması | `.gitignore`, `.gitleaks.toml`, `.githooks/pre-commit`, `.env.example` |

### M1 — BtcTurk public piyasa verisi ✅

**Native uçlar** (`client.SpotApi.ExchangeData`):

| Uç | Metod |
|---|---|
| `/api/v2/server/exchangeinfo` | `GetExchangeInfoAsync` · `GetServerTimeAsync` |
| `/api/v2/ticker` | `GetTickersAsync` · `GetTickerAsync` |
| `/api/v2/ticker/currency` | `GetTickersByQuoteAssetAsync` |
| `/api/v2/orderbook` | `GetOrderBookAsync` |
| `/api/v2/trades` | `GetTradesAsync` |

### M2 — Kimlik doğrulama + private REST ✅

İmzalama zinciri: `Base64(HMAC-SHA256(Base64Decode(secret), apiKey + stamp))`,
başlıklar `X-PCK` / `X-Stamp` / `X-Signature`. Resmi test vektörü olmadığı için
deterministik vektörler üretilip testlere sabitlendi.

**Hesap** (`client.SpotApi.Account`):

| Uç | Metod |
|---|---|
| `/api/v1/users/balances` | `GetBalancesAsync` |

**Emirler** (`client.SpotApi.Trading`):

| Uç | Metod |
|---|---|
| `GET /api/v1/openOrders` | `GetOpenOrdersAsync` |
| `GET /api/v1/allOrders` | `GetOrdersAsync` |
| `GET /api/v1/order/{id}` | `GetOrderAsync` |
| `POST /api/v1/order` | `PlaceOrderAsync` |
| `DELETE /api/v1/order` | `CancelOrderAsync` |

**Shared yüzey** (`client.SpotApi.SharedClient`):
`ISpotSymbolRestClient` · `ISpotTickerRestClient` · `IOrderBookRestClient` ·
`IRecentTradeRestClient` · `IBalanceRestClient`

### Belgeler ✅

| Dosya | İçerik |
|---|---|
| `docs/credentials/README.md` | Genel güvenlik: saklama, least-privilege, sızıntı durumu |
| `docs/credentials/btcturk.md` | BtcTurk'te adım adım API anahtarı alma ve bağlama |
| `docs/vendor/btcturk-capabilities.md` | Resmi kaynaklı endpoint envanteri + istek limitleri |
| `docs/spec/` | Orijinal spesifikasyon + doğrulama ekleri (D-1…D-18) |

---

## Yapılmayanlar

| Konu | Neden |
|---|---|
| **`ISpotOrderRestClient`** (emirlerin shared yüzeyi) | Arayüz kullanıcı işlem geçmişi uçlarını da zorunlu kılıyor (`GetSpotOrderTradesAsync`, `GetSpotUserTradesAsync`). Kısmen uygulanamaz; önce `users/transactions/trade` ucu gerekiyor |
| **Kullanıcı işlem geçmişi** | Henüz envanterlenmedi — yukarıdakinin ön koşulu |
| **WebSocket** | M3 |
| **OHLC / kline** | Endpoint path'i resmi dokümandan doğrulanamadı; **uydurmaktansa yazılmadı** |
| **Canlı private doğrulama** | API anahtarı yok. İmzalama sabit test vektörleriyle, uçlar contract testleriyle doğrulandı; gerçek hesaba karşı hiç çalıştırılmadı |
| **Binance TR · Paribu · Bitexen** | M4–M6 |
| **`gitleaks` yerel taraması** | Araç makinede kurulu değil. Yapılandırma ve hook hazır; CI'da çalışacak |

---

## Doğrulama durumu

Son çalıştırma (25 Ağu 2026):

```
dotnet build -c Release   →  0 error, 5 TFM
dotnet test  -c Release   →  74/74 geçti
dotnet pack  -c Release   →  .nupkg + .snupkg
canlı API                 →  379 parite, native == shared
```

Örnek uygulama canlı public API'ye karşı uçtan uca doğrulama yapar
(kimlik bilgisi gerekmez):

```bash
dotnet run --project examples/TRCrypto.Examples.Console
```

---

## Bilinmesi gereken kararlar

1. **Endpoint'ler uydurulmaz.** Yalnızca resmi dokümantasyondan doğrulanan uçlar yazılır
   (ADR-010). Doğrulanamayan uç, yazılmak yerine `docs/vendor/` altında "dondurulmamış"
   olarak işaretlenir.
2. **Fixture'lar canlı API'den alınır.** Resmi örnek yanıtlar eksik/yanıltıcı olabiliyor —
   aşağıdaki bulgular bunun kanıtı.
3. **Enum'larda `Unknown` üyesi tanımlanmaz.** CryptoExchange.Net konvansiyonu: bilinmeyen
   değer tanımsız bir enum değerine düşer, `Enum.IsDefined` ile tespit edilir.
4. **Sembol adı ayrıştırılmaz.** BtcTurk base/quote'u ayrı alanlarda veriyor.
5. **Varlık türü tahmin edilmez.** Borsa `currencyType` alanıyla bildiriyor.
6. **Çekim izni hiçbir yerde önerilmez.** Kütüphane bu izne ihtiyaç duymaz.

---

## Dokümantasyonda olmayan, canlı API'de bulunan davranışlar

Ayrıntısı `docs/spec/` ekinde D-7…D-18. En önemlisi:

> **`code` alanının tipi uçlar arasında tutarsız:** çoğu uç `0` (sayı) döndürürken
> emir defteri `"SUCCESS"` (metin) döndürüyor. `int` olarak modellemek emir defteri
> çağrılarını tamamen kırıyordu.

Diğerleri: başarılı yanıtta `message` boş string (null değil); `numeratorSymbol` vs
`numerator` isim tutarsızlığı; ticker tek parite için de dizi döndürüyor; işlem
yanıtında dokümante edilmemiş `side` alanı.

---

## Sonraki adım

**1. Kullanıcı işlem geçmişi + emirlerin shared yüzeyi**

`ISpotOrderRestClient` uygulanabilmesi için önce `users/transactions/trade` ucunun
envanterlenmesi gerekiyor. Arayüz kısmen uygulanamaz — ya tamamı ya hiçbiri.

**2. WebSocket (M3)**

Vendor freeze: `websocket-feed/*` sayfaları. Kanal/olay/model yapısı, kimlik doğrulama
akışı, ve emir iptalinin kesinleştiği **kanal 452**.

**3. Sonraki borsa (M4)**

Binance TR. Convention'lar BtcTurk üzerinde oturdu; aynı sıra izlenir:
vendor freeze → public REST → auth → private REST → shared.

---

## ⚠️ Canlı private doğrulama yapılmadı

Bakiye ve emir uçları **gerçek bir hesaba karşı hiç çalıştırılmadı** — API anahtarı yok.
Doğrulanan: imzalama zinciri (sabit test vektörü), istek üretimi (contract testleri),
yanıt ayrıştırma (resmi örneklerden fixture).

Anahtar geldiğinde ilk yapılacak: `GetBalancesAsync` ile okuma testi. Gereken minimum
izinler: *Toplam Varlık* + *Hesap*, *Al-Sat* kapalı, **Çekim kapalı**, IP allow-list dolu.
Ayrıntı: `docs/credentials/btcturk.md`.

---

## ⚠️ Açık konu: sistem saati

Geliştirme makinesi ile BtcTurk sunucusu arasında tutarlı olarak **~19 saniye** fark ölçüldü.
`X-Stamp` UTC milisaniye gerektirdiğinden bu, M2'de imzalı isteklerin reddedilmesine yol
açar. İmzalı uçları gerçek bir hesaba karşı denemeden önce NTP ile senkronize edin.

Kontrol:

```csharp
var serverTime = await client.SpotApi.ExchangeData.GetServerTimeAsync();
Console.WriteLine((DateTime.UtcNow - serverTime.Data).TotalSeconds);
```
