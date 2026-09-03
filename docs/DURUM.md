# Proje Durumu

> **Son güncelleme:** 27 Ağustos 2026
> Bu dosya, projeye ara verip döndüğünüzde ya da yeni biri katıldığında okunacak
> tek sayfalık özettir. Ayrıntı için ilgili belgelere bakın.

---

## Tek cümleyle

BtcTurk **tamamlandı** (REST + WebSocket + shared); yalnızca kullanıcıya özel socket
akışları eksik ve bunlar hesapta emir hareketi gerektiriyor.
**Binance TR: REST + WebSocket + shared yüzey** çalışıyor; kimlik doğrulama bekliyor.

---

## Tamamlananlar

### M0. Proje temeli ✅

| Ne | Nerede |
|---|---|
| Solution, 5 hedef platform, merkezi paket yönetimi | `Directory.Build.props`, `Directory.Packages.props` |
| Deterministic Release build, SourceLink, snupkg | `src/Directory.Build.props` |
| Analyzer'lar warnings-as-errors | `.editorconfig` |
| CI (derle/test/paketle/secret tara) | `.github/workflows/ci.yml` |
| Secret koruması | `.gitignore`, `.gitleaks.toml`, `.githooks/pre-commit`, `.env.example` |

### M1. BtcTurk public piyasa verisi ✅

**Native uçlar** (`client.SpotApi.ExchangeData`):

| Uç | Metod |
|---|---|
| `/api/v2/server/exchangeinfo` | `GetExchangeInfoAsync` · `GetServerTimeAsync` |
| `/api/v2/ticker` | `GetTickersAsync` · `GetTickerAsync` |
| `/api/v2/ticker/currency` | `GetTickersByQuoteAssetAsync` |
| `/api/v2/orderbook` | `GetOrderBookAsync` |
| `/api/v2/trades` | `GetTradesAsync` |

### M2. Kimlik doğrulama ve private REST ✅

İmzalama zinciri: `Base64(HMAC-SHA256(Base64Decode(secret), apiKey + stamp))`,
başlıklar `X-PCK` / `X-Stamp` / `X-Signature`. Resmi test vektörü olmadığı için
deterministik vektörler üretilip testlere sabitlendi.

**Hesap** (`client.SpotApi.Account`):

| Uç | Metod |
|---|---|
| `/api/v1/users/balances` | `GetBalancesAsync` |
| `/api/v1/users/transactions/trade` | `GetUserTradesAsync` |

**Emirler** (`client.SpotApi.Trading`):

| Uç | Metod |
|---|---|
| `GET /api/v1/openOrders` | `GetOpenOrdersAsync` |
| `GET /api/v1/allOrders` | `GetOrdersAsync` |
| `GET /api/v1/order/{id}` | `GetOrderAsync` |
| `POST /api/v1/order` | `PlaceOrderAsync` |
| `DELETE /api/v1/order` | `CancelOrderAsync` |

### Kline verisi ayrı bir sunucuda ✅

`GET graph-api.btcturk.com/v1/klines/history` → `ExchangeData.GetKlinesAsync`

Bu uç **ayrı bir host** kullanır, **standart zarfı taşımaz** ve zaman damgalarını
**saniye** cinsinden döndürür. Ayrıntı: [vendor/btcturk-kline-and-trades.md](vendor/btcturk-kline-and-trades.md)

### Shared yüzey ✅

`client.SpotApi.SharedClient` şu arayüzleri uygular:

`ISpotSymbolRestClient` · `ISpotTickerRestClient` · `IOrderBookRestClient` ·
`IRecentTradeRestClient` · `IKlineRestClient` · `IBalanceRestClient` ·
`ISpotOrderRestClient`

Yani BtcTurk artık borsadan bağımsız kodun tüm ortak REST işlemlerini destekliyor.


### M3. WebSocket ✅

Protokol resmi dokümanda gövdesiz listelendiği için **canlı bağlantıdan** çözüldü.
Ayrıntı: [vendor/btcturk-websocket.md](vendor/btcturk-websocket.md)

| Kanal | Metod | Shared |
|---|---|---|
| ticker | `SubscribeToTickerUpdatesAsync` | `ITickerSocketClient` |
| trade | `SubscribeToTradeUpdatesAsync` | `ITradeSocketClient` |
| orderbook | `SubscribeToOrderBookUpdatesAsync` | `IOrderBookSocketClient` |

Mesajlar `[tip, gövde]` dizisi biçimindedir; yönlendirme dizinin ilk elemanına göre
yapılır. Yeniden bağlanma ve abonelik geri kurma kütüphane tarafından sağlanır.


### M4. Binance TR ✅

Envanter canlı denemeyle çıkarıldı; dokümantasyon hangi uçların gerçekten public
olduğunu söylemiyor. Ayrıntı: [vendor/binance-tr-capabilities.md](vendor/binance-tr-capabilities.md)

| Uç | Metod |
|---|---|
| `/open/v1/common/time` | `GetServerTimeAsync` |
| `/open/v1/common/symbols` | `GetSymbolsAsync` |
| `/open/v1/market/depth` | `GetOrderBookAsync` |
| `/open/v1/market/agg-trades` | `GetAggregatedTradesAsync` |

**WebSocket** (`socket.SpotApi`): ticker · trade · aggTrade · depth · depth5/10/20 · kline

Ayrıntı: [vendor/binance-tr-websocket.md](vendor/binance-tr-websocket.md)

**Bağımlılık enjeksiyonu:** `services.AddTRCryptoBinanceTR(...)` çağrısı REST ve
WebSocket istemcilerini birlikte kaydeder.

**Private uçlar** (`client.SpotApi.Account` ve `client.SpotApi.Trading`): hesap bilgisi,
emir oluşturma, emir listesi, emir ayrıntısı, emir iptali ve işlem geçmişi. İmzalama
etkin; canlı hesap doğrulaması anahtar geldiğinde yapılacak.

**Shared yüzey:** REST tarafında `ISpotSymbolRestClient` · `IOrderBookRestClient` ·
`IRecentTradeRestClient` · `IBalanceRestClient` · `ISpotOrderRestClient`; socket
tarafında `ITickerSocketClient` · `ITradeSocketClient` · `IOrderBookSocketClient` ·
`IKlineSocketClient`.

Aynı kod artık iki borsayla çalışıyor ve bu canlı olarak doğrulandı. Tek bir `SharedSymbol` ile
her iki borsadan paralel fiyat okunuyor, her biri kendi sembol biçimini (`BTCTRY` /
`BTC_TRY`) kullanırken çağıran kod hiçbirini görmüyor.

BtcTurk'tan üç önemli fark:

1. **Zarf farklı.** `success` alanı yok, başarı `code == 0`; mesaj `msg`; zarfta `timestamp`
2. **Sembol alt çizgili.** `BTC_TRY` kullanılır, BtcTurk ise `BTCTRY` yazar
3. **Ticker REST'te anahtarsız alınamıyor.** `/api/v3/*` burada public değil, ama socket'te çalışıyor

### Belgeler ✅

| Dosya | İçerik |
|---|---|
| `docs/credentials/README.md` | Genel güvenlik: saklama, least-privilege, sızıntı durumu |
| `docs/credentials/btcturk.md` | BtcTurk'te adım adım API anahtarı alma ve bağlama |
| `docs/credentials/binance-tr.md` | Binance TR'de anahtar alma; imzalama şemasının BtcTurk'ten farkları |
| `docs/vendor/` | Resmi kaynaklı endpoint envanteri, istek limitleri, kline ve işlem geçmişi |
| `docs/spec/` | Orijinal spesifikasyon + doğrulama ekleri (D-1…D-44) |

---

## Yapılmayanlar

| Konu | Neden |
|---|---|
| **Kullanıcıya özel socket akışları** | Giriş çalışıyor, ama mesaj gövdeleri (423/441/451/452/453) hesapta **hareket olmadan gelmiyor** ve hiçbir yerde belgelenmemiş. Modelleri yazmak için gerçek emir hareketi gerekiyor |
| **`tax` alanının shared karşılığı** | BtcTurk işlem başına vergi bildiriyor; `SharedUserTrade` bunu temsil edemiyor. Native modelde korunur, shared yüzeyde yalnızca komisyon aktarılır |
| **Emir uçlarının canlı doğrulaması** | Gerçek emir vermeyi gerektirir; bilinçli olarak ertelendi. İmzalama ve okuma uçları canlı doğrulandı |
| **Binance TR: private uçların canlı doğrulaması** | Şema resmi dokümantasyondan alındı, imzalama yayımlanmış test vektörüyle doğrulandı. Gerçek bir hesaba karşı denenmesi anahtar geldiğinde yapılacak; `AuthenticationProbeTests` bunu ilk çalıştırmada bildirir |
| **Binance TR: REST ticker** | Borsa anahtarsız REST ticker sunmuyor; **socket üzerinden çalışıyor** |
| **Paribu · Bitexen** | M5–M6 |
| **`gitleaks` yerel taraması** | Araç makinede kurulu değil. Yapılandırma ve hook hazır; CI'da çalışacak |

---

## Doğrulama durumu

Son çalıştırma (27 Ağu 2026):

```
dotnet build -c Release   →  0 error, 5 TFM
dotnet test  -c Release   →  212/212 birim · 13 canli API · 2 atlandi (anahtar yok)
                             birim testler her PR'da, canli testler haftalik iste
dotnet pack  -c Release   →  .nupkg + .snupkg
canlı API                 →  379 parite, native == shared
```

Örnek uygulama canlı public API'ye karşı uçtan uca doğrulama yapar
(kimlik bilgisi gerekmez):

```bash
dotnet run --project examples/TRCrypto.Examples.Console
```

Son bölüm (`[11] Iki borsa, tek kod`) projenin varlık nedenini çalıştırarak gösterir:
tek bir `SharedSymbol` ile her iki borsadan REST ve WebSocket üzerinden fiyat okunur,
çağıran kod hiçbir borsanın sembol biçimini veya zarfını görmez.

---

## Bilinmesi gereken kararlar

1. **Endpoint'ler uydurulmaz.** Yalnızca resmi dokümantasyondan doğrulanan uçlar yazılır
   (ADR-010). Doğrulanamayan uç, yazılmak yerine `docs/vendor/` altında "dondurulmamış"
   olarak işaretlenir.
2. **Fixture'lar canlı API'den alınır.** Resmi örnek yanıtlar eksik ya da yanıltıcı olabiliyor;
   aşağıdaki bulgular bunun kanıtı.
3. **Enum'larda `Unknown` üyesi tanımlanmaz.** CryptoExchange.Net konvansiyonu: bilinmeyen
   değer tanımsız bir enum değerine düşer, `Enum.IsDefined` ile tespit edilir.
4. **Sembol adı ayrıştırılmaz.** BtcTurk base/quote'u ayrı alanlarda veriyor.
5. **Varlık türü tahmin edilmez.** Borsa `currencyType` alanıyla bildiriyor.
6. **Çekim izni hiçbir yerde önerilmez.** Kütüphane bu izne ihtiyaç duymaz.

---

## Dokümantasyonda olmayan, canlı API'de bulunan davranışlar

Ayrıntısı `docs/spec/` ekinde D-7…D-31. En önemlisi:

> **`code` alanının tipi uçlar arasında tutarsız:** çoğu uç `0` (sayı) döndürürken
> emir defteri `"SUCCESS"` (metin) döndürüyor. `int` olarak modellemek emir defteri
> çağrılarını tamamen kırıyordu.

Diğerleri: başarılı yanıtta `message` boş string (null değil); `numeratorSymbol` vs
`numerator` isim tutarsızlığı; ticker tek parite için de dizi döndürüyor; işlem
yanıtında dokümante edilmemiş `side` alanı.

---

## Sonraki adım

**1. Sonraki borsa (M4)**

Binance TR. Convention'lar BtcTurk üzerinde eksiksiz oturdu; aynı sıra izlenir:
vendor freeze → public REST → auth → private REST → WebSocket → shared.

**2. Kullanıcıya özel socket akışları**

Emir iptalinin kesinleştiği kanal 452 burada. Canlı bir hesap gerektirir.

**3. Ön sürüm**

BtcTurk tamamlandığına göre NuGet v0.1.0-preview yayınlanabilir. Workflow hazır;
`NUGET_API_KEY` ve environment onayı eksik.

---

## ✅ Canlı hesap doğrulaması yapıldı (26 Ağu 2026)

Gerçek bir hesaba karşı doğrulananlar. Yalnızca okuma yapıldı, emir oluşturulmadı:

| Doğrulama | Sonuç |
|---|---|
| REST imzalama | ✅ Kabul edildi |
| Bakiye ucu | ✅ Ayrıştırıldı; toplam = serbest + kilitli |
| Ondalık ayırıcı | ✅ **Nokta.** Dokümantasyondaki virgüllü örnek yanıltıcıymış |
| Socket giriş imzası | ✅ `publicKey + nonce`, REST'ten farklı. Dört aday denendi |

**Hâlâ doğrulanmayan:** emir oluşturma/iptal uçları ve özel akış mesajları.

### Özel akışlar neden bekliyor

Giriş çalışıyor, ama `423` (UserTrade), `441` (OrderMatch), `451`/`452`/`453` (Order*)
mesajlarının **gövdeleri hiçbir yerde belgelenmemiş** ve bu mesajlar yalnızca hesapta
emir hareketi olduğunda üretiliyor. Giriş sonrası 45 saniye dinlendi; hiçbiri gelmedi.

Modelleri yazmak için gerçek bir emir vermek gerekirdi. Bu bilinçli olarak ertelendi:
alan adlarını tahmin etmek, projenin endpoint uydurmama kuralının ihlali olurdu.

Hazır olduğunda iki yol var:

| Yol | Öğrenilenler | Risk |
|---|---|---|
| Piyasadan uzak limit emri + iptal | ,  | Emir asla eşleşmez, para el değiştirmez. *Al-Sat* izni gerekir |
| Gerçek eşleşen emir | + ,  | Gerçek para hareket eder, komisyon ödenir. Ayrı ve düşük bakiyeli hesap şart |

---

## ⚠️ Açık konu: sistem saati

Geliştirme makinesinin saati düzeltildi; kalan sapma **~1,8 saniye**. Değer BtcTurk ve
Binance TR sunucularına karşı ayrı ayrı ölçüldü ve ikisi de aynı sonucu verdiği için
ölçüm hatası değil, gerçek bir kayma.

| | Tolerans | Durum |
|---|---|---|
| BtcTurk | geniş | ✅ sorun yok, imzalı uçlar canlı doğrulandı |
| Binance TR | `recvWindow` varsayılan **5000 ms** | ⚠️ pencere içinde, ama payı ~3,2 saniye |

Binance TR'nin penceresi dar olduğu için bu kayma büyürse imzalı istekler reddedilir.
`ServerTimeIntegrationTests` her çalıştırmada sapmayı ölçer ve pencere aşılırsa
başarısız olur. Böylece sorun, anahtarı bağladıktan sonra değil öncesinde görünür.

Makine etki alanına bağlı olduğundan saat etki alanı denetleyicisinden gelir; kalıcı
çözüm sistem yöneticisi tarafındadır.

```bash
dotnet test --filter "FullyQualifiedName~ServerTimeIntegrationTests" -l "console;verbosity=detailed"
```
