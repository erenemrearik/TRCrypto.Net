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

> ⚠️ **Kritik:** `success` HTTP 200 içinde `false` olabilir. İş mantığı hatası HTTP durum koduna
> yansımaz. Bu nedenle `success == false` durumu `HttpResult.Success == false` olarak
> yüzeye çıkarılmalıdır (spesifikasyon Bölüm 10.5).

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

## Henüz Dondurulmamış Alanlar

Aşağıdakiler ilgili story açılırken resmi dokümandan doğrulanacaktır:

- OHLC / kline endpoint'i (path, parametreler, **saniye** cinsinden timestamp)
- Private endpoint'ler: `account-balance`, `open-orders`, `all-orders`, `get-single-order`,
  `submit-order`, `cancel-order`, `user-transactions`
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
