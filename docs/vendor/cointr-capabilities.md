# CoinTR Uç Envanteri

> **Kaynak:** [cointr-ex.github.io/openapis](https://cointr-ex.github.io/openapis/) ·
> [cointr.com/api-doc](https://www.cointr.com/api-doc/common/intro)
> **Erişim tarihi:** 8 Eylül 2026
>
> Bu dosya kopyalanmış bir doküman değildir; kaynağı belirtilmiş bir envanterdir. Yalnızca
> resmi dokümantasyondan ya da canlı denemeden doğrulanan uçlar yazılır.
>
> **Uygulama durumu:** aşağıdaki altı public uç ve üç WebSocket kanalı
> `src/TRCrypto.CoinTR` içinde uygulandı ve canlı doğrulandı. Private uçlar, imzalama
> gerçek bir hesapta kabul edilene kadar yayımlanmayacaktır.

## Genel

| Alan | Değer |
|---|---|
| REST base URL | `https://api.cointr.pro` |
| WebSocket base URL | `wss://ws.cointr.pro/v2/ws/public` |
| Sembol biçimi | `BTCTRY`, **birleşik ve büyük harf** |
| Quote varlık | `TRY` |
| Parite sayısı | 309 (7 Eylül 2026) |
| Kimlik doğrulama | `ACCESS-KEY`, `ACCESS-PASSPHRASE`, `ACCESS-SIGN`, `ACCESS-TIMESTAMP` |
| Zaman damgası | Unix **milisaniye** |

## Kritik 1: başarı kodu metindir, sayı değil

Zarf `{code, msg, requestTime, data}` biçimindedir ve başarı `code` alanının **`"00000"`**
olmasıyla anlaşılır. Değer bir **metindir**, sayı değil.

```json
{
  "code": "00000",
  "msg": "success",
  "requestTime": 1788787399131,
  "data": [ ... ]
}
```

Bu, projedeki dördüncü farklı zarf biçimidir:

| Borsa | Başarı göstergesi |
|---|---|
| BtcTurk | `success` alanı, mantıksal değer |
| Binance TR | `code == 0`, sayı |
| Paribu | HTTP durum kodu, gövdede zarf yok |
| **CoinTR** | **`code == "00000"`, metin** |

`code` alanını `int` olarak okumak burada ayrıştırma hatası verir. BtcTurk'te aynı alanın
uçlar arasında sayı ve metin olarak değiştiği zaten belgelenmişti (D-7); CoinTR bunu her
uçta metin yaparak tutarlı davranıyor, ama diğerlerinden farklı bir tutarlılıkla.

## Kritik 2: kimlik doğrulama şeması dokümantasyondan farklı

`cointr-ex.github.io` üzerindeki açıklama, imzanın iki aşamada üretildiğini ve HMAC
anahtarının 30 saniyelik zaman diliminden türetildiğini söylüyor. Başlıklar için de
`X-COINTR-APIKEY` ve `X-COINTR-SIGN` veriyor.

**Çalışan şema bu değil.** Aşağıdaki şema üretimde kullanılan bir entegrasyondan
doğrulanmıştır.

### Başlıklar

| Başlık | Değer |
|---|---|
| `ACCESS-KEY` | API anahtarı |
| `ACCESS-PASSPHRASE` | **Parola.** Anahtar oluşturulurken belirlenir |
| `ACCESS-SIGN` | `HMAC-SHA256` sonucunun **Base64** hali |
| `ACCESS-TIMESTAMP` | Unix **milisaniye** |
| `locale` | `en-US` |

> [!IMPORTANT]
> Kimlik bilgisi **üç parçalıdır.** Diğer üç borsa anahtar ve secret ile yetinir; burada
> ayrıca bir parola vardır ve eksikse istek reddedilir. Dokümantasyondan okunan şemada bu
> alan hiç geçmiyordu.

### İmza

```
preHash = timestamp + METHOD + path + query + body
imza    = Base64(HMAC-SHA256(secret, preHash))
```

| Parça | Kural |
|---|---|
| `timestamp` | `ACCESS-TIMESTAMP` başlığındaki değerin aynısı, milisaniye |
| `METHOD` | Büyük harf: `GET`, `POST` |
| `path` | `/api/v2` öneki dahil tam yol |
| `query` | Baştaki `?` **dahil**; sorgu yoksa boş metin |
| `body` | Ham JSON gövde; `GET` için boş metin |

Yol imzaya **dahildir**. Bu, dört borsa arasında yalnızca burada geçerlidir.

### Dört borsanın imzalama şeması

| | BtcTurk | Binance TR | Paribu | CoinTR |
|---|---|---|---|---|
| Kimlik parçası | 2 | 2 | 2 | **3** (parola ile) |
| Anahtar başlığı | `X-PCK` | `X-MBX-APIKEY` | `Authorization` | `ACCESS-KEY` |
| İmza yeri | `X-Signature` | `signature` parametresi | `X-Signature` | `ACCESS-SIGN` |
| İmzalanan | `anahtar + damga` | sorgu ve gövde | `damga + sorgu + gövde` | `damga + metot + yol + sorgu + gövde` |
| Yol imzada mı | hayır | hayır | hayır | **evet** |
| Secret | Base64, çözülür | ham metin | ham metin | ham metin |
| İmza kodlaması | Base64 | onaltılık | Base64 | Base64 |

Dördü de HMAC-SHA256 kullanır ve hiçbirinde imzalanan değer aynı değildir. Ortak bir
yardımcı sınıf yazma denemesi, hangi borsada yanlış imza üretildiğini gizlemekten başka
bir işe yaramaz.

### Bu şema neden tanıdık

Zarfın `code: "00000"` biçimi, `ACCESS-*` başlıkları ve parola alanı Bitget'in
konvansiyonlarıdır. CoinTR bu altyapıyı kullanıyor. Bitget için yazılmış bir istemcinin
davranışı burada iyi bir tahmin kaynağıdır, ancak yine de kaynak olarak resmi
dokümantasyon ve canlı ölçüm esas alınır.

## WebSocket (canlı doğrulandı)

| Alan | Değer |
|---|---|
| Adres | `wss://ws.cointr.pro/v2/ws/public` |
| Abonelik | `{"op":"subscribe","args":[{"instType":"SPOT","channel":"ticker","instId":"BTCTRY"}]}` |
| Onay | `{"event":"subscribe","arg":{...}}` |
| Veri | `{"action":"snapshot"\|"update","arg":{...},"data":[...]}` |
| Canlılık | Sunucu düz metin `pong` çerçevesi gönderir; JSON değildir |

Kanallar: `ticker`, `books`, `books1`, `books5`, `books15`, `trade`, `candle<aralık>`.

`books` farklı gönderir, `books5` ve `books15` tam görüntü gönderir. İlk mesajın
`action` alanı `snapshot`, sonrakiler `update` olur.

Ticker gövdesi:

```json
{
  "instId": "BTCTRY",
  "lastPr": "3830046",
  "open24h": "3810376", "high24h": "3901118", "low24h": "3800024",
  "change24h": "-0.00912",
  "bidPr": "3829767", "askPr": "3830901",
  "bidSz": "0.05552", "askSz": "0.04752",
  "baseVolume": "13.72871", "quoteVolume": "52468521.90"
}
```

> `change24h` **kesirdir**, yüzde değil: `-0.00912` yüzde 0,912 düşüş demektir. REST
> ticker ucu da aynı biçimi kullanır. Doğrudan aktarmak değeri yüz kat küçük gösterir.

Düz metin `pong` çerçevesi JSON çözümleyicisini düşürür; mesaj işleyicisi bunu JSON
olarak ayrıştırmaya çalışmadan önce elemelidir.


## Public uçlar (canlı doğrulandı)

Dördü de API anahtarı gerektirmez. Ölçüm 7 Eylül 2026.

| Method | Path | Durum |
|---|---|---|
| GET | `/api/v2/public/time` | ✅ Canlı doğrulandı |
| GET | `/api/v2/spot/public/symbols` | ✅ Canlı doğrulandı |
| GET | `/api/v2/spot/market/tickers` | ✅ Canlı doğrulandı |
| GET | `/api/v2/spot/market/orderbook` | ✅ Canlı doğrulandı |
| GET | `/api/v2/spot/market/fills` | ✅ Canlı doğrulandı |
| GET | `/api/v2/spot/market/candles` | ✅ Canlı doğrulandı |

### `GET /api/v2/spot/public/symbols`

309 parite döner. Base ve quote varlık **ayrı alanlardadır**, sembol adı ayrıştırılmaz.

```json
{
  "symbol": "OPTRY",
  "baseCoin": "OP",
  "quoteCoin": "TRY",
  "minTradeAmount": "0",
  "maxTradeAmount": "10000000000",
  "takerFeeRate": "0.001",
  "makerFeeRate": "0.001",
  "pricePrecision": "2",
  "quantityPrecision": "2",
  "quotePrecision": "6",
  "status": "online",
  "minTradeUSDT": "1",
  "buyLimitPriceRatio": "0.02",
  "sellLimitPriceRatio": "0.02",
  "areaSymbol": "no"
}
```

Komisyon oranları parite başına bildirilir; diğer üç borsada bu bilgi ya hesap ucundadır
ya da hiç yoktur.

### `GET /api/v2/spot/market/tickers`

`symbol` parametresi isteğe bağlıdır. Yanıt tek parite istendiğinde bile **dizidir**.

```json
{
  "open": "3857566", "symbol": "BTCTRY",
  "high24h": "3901118", "low24h": "3813063",
  "lastPr": "3846834",
  "quoteVolume": "53409991.10738", "baseVolume": "13.86216",
  "usdtVolume": "1105202.659684778281",
  "ts": "1788787397722",
  "bidPr": "3846177", "askPr": "3847875",
  "bidSz": "0.04892", "askSz": "0.06114",
  "openUtc": "3883216",
  "changeUtc24h": "-0.00937", "change24h": "-0.00205"
}
```

Değişim oranları **kesir** olarak gelir, yüzde olarak değil: `-0.00205` yüzde 0,205
düşüş demektir. BtcTurk ve Paribu aynı bilgiyi yüzde olarak verir; doğrudan aktarmak
değeri yüz kat küçük gösterir.

Bütün sayısal değerler metindir.

### `GET /api/v2/spot/market/orderbook`

`symbol` zorunlu, `limit` isteğe bağlı. Kademeler `[fiyat, miktar]` dizisidir.

```json
{
  "asks": [["3847875", "0.06114"], ["3848024", "0.06007"]],
  "bids": [["3846177", "0.04892"], ["3845969", "0.04997"]],
  "ts": "1788787409981"
}
```

`limit` parametresinin gerçekten uygulandığı doğrulandı: 5 istendiğinde 5 kademe döndü.
Paribu'nun aksine bu parametre yok sayılmıyor.

### `GET /api/v2/spot/market/fills`

Son işlemler. Yön `side` alanında **metin** olarak gelir (`buy` ya da `sell`), Binance
TR'deki gibi sayı değil.

```json
{
  "symbol": "BTCTRY",
  "tradeId": "1480847216137556071",
  "side": "sell",
  "price": "3847741",
  "size": "0.00009",
  "ts": "1788787409815"
}
```

### `GET /api/v2/spot/market/candles`

`symbol` ve `granularity` alır. Mumlar **isimsiz dizi** olarak döner:

```json
[
  "1788787320000",
  "3846995", "3847996", "3846195", "3847544",
  "0.00771", "613.668103686567", "29660.69637"
]
```

Sıra: açılış zamanı, açılış, en yüksek, en düşük, kapanış, base hacim, quote hacim,
USDT hacmi. Alan adı taşımadığı için sıranın doğrulanması gerekir; yukarıdaki eşleme
canlı yanıttaki değerlerin büyüklüklerinden çıkarılmıştır ve **kod yazılmadan önce
dokümantasyonla karşılaştırılacaktır.**

## Dokümantasyonda yer alan, henüz doğrulanmamış uçlar

| Alan | Not |
|---|---|
| Hesap ve bakiye | İmza şeması doğrulanmadan denenmeyecek |
| Emir oluşturma, iptal, sorgulama | Aynı |
| İşlem geçmişi | Aynı |
| Vadeli işlem akışları | Kapsam dışı; TRCrypto yalnızca spot sunar |
| Vadeli işlemler | Kapsam dışı; TRCrypto yalnızca spot sunar |
| İstek limitleri | Belgelenmedi |

## Kaynaklar

- `cointr-ex.github.io/openapis`, 7 Eylül 2026
- `cointr.com/api-doc/common/intro`, 7 Eylül 2026
- Canlı uç denemeleri, 7 Eylül 2026: `api.cointr.pro` üzerinde altı public uç
