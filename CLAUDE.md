# TRCrypto.Net

Türkiye'deki lisanslı kripto platformları için .NET client ekosistemi.
[JKorf/CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) 12.5.0 üzerine kuruludur.

Her borsa bağımsız bir NuGet paketidir ve iki yüzey sunar: borsanın tüm özelliklerine
erişen **native** yüzey ve `CryptoExchange.Net.SharedApis` ile borsadan bağımsız kod
yazmayı sağlayan **shared** yüzey.

Ayrıntılı durum ve yol haritası: [docs/DURUM.md](docs/DURUM.md)
Dokümantasyon sitesi: <https://erenemrearik.github.io/TRCrypto.Net/>

---

## Bu depoda çalışırken uyulacak kurallar

Aşağıdakiler tercih değil, projenin çalışma biçimidir. Her biri yaşanmış bir hatanın
sonucudur.

### 1. Endpoint uydurulmaz

Yalnızca resmi dokümantasyondan ya da canlı denemeden doğrulanmış uçlar yazılır.
Doğrulanamayan bir uç, tahmin edilerek yazılmak yerine `docs/vendor/` altında
"dondurulmamış" olarak işaretlenir.

Borsaların dokümantasyonu eksik ve zaman zaman yanlıştır. Şimdiye kadar 44 sapma tespit
edildi ve `docs/spec/` ekinde D-1 ile D-44 arasında numaralanarak belgelendi. Bir davranışı
doğrulamadan koda yazmak, bu listenin uzamasına değil, sessiz hatalara yol açar.

### 2. Sessiz başarısızlıklar en tehlikelisidir

Bu borsalarda en sık karşılaşılan sorun, hata dönmeyen ama yanlış çalışan davranışlardır:

- BtcTurk tüm ticker aboneliğinde olay adı boş bırakılırsa istek **onaylanır** ve hiç
  mesaj gelmez.
- Binance TR'de yanlış sembol biçimiyle abone olunursa bağlantı kurulur, veri akmaz.
- Binance TR'nin `market/trades` ve `market/klines` uçları başarı koduyla **boş liste**
  döndürür.
- Yönlendirme eşlemesi gövde tipine göre kayıtlıdır; yeni bir tip için kayıt unutulursa
  abonelik başarılı görünür ama mesaj ulaşmaz.

Böyle bir davranış bulunduğunda testle sabitlenir ve testin gerçekten yakaladığı,
düzeltme geri alınarak kanıtlanır.

### 3. Test önce yazılır, sonra kanıtlanır

Yeni bir uç ya da davranış için önce fixture ve test yazılır, kırmızı olduğu görülür,
sonra uygulama yazılır. Kritik bir düzeltmede test, düzeltme geçici olarak geri alınarak
gerçekten kırmızıya döndüğü doğrulanır. Geçtiğini görmek yetmez; yakaladığını görmek
gerekir.

### 4. İki test türü ayrıdır

| Tür | Ne yapar | Ne zaman çalışır |
|---|---|---|
| Birim | Ağa çıkmaz; sabit yanıtlarla çözümleme, istek kurulumu ve doğrulama | Her PR'da |
| Canlı (`*.IntegrationTests`) | Borsanın gerçek API'sine çıkar | Haftalık zamanlanmış iş |

Canlı testler PR akışında çalıştırılmaz: borsanın istek limitini her PR için yakmamak ve
derlemeyi borsanın erişilebilirliğine bağımlı kılmamak için. Kimlik bilgisi isteyen canlı
testler anahtar tanımlı değilse atlanır, başarısız olmaz.

```bash
dotnet test -c Release --filter "FullyQualifiedName!~IntegrationTests"   # yalnizca birim
dotnet test -c Release --filter "FullyQualifiedName~IntegrationTests"    # yalnizca canli
```

### 5. Kimlik bilgileri depoya girmez

API anahtarları yalnızca `dotnet user-secrets` ile, depo dizininin tamamen dışında
saklanır. Anahtar, secret ve imza hiçbir günlük satırına, istisna mesajına ya da telemetri
alanına girmez. `.gitignore`, `.gitleaks.toml` ve pre-commit kancası bunu ayrıca korur.

Kullanıcıya kurulum anlatırken kabuğun `cmd.exe` olduğu varsayılır; PowerShell sözdizimi
çalışmaz.

### 6. Dil ayrımı

| Nerede | Dil |
|---|---|
| Commit mesajları, PR başlıkları, dal adları | İngilizce |
| Kod içi yorumlar, XML dokümantasyonu | Türkçe |
| `docs/` altındaki belgeler ve site | Türkçe |
| Kullanıcıyla konuşma | Türkçe |

**Commit mesajlarına `Co-Authored-By` satırı eklenmez.** Kullanıcı bunu açıkça istedi ve
geçmiş commit'lerden de temizletti.

### 7. Yazım biçimi

Belgeler ve site metinleri akıcı, profesyonel ve doğrudan olmalıdır. Uzun tire noktalama
olarak kullanılmaz; cümle yeniden kurulur. Teknik terimlerin içindeki kısa
çizgi (`user-secrets`, `least-privilege`) korunur.

Her belge, bir kararın **neden** verildiğini de anlatır. Bu depodaki belgelerin değeri,
borsaların dokümantasyonunun anlatmadığı şeyleri anlatmalarıdır.

---

## Yapı

```
src/TRCrypto.BtcTurk/          BtcTurk adaptörü
src/TRCrypto.BinanceTR/        Binance TR adaptörü
tests/*.UnitTests/             Ağa çıkmayan testler
tests/*.IntegrationTests/      Canlı API testleri
examples/TRCrypto.Examples.Console/
docs/DURUM.md                  Nerede kaldık, ne kaldı
docs/credentials/              Borsa başına API anahtarı rehberi
docs/vendor/                   Doğrulanmış uç envanteri
docs/spec/                     Teknik spesifikasyon ve D-1..D-44 doğrulama ekleri
docs/index.html                Üretilen dokümantasyon sitesi
tools/site/                    Siteyi üreten betikler
```

Her adaptör aynı iskeleti izler: `Clients/SpotApi/` altında REST ve socket istemcileri,
`Objects/Models/` altında modeller, `Objects/Internal/` altında zarf ve serileştirme,
`Enums/`, `Interfaces/`, ve kök dizinde `*Exchange`, `*Environment`, `*Errors`,
`*AuthenticationProvider`, `*ServiceCollectionExtensions`.

---

## Doğrulama

Bir işi tamamlandı saymadan önce şunlar çalıştırılır ve **çıktısı raporlanır**:

```bash
dotnet build -c Release          # 0 uyari, 5 hedef platform
dotnet test  -c Release          # tum testler yesil
node tools/site/build.mjs        # dokuman degistiyse site yeniden uretilir
node tools/site/check.mjs        # markdown isleyicisi dogrulanir
```

Uyarılar hata sayılır (`TreatWarningsAsErrors`). Beş hedef platform vardır:
`net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `netstandard2.1`.

Doküman değiştirip siteyi yeniden üretmezseniz CI derlemeyi durdurur.

---

## Borsalar arasındaki farklar

Bu farklar ADR-003'ün ("her borsa bağımsız adaptör") gerekçesidir. Tek bir ortak
uygulamaya indirgenemezler.

| | BtcTurk | Binance TR |
|---|---|---|
| Sembol biçimi | `BTCTRY` | REST'te `BTC_TRY`, abonelikte `btctry`, akışta `BTCTRY` |
| Başarı göstergesi | `success` alanı | `code == 0` |
| Hata mesajı alanı | `message` | `msg`, emir oluşturmada `message` |
| Secret | Base64, çözülerek kullanılır | Ham metin, çözülmez |
| İmza kodlaması | Base64 | Onaltılık |
| İmza başlıkları | `X-PCK`, `X-Stamp`, `X-Signature` | `X-MBX-APIKEY` ve `signature` parametresi |
| Alan eşleşmesi | Harf büyüklüğüne **duyarsız** olmalı | Harf büyüklüğüne **duyarlı** olmalı |
| Emir durumu | Metin | Sayı |
| REST ticker | Var | Yok, yalnızca WebSocket |
| Zaman toleransı | Geniş | `recvWindow` varsayılan 5000 ms |

Paribu bu tabloya üçüncü bir biçim ekler: `btc_tl`. Türk lirasını `TRY` değil **`TL`**
olarak yazan tek borsadır, sembolleri küçük harflidir ve imza yükü zaman damgası, sorgu
dizesi ve gövdenin birleşimidir. Ayrıntı: `docs/vendor/paribu-capabilities.md`.

Son satır pratikte önemlidir: birkaç saniyelik saat kayması BtcTurk'te sorun çıkarmazken
Binance TR'de tüm imzalı istekleri reddettirir. `ServerTimeIntegrationTests` bunu her
çalıştırmada ölçer.

---

## Şu an nerede

**BtcTurk** REST, WebSocket, kimlik doğrulama ve shared yüzey tamamlandı; okuma uçları
gerçek bir hesaba karşı doğrulandı.

**Binance TR** public piyasa verisi, WebSocket, imzalama ve private REST yüzeyi
tamamlandı. Private uçlar canlı bir hesapla henüz denenmedi; anahtar geldiğinde
`AuthenticationProbeTests` şemayı doğrulayacaktır.

**Paribu** için uç envanteri çıkarıldı ve `docs/vendor/paribu-capabilities.md` dosyasına
yazıldı; kod henüz yazılmadı. Borsanın resmi bir API'si vardır, public ticker ve emir
defteri anahtarsız çalışır ve canlı doğrulanmıştır.

**Bitexen** planlandı, başlanmadı.

NuGet'e henüz yayınlanmadı; ilk sürüm `0.1.0-preview` olarak planlanıyor.
