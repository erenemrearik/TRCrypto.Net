# BtcTurk — Vendor Capability Freeze

> **Story:** BTC-001 · **Erişim tarihi:** 24 Ağustos 2026
> **Kaynak:** https://docs.btcturk.com/ (resmi dokümantasyon)
>
> Bu dosya kopyalanmış tam doküman **değildir**; kaynak linkli bir envanterdir (spesifikasyon Bölüm 17.3).
> ADR-010 uyarınca **yalnızca resmi dokümantasyon source-of-truth'tur**. Üçüncü taraf wrapper'lar
> (ccxt, arşivlenmiş `BtcTurk.Net`) yalnızca keşif amaçlı kullanılabilir, kontrat kaynağı değildir.

## Genel

| Alan | Değer |
|---|---|
| Base URL | `https://api.btcturk.com` |
| Public API sürümü | v2 |
| Private API sürümü | v1 |
| Durum sayfası | https://status.btcturk.com/ |
| İstek sembol formatı | `BTCTRY` — birleşik, büyük harf, ayırıcı **yok** |
| Zaman damgası | **milisaniye** (OHLC hariç) |
| OHLC zaman damgası | **saniye** |
| Content-Type | Auth gerektiren isteklerde `application/json` |
| Parametre sırası | Serbest |

### Yanıt zarfı (envelope)

Tüm endpoint'ler şu yapıyı döndürür:

```json
{ "success": true, "message": null, "code": 0, "data": { } }
```

> ⚠️ **Kritik 1:** `success` HTTP 200 içinde `false` olabilir. İş mantığı hatası HTTP durum
> koduna yansımaz. Bu nedenle `success == false` durumu `HttpResult.Success == false` olarak
> yüzeye çıkarılmalıdır (spesifikasyon Bölüm 10.5).
>
> ⚠️ **Kritik 2 — `code` alanının tipi uçlar arasında tutarsızdır:**
>
> | Uç | `code` değeri |
> |---|---|
> | exchangeinfo / ticker / trades | `0` — **sayı** |
> | **orderbook** | `"SUCCESS"` — **metin** |
>
> Alanı `int` olarak modellemek emir defteri çağrılarını deserialization hatasıyla kırar.
> Bu davranış resmi dokümantasyonda **geçmez**; canlı API ile tespit edilmiştir.
> Değer metin olarak taşınır.
>
> ⚠️ **Kritik 3:** Başarılı yanıtlarda `message` alanı `null` değil **boş string** döner.

## Public Endpoint'ler

| # | Method | Path | Parametreler | MVP eşlemesi |
|---|---|---|---|---|
| 1 | GET | `/api/v2/server/exchangeinfo` | — | `ISpotSymbolRestClient` + server time |
| 2 | GET | `/api/v2/ticker` | `pairSymbol` (ops.) | `ISpotTickerRestClient` |
| 3 | GET | `/api/v2/ticker/currency` | `symbol` (ör. `usdt`) | native |
| 4 | GET | `/api/v2/orderbook` | `pairSymbol` (zor.), `limit` (ops., varsayılan **25**) | `IOrderBookRestClient` |
| 5 | GET | `/api/v2/trades` | `pairSymbol` (zor.), `last` (ops., **max 50**) | `IRecentTradeRestClient` |
| 6 | GET | OHLC / kline | ⏳ PR-002'de doğrulanacak | `IKlineRestClient` |

### 1 — Exchange Info

`GET /api/v2/server/exchangeinfo` · parametresiz

`data` alanları:

- `timeZone` — `"UTC"`
- `serverTime` — Unix ms (ör. `1641916253216`)
- `symbols[]` — parite listesi
- `currencies[]` — varlık bilgisi (`minWithdrawal`, `minDeposit`, `precision`, yatırma/çekme durum bayrakları)

`symbols[]` öğe alanları:

| Alan | Açıklama |
|---|---|
| `id` | Sayısal parite kimliği |
| `name` | `"BTCTRY"` — native sembol |
| `nameNormalized` | `"BTC_TRY"` |
| `numerator` / `denominator` | **`"BTC"` / `"TRY"`** — base ve quote varlık, ayrı alanlar |
| `numeratorScale` / `denominatorScale` | Ondalık hassasiyet |
| `status` | `"TRADING"` |
| `hasFraction` | bool |
| `filters[]` | `PRICE_FILTER`: `minPrice`, `maxPrice`, `tickSize`, `minExchangeValue` |
| `orderMethods[]` | `MARKET`, `LIMIT`, `STOP_MARKET`, `STOP_LIMIT` |
| `displayFormat` | Sayı biçimi deseni |
| `commissionFromNumerator` | bool |
| `order` | Görüntüleme sırası |
| `priceRounding`, `isNew` | bool |
| `marketPriceWarningThresholdPercentage` | decimal |

> ✅ **`serverTime` bu yanıtın içindedir** — ayrı bir server-time endpoint'ine ihtiyaç yoktur.
> `GetServerTimestampAsync()` bu endpoint üzerinden karşılanır.
>
> ✅ **`numerator`/`denominator` ayrı alan olarak gelir.** `SharedSymbol` eşlemesi bu alanlardan
> yapılır; sembol string'i **ayrıştırılmaz**. Bu, RISK-05'i (TL/TRY alias) ortadan kaldırır.

### 4 — Order Book

`GET /api/v2/orderbook?pairSymbol=BTCTRY&limit=25`

```json
{ "success": true, "message": null, "code": 0,
  "data": { "timestamp": 1543836448605,
            "bids": [["33245.00","2.10695265"], ["33209.00","0.001"]],
            "asks": [["33490.00","0.03681877"]] } }
```

`bids`/`asks`: `[fiyat, miktar]` — her ikisi de **string**, `decimal`'e dönüştürülür.

> ⚠️ Gerçek zamanlı veride gecikme/arıza durumunda bu endpoint **HTTP 503** döndürür.
> Geçici hata olarak sınıflandırılmalıdır (`IsTransient`).

### 5 — Trades

`GET /api/v2/trades?pairSymbol=BTCUSDT&last=30` · `last` max **50** (istemci tarafında doğrulanır)

```json
{ "success": true, "message": null, "code": 0,
  "data": [{ "pair": "BTCTRY", "pairNormalized": "BTC_TRY",
             "numerator": "BTC", "denominator": "TRY",
             "date": 1533650242300, "tid": "636692470417865271",
             "price": "33490", "amount": "0.00032747" }] }
```

## Authentication (M2 — henüz implement edilmedi)

Kaynak: `docs.btcturk.com/docs/authentication/authentication-v1`

| Header | İçerik |
|---|---|
| `X-PCK` | API public key |
| `X-Stamp` | Nonce — UTC **milisaniye** |
| `X-Signature` | HMAC-SHA256 imza |

**İmza zinciri:**

1. Mesaj = `apiKey + stamp` (string birleştirme)
2. Secret **Base64 decode** edilir → HMAC anahtarı
3. `HMAC-SHA256(decodedSecret, mesaj)` → ham digest
4. Digest **Base64 encode** edilir → `X-Signature`

> ⚠️ 2. adım (Base64 decode) atlanırsa imza **sessizce yanlış** olur. En sık yapılan hata budur;
> deterministic test vector ile doğrulanması zorunludur.

Sunucu saati UTC ms ile senkron olmalıdır; kayma durumunda istekler reddedilir.

### API key izinleri

Panel: **Hesap > API Erişimi**. Key oluştururken IP adresi girişi form'un parçasıdır.

| İzin | Kapsam |
|---|---|
| Total Funds (Toplam Varlık) | Bakiye uçları |
| Trade (Al-Sat) | Emir işlemleri |
| Account (Hesap) | İşlem geçmişi |
| WebSocket | Socket mesajları |

Detaylı rehber: [`docs/credentials/btcturk.md`](../credentials/btcturk.md)

### 2 — Ticker

`GET /api/v2/ticker` (tüm pariteler) · `?pairSymbol=BTCTRY` (tek parite — **yine dizi döner**)

Alanlar: `pair`, `pairNormalized`, `numeratorSymbol`, `denominatorSymbol`, `timestamp` (ms),
`last`, `high`, `low`, `bid`, `ask`, `open`, `volume`, `average`, `daily`, `dailyPercent`, `order`.

> ⚠️ Sayısal alanlar bu uçta **sayı** olarak gelir (trades ucunda **metin** olarak gelir).
>
> ⚠️ Base/quote alan adları bu uçta `numeratorSymbol`/`denominatorSymbol` iken trades ucunda
> `numerator`/`denominator`'dur. İsimlendirme uçlar arasında tutarlı değildir.
>
> ⚠️ Tek parite istendiğinde de **dizi** döner; boş dizi bilinmeyen sembol anlamına gelir.

### 5 — Trades (ek alan)

Canlı yanıt, resmi örnekte bulunmayan bir **`side`** alanı içerir (`"buy"` / `"sell"`).

## İstek Limitleri

Kaynak: `docs.btcturk.com/docs/private-endpoints/rate-limits/` — IP bazlı, 24 Ağu 2026.

| Uç | Limit |
|---|---|
| Ticker | 600 / 60 sn |
| Order book | 180 / 60 sn |
| OHLC | 120 / 60 sn |
| Graph API | 600 / 10 dk |
| Bakiye (private) | 120 / 60 sn |
| Emir POST/DELETE (private) | 300 / 60 sn + 10 / 1 sn |
| Emir GET tekil (private) | 900 / 60 sn + 30 / 1 sn |
| WebSocket bağlantı | 15 / dk (aşımda 60 sn engel) |

Kütüphane şu an en kısıtlayıcı public limiti (180/60 sn) genel tavan olarak uygular;
uç bazlı ince ayar ilgili uçlar eklendikçe yapılacaktır.

## Private Endpointler

Hepsi `X-PCK` + `X-Stamp` + `X-Signature` başlıklarını gerektirir.

| # | Method | Path | Durum |
|---|---|---|---|
| 1 | GET | `/api/v1/users/balances` | ✅ Uygulandı |
| 2 | GET | `/api/v1/openOrders?pairSymbol=` | ⏳ Envanteri çıkarıldı |
| 3 | POST | `/api/v1/order` | ⏳ Envanteri çıkarıldı |

### 1 — Account Balance

`GET /api/v1/users/balances` · parametresiz · izin: **Toplam Varlık**

Alanlar: `asset`, `assetname`, `balance`, `locked`, `free`, `orderFund`, `requestFund`,
`precision`, `timestamp` (ms).

> ### ⚠️ Kritik 4 — bu uç ondalık ayırıcı olarak VİRGÜL kullanır
>
> ```json
> "balance": "27223,7283250757643288"
> "free":    "22349,3654565035348765"
> ```
>
> Piyasa verisi ve emir uçları **nokta** kullanır (`"0.00269390"`). Bakiyeyi
> `InvariantCulture` ile ayrıştırmak virgülü binlik ayırıcı sayar ve tutarı
> **kat kat büyük** gösterir — bir bakiye kütüphanesinde kabul edilemez bir hata.
>
> Belirsizlik yoktur: BtcTurk binlik ayırıcı kullanmaz (büyük sayılar `"3708000"`
> biçimindedir), dolayısıyla virgül her zaman ondalık ayırıcıdır.
> Çözüm: `BtcTurkDecimalConverter` her iki ayırıcıyı da kabul eder.

### 2 — Open Orders

`GET /api/v1/openOrders?pairSymbol=BTCTRY` · izin: **Toplam Varlık**

Yanıt `asks` ve `bids` olarak iki listeye ayrılır. Emir alanları: `id`, `price`, `amount`,
`quantity`, `stopPrice`, `pairSymbol`, `type`, `method`, `orderClientId`, `updateTime`,
`status`, `leftAmount`. Ondalıklar **nokta** ayırıcılıdır (`"0.09733687"`).

### 3 — Submit Order

`POST /api/v1/order` · izin: **Al-Sat**

| Parametre | Tip | Zorunlu |
|---|---|---|
| `quantity` | number | Evet |
| `price` | number | Limit emirlerde |
| `orderMethod` | `limit` · `market` · `stoplimit` · `stopmarket` | Evet |
| `orderType` | `buy` · `sell` | Evet |
| `pairSymbol` | string | Evet |
| `stopPrice` | number | Stop emirlerde |
| `newOrderClientId` | string | Hayır |

Dokümantasyon ondalık ayırıcı olarak **nokta** kullanılmasını açıkça belirtir.
Piyasa emirlerinde `price` yok sayılır (%5 tolerans).

> ⚠️ Emir uçları gerçek para hareketi yaratır. ADR-009 uyarınca bu uçlarda otomatik
> yeniden deneme **yapılmaz**.

## Kimlik Doğrulama Test Vektörü

Resmi dokümantasyon test vektörü yayınlamaz. Algoritmanın bağımsız bir uygulamasıyla
üretilen ve birim testlerinde kullanılan vektör (kimlik bilgileri **sahtedir**):

| Alan | Değer |
|---|---|
| apiKey | `test-public-key` |
| secret | `dGVzdC1zZWNyZXQta2V5LWZvci11bml0LXRlc3Rz` |
| stamp | `1735689600000` |
| **imza** | `7gyFGcOS+qnq46h/rl83VtpaEAsh8Th3Z3lQrF7g2I0=` |

Base64 decode adımı atlanırsa imza `38qSfoys8cvFpd0FBe50RUaqT6Dl3iMO7iyblkzlqnw=` olur —
sessizce yanlış, ama borsa yalnızca genel bir kimlik doğrulama hatası döndürür.

## Henüz Dondurulmamış Alanlar

Aşağıdakiler ilgili story açılırken resmi dokümandan doğrulanacaktır:

- OHLC / kline endpoint'i (path, parametreler, **saniye** cinsinden timestamp)
- `GET /api/v2/ticker/currency` yanıt şeması (uygulandı, canlı doğrulaması yapılmadı)
- Emir defteri `limit` parametresinin üst sınırı (dokümante edilmemiş)
- Rate limit değerleri (`private-endpoints/rate-limits`)
- Hata kodları (`error-handling/*`)
- WebSocket protokolü (`websocket-feed/*`) — kanal/olay/model yapısı

## Kaynak Sayfaları

Envanter şu sayfalardan çıkarılmıştır (24 Ağu 2026):

- `docs.btcturk.com/docs/general-information`
- `docs.btcturk.com/docs/public-endpoints/exchange-info/`
- `docs.btcturk.com/docs/public-endpoints/ticker/`
- `docs.btcturk.com/docs/public-endpoints/orderbook/`
- `docs.btcturk.com/docs/public-endpoints/trades/`
- `docs.btcturk.com/docs/api-access-permissions`
- `docs.btcturk.com/docs/authentication/authentication-v1`
