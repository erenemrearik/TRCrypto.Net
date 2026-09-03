# Paribu Uç Envanteri

> **Kaynak:** [docs.paribu.com/api](https://docs.paribu.com/api)
> **Erişim tarihi:** 4 Eylül 2026
>
> Bu dosya kopyalanmış bir doküman değildir; kaynağı belirtilmiş bir envanterdir. Yalnızca
> resmi dokümantasyondan ya da canlı denemeden doğrulanan uçlar yazılır.

## Genel

| Alan | Değer |
|---|---|
| Base URL | `https://api.paribu.com` |
| Yol öneki | **Yok.** Yollar base URL'e doğrudan eklenir, `/api` öneki yazılmaz |
| Sembol biçimi | `btc_tl`, **küçük harf ve alt çizgili** |
| Quote varlıklar | `tl` ve `usdt` |
| Kimlik doğrulama | `Authorization` başlığında ham anahtar, `X-Signature` başlığında Base64 HMAC-SHA256 |
| Zaman damgası | `X-Timestamp` başlığı, Unix **milisaniye**, ±5 saniye pencere |
| Emir defteri zaman damgası | Yanıt gövdesinde **saniye** |

## Kritik 1: sembol adında TRY değil TL kullanılır

Paribu Türk lirasını `tl` olarak yazar, `try` olarak değil. BtcTurk `TRY`, Binance TR de
`TRY` kullanır.

| Borsa | BTC/Türk lirası paritesi |
|---|---|
| BtcTurk | `BTCTRY` |
| Binance TR | `BTC_TRY` |
| **Paribu** | **`btc_tl`** |

Sembol biçimlendirmesi bu farkı çevirmek zorundadır. Spesifikasyonun RISK-05 maddesi
(TL ve TRY takma adı) burada teorik bir risk değil, günlük bir gerçektir.

Sembol ayrıca **küçük harflidir.** Büyük harfli bir değerin kabul edilip edilmediği
doğrulanmamıştır; kütüphane küçük harf üretmelidir.

## Kritik 2: bilinmeyen yol hata değil, HTML döndürür

`www.paribu.com` üzerindeki tanımsız bir yol **HTTP 200** ve tek sayfa uygulamasının HTML
kabuğunu döndürür. Denenen dört tanımsız yolun dördü de 200 verdi.

Durum koduna bakarak uç varlığı çıkarmak bu borsada güvenilir değildir; yanıtın
`Content-Type` başlığı da denetlenmelidir. Bu, bir uç yanlış yazıldığında sorunun
"ayrıştırma hatası" olarak görünmesine ve gerçek nedenin geç anlaşılmasına yol açar.

Doğru taban `api.paribu.com`'dur.

## Kritik 3: emir defterinde `depth` parametresi yok sayılıyor

Dokümantasyon `depth` için "en fazla 20" diyor ve parametrenin bids ile asks dizilerinin
uzunluğunu sınırladığını belirtiyor. Canlı ölçüm bunun uygulanmadığını gösteriyor:

| İstenen `depth` | Dönen bids | Dönen asks |
|---|---|---|
| 3 | 20 | 20 |
| 5 | 20 | 20 |
| 20 | 20 | 20 |
| 50 | 20 | 20 |

Ölçüm 4 Eylül 2026, `btc_tl` paritesi. Uç her durumda 20 kademe döndürüyor ve fazla değer
için hata da vermiyor.

Kütüphane bu yüzden istenen kademe sayısını **kendisi kırpmalıdır.** Aksi halde beş kademe
isteyen çağıran taraf yirmi kademe alır ve bunu fark etmez.

## Public uçlar (canlı doğrulandı)

Her ikisi de API anahtarı gerektirmez.

| Method | Path | Durum |
|---|---|---|
| GET | `/market/ticker` | ✅ Canlı doğrulandı |
| GET | `/orderbook` | ✅ Canlı doğrulandı |

### `GET /market/ticker`

`market` parametresi isteğe bağlıdır. Verilmezse tüm pariteler döner.

Yanıt bir **dizidir**, tek parite istendiğinde bile:

```json
[
  {
    "market": "btc_tl",
    "low": "3710152",
    "high": "3950000",
    "first": "3727682",
    "last": "3928181",
    "volume": "40.653359",
    "pair_volume": "153959746",
    "change": "200499",
    "percentage": "5.37",
    "average": "3787134"
  }
]
```

| Alan | Anlamı |
|---|---|
| `market` | Native parite adı |
| `low` · `high` | Son 24 saatteki en düşük ve en yüksek fiyat |
| `first` · `last` | 24 saat önceki ve en son işlem fiyatı |
| `volume` | Base varlık cinsinden hacim |
| `pair_volume` | Quote varlık cinsinden hacim |
| `change` · `percentage` | Mutlak ve yüzde değişim |
| `average` | Ortalama fiyat |

Bütün sayısal değerler **metin** olarak gelir.

### `GET /orderbook`

`market` zorunlu, `depth` yok sayılıyor (Kritik 3).

```json
{
  "timestamp": 1788472458,
  "bids": [["3922677", "0.037471"], ["3922675", "0.044722"]],
  "asks": [["3928000", "0.012000"], ["3929500", "0.030000"]]
}
```

Kademeler `[fiyat, miktar]` dizisidir ve her iki değer de metindir.

> Zaman damgası **saniye** cinsindendir. Kimlik doğrulamadaki `X-Timestamp` ise
> milisaniyedir; aynı borsa içinde iki farklı birim kullanılır.

## Kimlik doğrulama

Private uçların tamamı HMAC-SHA256 imzası ister. İmzasız istek `401 Unauthorized` alır.

| Başlık | Değer |
|---|---|
| `Authorization` | Ham public anahtar. **`Bearer` öneki yoktur** |
| `X-Signature` | `HMAC-SHA256(secret, payload)` sonucunun **Base64** hali |
| `X-Timestamp` | Unix **milisaniye**. Şu an isteğe bağlı, ileride zorunlu olacak; ±5 saniye |

`POST` ve `PUT` isteklerinde ayrıca `Content-Type: application/json` gönderilir.

### İmza yükü

```
payload = timestamp + queryString + body
```

Üç parça **ayırıcı olmadan** peş peşe eklenir.

| Parça | Kural |
|---|---|
| `timestamp` | `X-Timestamp` başlığındaki değerin aynısı. Başlık gönderilmiyorsa boş metin |
| `queryString` | Baştaki `?` olmadan, URL'de göründüğü haliyle ham sorgu dizesi. Sorgu yoksa boş metin |
| `body` | Ham istek gövdesi. `GET` ve `DELETE` için boş metin |

**İstek yolu imzaya dahil değildir.** Bu, üç borsanın üçünde de farklı olan bir ayrıntıdır
ve yanlış varsayılırsa imza sessizce geçersiz olur.

### Üç borsanın imzalama şeması

| | BtcTurk | Binance TR | Paribu |
|---|---|---|---|
| Anahtar başlığı | `X-PCK` | `X-MBX-APIKEY` | `Authorization` |
| İmza yeri | `X-Signature` başlığı | `signature` parametresi | `X-Signature` başlığı |
| İmzalanan | `anahtar + damga` | sorgu dizesi ve gövde | `damga + sorgu + gövde` |
| Secret | Base64, çözülür | ham metin | ham metin |
| İmza kodlaması | Base64 | onaltılık | Base64 |
| Zaman damgası | `X-Stamp`, ms | `timestamp` parametresi, ms | `X-Timestamp`, ms |
| Zaman penceresi | geniş | `recvWindow`, varsayılan 5000 ms | ±5 saniye |

Üçü de HMAC-SHA256 kullanır ve üçünde de imzalanan değer farklıdır. Ortak bir yardımcı
sınıf yazma denemesi, hangi borsada yanlış imza üretildiğini gizlemekten başka bir işe
yaramaz.

## Dokümantasyonda yer alan, henüz doğrulanmamış uçlar

Aşağıdakiler resmi dokümantasyon indeksinde listelidir ancak bu envanterde henüz
dondurulmamıştır. Şemaları çıkarılmadan koda yazılmayacaktır.

| Alan | Kaynak sayfa |
|---|---|
| Emir oluşturma ve iptali | `orders/placing-orders`, `orders/cancelling-orders` |
| Emir ayrıntısı ve açık emirler | `orders/order-details-and-open-orders` |
| İstemci emir kimliği (v2) | `orders/client-order-id-v2` |
| Kullanıcı bilgisi ve varlıklar | `account/user-info`, `account/assets` |
| İşlem geçmişi | `account/trades-history` |
| Transferler | `account/transfers` |
| Yatırma ve çekme | `deposit-and-withdrawals/*` |
| WebSocket akışları | `streams-v2/overview`, `public-streams`, `private-streams` |
| İstek limitleri | `error-handling/rate-limiting` |
| Hata kodları | `error-handling/common-error-codes` |

WebSocket tarafında public akışların kimlik doğrulama gerektirmediği, private akışların ise
bağlantı başına API anahtarı istediği belirtiliyor.

## Kaynaklar

- `docs.paribu.com/api` ve alt sayfaları, 4 Eylül 2026
- Canlı uç denemeleri, 4 Eylül 2026: `api.paribu.com/market/ticker`, `api.paribu.com/orderbook`
