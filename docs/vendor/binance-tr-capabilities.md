# Binance TR: Vendor Capability Freeze

> **Erişim tarihi:** 26 Ağustos 2026
> **Kaynak:** `binance.tr/apidocs` **ve canlı uç denemeleri**
>
> Dokümantasyon endpoint listesi verir ama hangi uçların gerçekten public olduğunu ve
> hangilerinin veri döndürdüğünü söylemez. Aşağıdaki "canlı" işaretleri doğrudan
> denemeyle belirlenmiştir.

## Genel

| Alan | Değer |
|---|---|
| REST taban adresi | `https://www.binance.tr` |
| Alternatif | `https://api.binance.me` |
| WebSocket (tip 1) | `wss://stream-cloud.binance.tr` |
| WebSocket (tip 3) | `wss://stream-tr.2meta.app` |
| WebSocket API | `wss://ws-api.binance.tr:443/ws-api/v3` |
| Sembol formatı | **`BTC_TRY`**, alt çizgili |
| Kimlik doğrulama | `X-MBX-APIKEY` başlığı + HMAC-SHA256 imza |
| İmzalanan | Query string + istek gövdesi |
| Zorunlu parametre | `timestamp` (ms); opsiyonel `recvWindow` (varsayılan 5000, en fazla 60000) |

### Yanıt zarfı

```json
{ "code": 0, "msg": "Success", "data": { }, "timestamp": 1787775149371 }
```

> ### ⚠️ Kritik 1: zarf BtcTurk'ten tamamen farklı
>
> | | BtcTurk | Binance TR |
> |---|---|---|
> | Başarı alanı | `success` (bool) | **yok**; `code == 0` ile anlaşılır |
> | Mesaj | `message` | `msg` |
> | Zaman | yok | `timestamp` (zarf seviyesinde) |
>
> İki borsa için tek bir ortak zarf modeli kullanılamaz. Her adaptörün kendi zarfı ve
> kendi hata çevirisi olmalıdır.
>
> Hatalar da HTTP 200 içinde döner (`code != 0`), tıpkı BtcTurk'teki gibi.

---

## Public uçlar: canlı doğrulama

| Uç | Durum | Not |
|---|---|---|
| `GET /open/v1/common/time` | ✅ Çalışıyor | Zaman zarftaki `timestamp` alanındadır; `data` **null** döner |
| `GET /open/v1/common/symbols` | ✅ Çalışıyor | 307 parite; `data.list` altında |
| `GET /open/v1/market/depth` | ✅ Çalışıyor | `lastUpdateId` + `bids`/`asks` |
| `GET /open/v1/market/agg-trades` | ✅ Çalışıyor | Toplulaştırılmış işlemler |
| `GET /open/v1/market/trades` | ⚠️ **Boş dönüyor** | `code: 0` ama `list: []`; denenen tüm paritelerde |
| `GET /open/v1/market/klines` | ⚠️ **Boş dönüyor** | `code: 0` ama `list: []`; `interval=1h` kabul ediliyor, `60m` reddediliyor (kod 2803) |
| `GET /open/v1/market/ticker` | ❌ 404 | Böyle bir uç yok |
| `GET /api/v3/ticker/24hr` | ❌ **Anahtar gerektiriyor** | `code: 3701 Invalid API-key, IP, or permissions` |
| `GET /api/v3/depth` · `/api/v3/trades` | ❌ **Anahtar gerektiriyor** | Aynı hata |

> ### ⚠️ Kritik 2: standart Binance uçları burada public DEĞİL
>
> `/api/v3/*` yolları, global Binance'te herkese açık olan piyasa verisi uçlarıdır.
> Binance TR'de bunlar **API anahtarı ister**. Global Binance için yazılmış kod bu
> nedenle çalışmaz; spesifikasyonun "Binance TR ayrı adapter olmalı" kararı (ADR-005)
> burada somutlaşıyor.

> ### ⚠️ Kritik 3: ticker verisi anahtarsız alınamıyor
>
> Ne `/open/v1/market/*` altında bir ticker ucu var, ne de `/api/v3/ticker/24hr`
> anahtarsız çalışıyor. **Binance TR için "public ticker" mümkün görünmüyor.**
>
> Bu, spesifikasyonun MVP kapsamını (Bölüm 9.5, ticker = P0) doğrudan etkiler:
> BtcTurk'te anahtarsız çalışan bir özellik, Binance TR'de anahtar gerektiriyor.
> `Discover()` bu farkı bildirmelidir.

> ### ⚠️ Kritik 4: `trades` ve `klines` boş liste döndürüyor
>
> Her ikisi de `code: 0` (başarılı) ama veri yok. Denenen pariteler: `BTC_TRY`,
> `USDT_TRY`, `BTC_USDT`. `startTime`/`endTime` eklemek de sonucu değiştirmiyor.
>
> Başarılı görünüp boş dönen bir uç, hata döndüren uçtan daha tehlikelidir: çağıran
> taraf "işlem yok" sanır. İşlem verisi için **`agg-trades` kullanılmalıdır**.
>
> Mum verisi için anahtarsız bir yol şu an bilinmiyor.

---

## Şemalar (canlı doğrulama)

### `common/symbols`

```json
{ "code": 0, "msg": "Success", "timestamp": 1787775149371,
  "data": { "list": [ {
    "type": 1,
    "symbol": "BTC_TRY",
    "baseAsset": "BTC",  "basePrecision": 8,
    "quoteAsset": "TRY", "quotePrecision": 8,
    "filters": [
      { "filterType": "PRICE_FILTER", "minPrice": "1.00000000",
        "maxPrice": "19998638.00000000", "tickSize": "1.00000000", "applyToMarket": false },
      { "filterType": "LOT_SIZE", "minQty": "0.00001000",
        "maxQty": "4611.00000000", "stepSize": "0.00001000", "applyToMarket": false },
      { "filterType": "ICEBERG_PARTS", "limit": "100", "applyToMarket": false },
      { "filterType": "MARKET_LOT_SIZE", "minQty": "0.00000000",
        "maxQty": "2.59666377", "stepSize": "0.00000000", "applyToMarket": false },
      { "filterType": "TRAILING_DELTA", "applyToMarket": false },
      { "filterType": "PERCENT_PRICE_BY_SIDE", "bidMultiplierUp": 1.3,
        "bidMultiplierDown": 0.5, "askMultiplierUp": 2, "applyToMarket": false }
    ]
  } ] } }
```

> `type` alanı WebSocket akış adresini belirler (tip 1 / tip 3 farklı hostlar).
>
> Base ve quote ayrı alanlarda gelir. BtcTurk'te olduğu gibi sembol adı ayrıştırılmaz.
>
> Filtreler global Binance ile aynı yapıdadır.

### `market/depth`

```json
{ "code": 0, "msg": "Success",
  "data": { "lastUpdateId": 5893258250,
            "bids": [["3773371.00000000","0.01472000"]],
            "asks": [[ "...", "..." ]] } }
```

`lastUpdateId`: delta senkronizasyonu için sıra numarası.

### `market/agg-trades`

```json
{ "code": 0, "msg": "Success",
  "data": { "list": [ {
    "a": 73874422, "p": "3772843.00000000", "q": "0.00510000",
    "f": 80018855, "l": 80018855, "T": 1787775170124,
    "m": false, "M": true
  } ] } }
```

Global Binance ile aynı kısaltmalar: `a` toplu işlem kimliği, `p` fiyat, `q` miktar,
`f`/`l` ilk/son işlem kimliği, `T` zaman (ms), `m` alıcı piyasa yapıcı mı, `M` yok sayılır.

---

## Private uçlar

Tamamı imza ister. Şemalar resmi dokümantasyondan alındı; canlı doğrulama API anahtarı
geldiğinde yapılacaktır.

| Method | Path | Durum |
|---|---|---|
| GET | `/open/v1/account/spot` | ✅ Uygulandı |
| POST | `/open/v1/orders` | ✅ Uygulandı |
| GET | `/open/v1/orders` | ✅ Uygulandı |
| GET | `/open/v1/orders/detail` | ✅ Uygulandı |
| POST | `/open/v1/orders/cancel` | ✅ Uygulandı |
| POST | `/open/v1/orders/batch-cancel` | ⏳ Envanteri çıkarıldı |
| POST | `/open/v1/user-listen-token` | ⏳ Envanteri çıkarıldı |

`user-listen-token`, kullanıcı akışı için token üretir. Token kendiliğinden yenilenmez ve
yaşam döngüsü yönetimi gerektirir.

### İmza

Sorgu dizesi ile istek gövdesi parametre sırasına göre birleştirilir, secret ile
HMAC SHA256 hesaplanır. Secret **Base64 çözülmez**, imza **onaltılık** kodlanır ve
anahtar `X-MBX-APIKEY` başlığıyla gönderilir. Her imzalı istekte `timestamp` zorunludur;
`recvWindow` isteğe bağlıdır, varsayılanı 5000 ms, en fazla 60000 ms olabilir.

### Sayısal enum değerleri

Bu uçlar durum ve tür bilgisini metin yerine **sayı** olarak taşır. Global Binance metin
kullandığı için oradan taşınan kod burada sessizce yanlış çözümlenir.

| Alan | Değerler |
|---|---|
| `side` | 0 alış · 1 satış |
| `type` | 1 limit · 2 piyasa · 3 stop loss · 4 stop loss limit · 5 take profit · 6 take profit limit · 7 limit maker |
| `status` | -2 sistem işliyor · 0 yeni · 1 kısmen doldu · 2 doldu · 3 iptal · 4 iptal bekliyor · 5 reddedildi · 6 süresi doldu |
| `timeInForce` | 1 GTC · 2 IOC · 3 FOK · 4 GTX |
| `symbolType` | 1 ana · 2 sonraki |
| `direct` | `prev` artan · `next` azalan |

### Zarf alan adı tutarsız

Emir oluşturma yanıtı hata metnini `message` alanında, diğer private uçlar `msg` alanında
döndürür. Tek bir alan adı beklemek, hata mesajının bazı uçlarda boş görünmesine yol açar;
kütüphane her ikisini de okur.

### Yanıt alanları

`GET /open/v1/account/spot` → `makerCommission`, `takerCommission`, `buyerCommission`,
`sellerCommission`, `fiatMakerCommission`, `fiatTakerCommission`, `canTrade`,
`canWithdraw`, `canDeposit`, `accountAssets[]` (`asset`, `free`, `locked`).

`POST /open/v1/orders` → `orderId`, `createTime`.

`GET /open/v1/orders` → `list[]` içinde `orderId`, `clientId`, `symbol`, `symbolType`,
`side`, `type`, `price`, `origQty`, `origQuoteQty`, `executedQty`, `executedPrice`,
`executedQuoteQty`, `timeInForce`, `stopPrice`, `icebergQty`, `status`, `isWorking`,
`createTime`.

`GET /open/v1/orders/detail` ve `POST /open/v1/orders/cancel` → `orderId`, `orderListId`,
`clientId`, `symbol`, `side`, `type`, `price`, `status`, `origQty`, `origQuoteQty`,
`executedQty`, `executedPrice`, `executedQuoteQty`, `createTime`.

`user-listen-token`, kullanıcı akışı için token üretir. Spesifikasyon (Bölüm 11.2) bu
tokenın kendiliğinden yenilenmediğini, lifecycle yönetimi gerektiğini belirtir.

---

## Henüz Doğrulanmamış

- İmzanın canlı bir hesapta kabul edilip edilmediği (anahtar gerekiyor)
- Mum verisi için anahtarsız bir yol olup olmadığı
- `trades` ucunun neden boş döndüğü
- WebSocket protokolü ve `type` alanının akış adresini nasıl belirlediği
- İstek limitleri (weight/order-count), 429 ve 418 davranışı

## Kaynaklar

- `binance.tr/apidocs`
- Canlı uç denemeleri (26 Ağu 2026): `www.binance.tr` üzerinde `/open/v1/*` ve `/api/v3/*`
