# Proje Durumu — Nerede Kaldık?

> **Son güncelleme:** 25 Ağustos 2026
> Bu dosya, projeye ara verip döndüğünüzde ya da yeni biri katıldığında okunacak
> tek sayfalık özettir. Ayrıntı için ilgili belgelere bakın.

---

## Tek cümleyle

BtcTurk'ün **public piyasa verisi** uçları çalışıyor (native + borsadan bağımsız yüzey);
**kimlik doğrulama, emir işlemleri ve WebSocket henüz yok**; diğer üç borsaya başlanmadı.

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

**Shared yüzey** (`client.SpotApi.SharedClient`):
`ISpotSymbolRestClient` · `ISpotTickerRestClient` · `IOrderBookRestClient` · `IRecentTradeRestClient`

### Belgeler ✅

| Dosya | İçerik |
|---|---|
| `docs/credentials/README.md` | Genel güvenlik: saklama, least-privilege, sızıntı durumu |
| `docs/credentials/btcturk.md` | BtcTurk'te adım adım API anahtarı alma ve bağlama |
| `docs/vendor/btcturk-capabilities.md` | Resmi kaynaklı endpoint envanteri + istek limitleri |
| `docs/spec/` | Orijinal spesifikasyon + doğrulama ekleri (D-1…D-12) |

---

## Yapılmayanlar

| Konu | Neden |
|---|---|
| **Kimlik doğrulama (imzalama)** | Planlı olarak M2'ye bırakıldı. `BtcTurkAuthenticationProvider` iskeleti var, `ProcessRequest` bilinçli olarak `NotSupportedException` fırlatıyor |
| **Bakiye / emir işlemleri** | Kimlik doğrulamaya bağlı |
| **WebSocket** | M3 |
| **OHLC / kline** | Endpoint path'i resmi dokümandan doğrulanamadı; **uydurmaktansa yazılmadı** |
| **Binance TR · Paribu · Bitexen** | M4–M6 |
| **`gitleaks` yerel taraması** | Araç makinede kurulu değil. Yapılandırma ve hook hazır; CI'da çalışacak |

---

## Doğrulama durumu

Son çalıştırma (25 Ağu 2026):

```
dotnet build -c Release   →  0 error, 5 TFM
dotnet test  -c Release   →  37/37 geçti
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

Ayrıntısı `docs/spec/` ekinde D-7…D-12. En önemlisi:

> **`code` alanının tipi uçlar arasında tutarsız:** çoğu uç `0` (sayı) döndürürken
> emir defteri `"SUCCESS"` (metin) döndürüyor. `int` olarak modellemek emir defteri
> çağrılarını tamamen kırıyordu.

Diğerleri: başarılı yanıtta `message` boş string (null değil); `numeratorSymbol` vs
`numerator` isim tutarsızlığı; ticker tek parite için de dizi döndürüyor; işlem
yanıtında dokümante edilmemiş `side` alanı.

---

## Sonraki adım: M2 (kimlik doğrulama + private REST)

**API anahtarı olmadan başlanabilir.** İmzalama zinciri resmi dokümandan doğrulanmıştır:

```
1. mesaj    = apiKey + stamp          (stamp: UTC milisaniye)
2. anahtar  = Base64Decode(secret)     ← atlanırsa imza sessizce yanlış olur
3. digest   = HMAC-SHA256(anahtar, mesaj)
4. X-Signature = Base64Encode(digest)
```

Başlıklar: `X-PCK` · `X-Stamp` · `X-Signature`

**Sıra:**
1. `BtcTurkAuthenticationProvider` — sabit test vektörüyle doğrulanır, canlı hesap gerekmez
2. Vendor freeze: `private-endpoints/*` ve `error-handling/*` sayfaları
3. Bakiye → açık emirler → emir sorgulama → emir verme/iptal
4. Shared: `IBalanceRestClient`, `ISpotOrderRestClient`

**Anahtar geldiğinde gereken minimum:** *Toplam Varlık* + *Hesap* izinleri, *Al-Sat* kapalı,
**Çekim kapalı**, IP allow-list dolu. Ayrıntı: `docs/credentials/btcturk.md`.

---

## ⚠️ Açık konu: sistem saati

Geliştirme makinesi ile BtcTurk sunucusu arasında tutarlı olarak **~19 saniye** fark ölçüldü.
`X-Stamp` UTC milisaniye gerektirdiğinden bu, M2'de imzalı isteklerin reddedilmesine yol
açabilir. M2'ye başlamadan önce NTP ile senkronizasyon önerilir.

Kontrol:

```csharp
var serverTime = await client.SpotApi.ExchangeData.GetServerTimeAsync();
Console.WriteLine((DateTime.UtcNow - serverTime.Data).TotalSeconds);
```
