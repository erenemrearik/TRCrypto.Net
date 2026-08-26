# Binance TR — WebSocket Protokolü

> **Erişim tarihi:** 27 Ağustos 2026
> Kaynak: `binance.tr/apidocs` **ve canlı bağlantı gözlemi**
>
> REST envanteri: [binance-tr-capabilities.md](binance-tr-capabilities.md)

**Bağlantı:** `wss://stream-cloud.binance.tr/ws/<akış>`

Dokümantasyon parite türüne göre iki host tanımlar: tür 1 için `stream-cloud.binance.tr`,
tür 3 için `stream-tr.2meta.app`. Parite türü `common/symbols` yanıtındaki `type` alanından
okunur.

---

## ⚠️ Kritik 5 — sembol formatı REST'ten farklı

Aynı borsa içinde **üç** ayrı gösterim var:

| Yer | Biçim | Örnek |
|---|---|---|
| REST istek ve yanıt | Alt çizgili, büyük harf | `BTC_TRY` |
| **WebSocket abonelik** | **Alt çizgisiz, küçük harf** | **`btctry`** |
| WebSocket yanıt (`s` alanı) | Alt çizgisiz, büyük harf | `BTCTRY` |

Abonelikte `btc_try` ya da `BTC_TRY` kullanmak sessizce çalışır: bağlantı kurulur, hiçbir
hata dönmez, ama **hiçbir mesaj gelmez**. Yanlış biçim bir hataya yol açmadığı için
sorun ancak "veri akmıyor" olarak fark edilir.

---

## ⚠️ Kritik 6 — REST'te anahtar isteyen veri burada ücretsiz

| Veri | REST | WebSocket |
|---|---|---|
| Ticker | ❌ Anahtar gerekiyor (`/api/v3/ticker/24hr`) | ✅ **Anahtarsız** |
| Tekil işlemler | ⚠️ Boş liste dönüyor | ✅ **Çalışıyor** |
| Mum verisi | ⚠️ Boş liste dönüyor | ✅ **Çalışıyor** |

REST tarafında elde edilemeyen üç veri türü WebSocket üzerinden anahtarsız alınabiliyor.
Ticker, tekil işlem ve mum verisi için REST yerine socket kullanılmalıdır.

---

## ⚠️ Kritik 7 — alan adlari buyuk/kucuk harfe duyarli

Akis mesajlari tek harfli alanlar kullanir ve **harf buyuklugu anlam tasir**:

| Alan | Anlamı | | Alan | Anlamı |
|---|---|---|---|---|
| `p` | Fiyat değişimi | | `P` | Yüzde değişim |
| `b` | En iyi alış fiyatı | | `B` | En iyi alış miktarı |
| `a` | En iyi satış fiyatı | | `A` | En iyi satış miktarı |
| `U` | İlk sıra numarası | | `u` | Son sıra numarası |
| `q` | Quote hacmi | | `Q` | Son işlem miktarı |

Serileştirici büyük/küçük harfe **duyarsız** yapılandırılırsa bu çiftler çakışır ve
ayrıştırma tamamen başarısız olur. BtcTurk tam tersini gerektiriyordu: orada aynı alan
uçlar arasında farklı harf büyüklüğüyle geldiği için duyarsız eşleşme şarttı.

İki borsa için tek bir serileştirme ayarı kullanılamaz.

## Akışlar (canlı doğrulama)

Mesaj formatı global Binance ile aynıdır: tek bir JSON nesnesi, olay türü `e` alanında.

### `@ticker` — 24 saatlik özet

```json
{ "e": "24hrTicker", "E": 1787778712698, "s": "BTCTRY",
  "p": "23466.00000000", "P": "0.624", "w": "3773562.15399922",
  "x": "3762466.00000000", "c": "3785106.00000000", "Q": "0.00211000",
  "b": "3785105.00000000", "B": "0.00699000",
  "a": "3785106.00000000", "A": "0.00510000",
  "o": "...", "h": "...", "l": "...", "v": "...", "q": "...",
  "O": 0, "C": 0, "F": 0, "L": 0, "n": 0 }
```

| Alan | Anlamı |
|---|---|
| `p` / `P` | 24s değişim (mutlak / yüzde) · `w` ağırlıklı ortalama |
| `x` | Önceki kapanış · `c` son fiyat · `Q` son işlem miktarı |
| `b` / `B` | En iyi alış fiyatı / miktarı |
| `a` / `A` | En iyi satış fiyatı / miktarı |
| `o` `h` `l` | Açılış, en yüksek, en düşük |
| `v` / `q` | Hacim (base / quote) |
| `O` / `C` | İstatistik penceresi başı / sonu |
| `F` / `L` / `n` | İlk / son işlem kimliği, işlem sayısı |

### `@depth` — emir defteri farkı

```json
{ "e": "depthUpdate", "E": 1787778715698, "s": "BTCTRY",
  "U": 5893366894, "u": 5893366934,
  "b": [["3785134.00000000","0.12320000"]],
  "a": [["...","..."]] }
```

`U` ve `u` bu güncellemenin kapsadığı ilk ve son sıra numarasıdır. Miktar `0` olan bir
kademe, o fiyat seviyesinin defterden **silindiği** anlamına gelir.

### `@depth20` — tam görüntü

```json
{ "lastUpdateId": 5893367072,
  "bids": [["3785178.00000000","0.00510000"]],
  "asks": [["...","..."]] }
```

Olay türü alanı **yoktur**; bu akış diğerlerinden farklı olarak yalın bir görüntü döndürür.

### `@aggTrade` — toplu işlem

```json
{ "e": "aggTrade", "E": 1787778722442, "s": "BTCTRY",
  "a": 73875517, "p": "3785105.00000000", "q": "0.00017000",
  "f": 80020069, "l": 80020069, "T": 1787778722442,
  "m": true, "M": true }
```

### `@trade` — tekil işlem

```json
{ "e": "trade", "E": 1787778726638, "s": "BTCTRY",
  "t": 80020071, "p": "3784348.00000000", "q": "0.00101000",
  "T": 1787778726638, "m": false, "M": true }
```

> REST'teki `market/trades` ucu boş liste döndürürken bu akış çalışıyor.

### `@kline_<aralık>` — mum

```json
{ "e": "kline", "E": 1787778770746, "s": "BTCTRY",
  "k": { "t": 1787778720000, "T": 1787778779999, "s": "BTCTRY", "i": "1m",
         "f": 80020069, "L": 80020086,
         "o": "3785105.00000000", "c": "3784054.00000000",
         "h": "3785105.00000000", "l": "3782521.00000000",
         "v": "0.01253000", "n": 18, "x": false,
         "q": "47404.61345000", "V": "0.00766000" } }
```

| Alan | Anlamı |
|---|---|
| `t` / `T` | Mum başlangıç / bitiş zamanı |
| `i` | Aralık (`1m`, `5m` …) |
| `o` `h` `l` `c` | Açılış, yüksek, düşük, kapanış |
| `v` / `q` | Hacim (base / quote) |
| `x` | **Mum kapandı mı** — false ise değerler değişmeye devam eder |
| `n` | İşlem sayısı · `f`/`L` ilk/son işlem kimliği |

> ⚠️ Mesajlar mum aralığından daha seyrek gelebilir; ilk mesaj için bir dakikaya kadar
> beklemek gerekebilir. Kısa süreli bir denemede "akış çalışmıyor" sonucuna varmak yanlış olur.

### `@bookTicker` — en iyi alış/satış

```json
{ "u": 5893367821, "s": "BTCTRY",
  "b": "3782831.00000000", "B": "0.00009000",
  "a": "3782832.00000000", "A": "0.01319000" }
```

Olay türü alanı yoktur.

---

## İşlem yönü

Hiçbir akış yönü doğrudan vermez. `m` alanı alıcı tarafın piyasa yapıcı olup olmadığını
söyler: alıcı piyasa yapıcıysa işlemi başlatan satıcıdır, yani yön **satış**tır.
Bu, global Binance ile aynı kuraldır.

---

## Henüz Doğrulanmamış

- Tür 3 pariteler ve `stream-tr.2meta.app` hostu
- Birleşik akış (`/stream?streams=a/b`) desteği
- Ping/pong ve bağlantı ömrü kuralları
- Kullanıcı akışı (`user-listen-token` ile); anahtar gerektirir
