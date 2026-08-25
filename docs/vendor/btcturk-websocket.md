# BtcTurk — WebSocket Protokolü

> **Erişim tarihi:** 26 Ağustos 2026
> Kaynak: `docs.btcturk.com/docs/websocket-feed/*` **ve canlı bağlantı gözlemi**
>
> Resmi dokümantasyon mesaj kodlarını listeler ama **mesaj gövdelerini vermez**.
> Aşağıdaki alan adlarının tamamı canlı bağlantıdan doğrulanmıştır.

**Bağlantı:** `wss://ws-feed-pro.btcturk.com`

---

## Mesaj zarfı

Her mesaj **iki elemanlı bir dizidir**: `[tip, gövde]`. Gövde de kendi içinde `type`
alanını tekrar taşır.

```json
[402, { "type": 402, "channel": "ticker", "event": "BTCTRY", "...": "..." }]
```

> ⚠️ Bu, projedeki REST kalıbından tamamen ayrıdır: ne `success`/`code` zarfı vardır
> ne de tek bir JSON nesnesi. Yönlendirme dizinin ilk elemanına göre yapılır.

---

## Bağlantı akışı

Sunucu, bağlanır bağlanmaz **istenmeden** bir sürüm mesajı gönderir:

```json
[991, { "type": 991, "current": "6.0.0", "min": "2.3.0" }]
```

> ⚠️ Kod `991` resmi model listesinde **yer almaz**. Bilinmeyen mesaj olarak
> değerlendirilip bağlantı düşürülmemelidir.

### Abonelik

```json
[151, { "type": 151, "channel": "ticker", "event": "BTCTRY", "join": true }]
```

Abonelikten çıkmak için aynı mesaj `"join": false` ile gönderilir.

Sunucu onayı:

```json
[100, { "type": 100, "ok": true, "message": "join|ticker:BTCTRY" }]
```

> `message` alanı `join|<kanal>:<olay>` biçimindedir; hangi aboneliğin onaylandığını
> ayırt etmek için kullanılabilir.

**Kanallar:** `ticker` · `trade` · `orderbook`
**Olay (event):** parite adı, büyük harf (`BTCTRY`)

---

## Mesaj kodları

| Kod | Model | Erişim | Durum |
|---|---|---|---|
| 100 | Result (onay/sonuç) | Public | ✅ Gözlemlendi |
| 101 | Request | Public | — |
| 114 | UserLoginResult | Private | — |
| 151 | Subscription | Public | ✅ Gözlemlendi |
| 401 | TickerAll | Public | — |
| 402 | TickerPair | Public | ✅ Gözlemlendi |
| 421 | TradeList | Public | ✅ Gözlemlendi — **dokümante edilmemiş** |
| 422 | TradeSingle | Public | — |
| 423 | UserTrade | Private | — |
| 431 | OrderBookFull | Public | ✅ Gözlemlendi |
| 432 | OrderBookDifference | Public | — |
| 441 | UserOrderMatch | Private | — |
| 451 | OrderInsert | Private | — |
| 452 | OrderDelete | Private | — |
| 453 | OrderUpdate | Private | — |
| 991 | Sürüm bilgisi | Public | ✅ Gözlemlendi — **dokümante edilmemiş** |

> ⚠️ Resmi model listesi `422 TradeSingle` içerir ama abone olunduğunda gelen kod
> **421**'dir ve gövdesi bir **liste** taşır. İkisi de ele alınmalıdır.

---

## Gövde şemaları (canlı doğrulama)

### 402 — Ticker

Alan adları tek/iki harfe kısaltılmıştır:

```json
[402, {
  "B": "3779950", "A": "3781912",
  "BA": "0.00092482", "AA": "0.02976469",
  "PS": "BTCTRY", "H": "3891284", "L": "3745640", "LA": "3780291",
  "O": "3794559", "V": "31.53834999", "AV": "3824093.6008034",
  "D": "-12647", "DP": "-0.38",
  "DS": "TRY", "NS": "BTC", "PId": 1,
  "channel": "ticker", "event": "BTCTRY", "type": 402
}]
```

| Alan | Anlamı |
|---|---|
| `B` / `A` | En iyi alış / satış fiyatı |
| `BA` / `AA` | En iyi alış / satış miktarı |
| `PS` | Parite sembolü |
| `H` / `L` / `LA` / `O` | Yüksek / düşük / son / açılış |
| `V` | Hacim · `AV` | Ortalama fiyat |
| `D` / `DP` | Günlük değişim (mutlak / yüzde) |
| `NS` / `DS` | Base / quote varlık |
| `PId` | Parite kimliği |

> ⚠️ Değerler **metin** olarak gelir; REST ticker ucu aynı verileri **sayı** olarak
> döndürür.

### 421 — Trade listesi

```json
[421, {
  "symbol": "BTCTRY",
  "items": [
    { "D": "1787692476996", "I": "100163842129199947",
      "A": "0.0127318800", "P": "3765490.0000000000", "S": 0 }
  ],
  "channel": "trade", "event": "BTCTRY", "type": 421
}]
```

| Alan | Anlamı |
|---|---|
| `D` | Zaman damgası — **metin içinde milisaniye** |
| `I` | İşlem kimliği |
| `A` / `P` | Miktar / fiyat |
| `S` | Yön — **sayısal** (0 / 1) |

> ⚠️ `S` sayısaldır; REST işlem ucu yönü `"buy"` / `"sell"` metniyle verir.
>
> **Anlamı dokümante edilmemiştir.** Canlı akıştaki işlem kimlikleri REST yanıtıyla
> eşleştirilerek belirlendi (26 Ağu 2026, 15 eşleşen işlem, çelişki yok):
>
> | `S` | Yön |
> |---|---|
> | `0` | **sell** |
> | `1` | **buy** |
>
> Bu eşleme tahmine dayanmadığı için güvenle kullanılabilir; yine de borsa değiştirirse
> sessizce yanlış yön üretir, bu nedenle bir regresyon testiyle sabitlenmiştir.

### 431 — Order book (tam görüntü)

```json
[431, {
  "CS": 2721198,
  "PS": "BTCTRY",
  "AO": [{ "A": "0.0496469", "P": "3780992" }],
  "BO": [{ "A": "0.02538711", "P": "3781380" }],
  "channel": "orderbook", "event": "BTCTRY", "type": 431
}]
```

| Alan | Anlamı |
|---|---|
| `CS` | **Sıra numarası** (change set) — delta bütünlüğü için |
| `PS` | Parite sembolü |
| `AO` / `BO` | Satış / alış kademeleri (`A` miktar, `P` fiyat) |

> ⚠️ `CS` alanı emir defteri senkronizasyonunun temelidir: 432 (fark) mesajları
> geldiğinde sıra atlanıp atlanmadığı buradan anlaşılır. Atlama varsa defter
> geçersiz sayılıp yeni bir tam görüntü alınmalıdır.

---

## Private akış (henüz uygulanmadı)

Giriş mesajı, resmi dokümantasyondaki biçimiyle:

```json
[114, { "type": 114, "publicKey": "...", "timestamp": 0, "nonce": 3000, "signature": "..." }]
```

> ⚠️ İmza **REST'ten farklı üretiliyor gibi görünüyor**: dokümantasyon imzanın
> `publicKey + nonce` üzerinden hesaplandığını söylüyor, REST ise `apiKey + stamp`
> kullanıyor. Bu fark canlı bir hesapla doğrulanmadan uygulanmamalıdır — yanlış
> imza sessizce başarısız olur.

**Emir iptalinin kesinleşmesi kanal 452 (`OrderDelete`) üzerinden duyurulur.**
REST `DELETE /api/v1/order` çağrısının 200 dönmesi iptalin tamamlandığı anlamına
gelmez; kesinleşme yalnızca bu akıştan öğrenilir.

---

## Henüz doğrulanmamış

- 401 (TickerAll), 422 (TradeSingle), 432 (OrderBookDifference) gövdeleri
- `S` alanındaki 0/1 değerlerinin hangi yöne karşılık geldiği
- Private akışın tamamı (canlı hesap gerekiyor)
- Ping/pong ya da kalp atışı mekanizması
- Bağlantı limiti: dokümantasyon dakikada 15 bağlantı isteği, aşımda 60 sn engel
  belirtiyor (abonelik istekleri bu limite dahil değil)
