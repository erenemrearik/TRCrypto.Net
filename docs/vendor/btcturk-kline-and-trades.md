# BtcTurk — Kline ve Kullanıcı İşlem Geçmişi

> **Erişim tarihi:** 25 Ağustos 2026
> Ana envanter: [btcturk-capabilities.md](btcturk-capabilities.md)
>
> Bu iki uç, ana envanterden ayrı tutulmuştur çünkü ikisi de ortak kalıbın dışına çıkar.

---

## Kline / OHLC — ayrı host, ayrı format

`GET https://graph-api.btcturk.com/v1/klines/history`

> ### ⚠️ Kritik 6 — bu uç diğerlerinden iki yönden ayrılır
>
> **1. Farklı host.** `api.btcturk.com` değil, **`graph-api.btcturk.com`**.
> Ortak taban adres bu uçta kullanılamaz.
>
> **2. Standart zarf yok.** `success` / `message` / `code` / `data` alanları bulunmaz.
> Zarfı açan ortak yol (`SendAsync<T>`) bu uçta çalışmaz; ham yanıt okunmalıdır.

### Parametreler

| Parametre | Tip | Açıklama |
|---|---|---|
| `symbol` | string | `BTCTRY` |
| `resolution` | int | 1, 15, 30, 60, 240 (dakika); ayrıca gün / hafta / yıl |
| `from` | int | **Unix saniye** |
| `to` | int | **Unix saniye** |

### Canlı yanıt (25 Ağu 2026 doğrulaması)

```json
{
  "s": "ok",
  "t": [1787684400, 1787688000],
  "h": [3809691, 3796611],
  "o": [3802189, 3796611],
  "l": [3787859, 3760727],
  "c": [3796125, 3761074],
  "v": [0.54581564, 0.61832473]
}
```

| Alan | Anlamı |
|---|---|
| `s` | Durum göstergesi (`"ok"`) — **resmi dokümantasyonda geçmez**, canlı yanıtta bulunur |
| `t` | Zaman damgaları — **saniye** |
| `o` `h` `l` `c` | Açılış, en yüksek, en düşük, kapanış |
| `v` | Hacim (base varlık cinsinden) |

> ⚠️ Veriler **paralel diziler** halinde gelir, mum nesneleri olarak değil.
> Tüm dizilerin aynı uzunlukta olması beklenir; olmadığında yanıt bozuk sayılmalıdır.
>
> ⚠️ Zaman damgaları **saniye** cinsindendir — diğer tüm uçlar milisaniye kullanır.
> Milisaniye varsayan bir converter tarihleri 1970'e yakın gösterir.
>
> ⚠️ Değerler **sayı** olarak gelir, metin olarak değil.

**İstek limiti:** Graph API için 600 istek / 10 dakika.

---

## Kullanıcı İşlem Geçmişi

`GET /api/v1/users/transactions/trade` · izin: **Hesap**

### Parametreler

| Parametre | Tip | Açıklama |
|---|---|---|
| `orderId` | long | **Diğer parametrelerle birlikte kullanılamaz** |
| `type` | string[] | `buy`, `sell` |
| `symbol` | string[] | `btc`, `try` … |
| `pairSymbol` | string | `BTCTRY` |
| `startDate` | long | Unix **milisaniye** |
| `endDate` | long | Unix **milisaniye** |

Tarih aralığı verilmezse **son 30 gün** döner.

### Örnek yanıt

```json
{
  "success": true,
  "message": "SUCCESS",
  "code": 0,
  "data": [
    {
      "id": 1181163798924649598,
      "timestamp": 1663848223334,
      "amount": "-0.3384",
      "preciseAmount": -0.3384081100000000,
      "fee": "-0.06297817",
      "tax": "-0.01133607",
      "price": "122.00",
      "numeratorSymbol": "ETHW",
      "denominatorSymbol": "TRY",
      "orderType": "sell",
      "orderId": 10938696222,
      "orderClientId": null
    }
  ]
}
```

> ### ⚠️ Kritik 7 — tutarlar işaretlidir
>
> `amount`, `fee` ve `tax` satış işlemlerinde **negatif** gelir. İşaret, varlığın
> hesaptan çıktığını belirtir. Mutlak değer bekleyen bir hesaplama (toplam hacim,
> komisyon toplamı) işareti yok sayarsa sonuç yanlış çıkar.

> ### ⚠️ `tax` alanı Türkiye'ye özgüdür
>
> Diğer borsalarda karşılığı yoktur ve `CryptoExchange.Net`'in borsadan bağımsız
> işlem modelinde temsil edilemez. Native modelde korunur; shared yüzeyde kaybolur.
> Vergi/kesinti hesabı yapan tüketiciler native API kullanmalıdır.

> ⚠️ `preciseAmount` **sayı**, `amount` **metin** olarak gelir — aynı değerin iki
> gösterimi. Hassasiyet gerektiğinde `preciseAmount` tercih edilmelidir.

---

## Kaynaklar

- `docs.btcturk.com/docs/public-endpoints/get-kline-data/`
- `docs.btcturk.com/docs/private-endpoints/user-transactions/`
- Kline yanıt şeması `graph-api.btcturk.com` üzerinden canlı doğrulandı (25 Ağu 2026)
