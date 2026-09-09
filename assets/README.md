# Marka Varlıkları

TRCrypto.Net'in görsel kimliği dokümantasyon sitesinin paletini sürdürür: İznik çinisinden
gelen kobalt ve turkuaz, beyaz sır üzerine.

## İşaret

Sekiz kollu yıldız (rub el hizb), aynı boyutta iki karenin biri 45 derece döndürülerek üst
üste konmasıyla oluşur. İznik çinisinin temel motifidir ve kütüphanenin yaptığı işi de
anlatır: tek bir merkez, sekiz yöne açılan aynı yapı.

Ortadaki turkuaz kare, sitede "iki borsa tek kod" fikrini işaretleyen renktir.

| Dosya | Ne zaman kullanılır |
|---|---|
| `logo.svg` | Varsayılan. Kobalt yıldız, turkuaz merkez |
| `logo-mono.svg` | Tek renk gerektiğinde. Rengi `currentColor` ile devralır, merkez delik olarak açılır |
| `logo-512.png` | Raster gereken yerler; sosyal medya profili, paket simgesi |
| `wordmark.svg` | İşaret ve ismin yatay kilidi |
| `social-card.svg` · `social-card.png` | GitHub sosyal önizleme, 1280x640 |
| `linkedin-card.svg` · `linkedin-card.png` | LinkedIn paylaşımı, 1200x1200 |

`docs/favicon.svg` aynı işaretin kopyasıdır ve dokümantasyon sitesi tarafından kullanılır.

## Renkler

| Ad | Değer | Nerede |
|---|---|---|
| Kobalt | `#1F3E8C` | Tek vurgu rengi; yıldız, bağlantılar, başlıklardaki `TR` |
| Turkuaz | `#10707B` | Yalnızca borsadan bağımsız yüzeyi işaretlemek için |
| Mürekkep | `#141A29` | Gövde metni |
| Sır beyazı | `#FCFCFA` | Zemin |

Bolu kırmızısı (`#A9361F`) palette vardır ancak yalnızca uyarı anlamında kullanılır,
marka rengi değildir.

## Tipografi

Başlıklar **Archivo**, gövde metni **Source Serif 4**, veri ve kod **IBM Plex Mono**.
Gövdenin serif olması bilinçlidir: bu bir uygulama arayüzü değil, bir başvuru belgesidir.

## PNG üretimi

SVG kaynaktır; PNG dosyaları ondan türetilir. Yeniden üretmek için Archivo ve IBM Plex
Mono yüzlerinin bir dizinde bulunması gerekir, aksi halde metin sistem yüzüne düşer:

```bash
npx @resvg/resvg-js-cli assets/social-card.svg assets/social-card.png \
  --font-dir <yuzlerin-oldugu-dizin> --no-system-font

npx @resvg/resvg-js-cli assets/linkedin-card.svg assets/linkedin-card.png \
  --font-dir <yuzlerin-oldugu-dizin> --no-system-font

npx @resvg/resvg-js-cli assets/logo.svg assets/logo-512.png --fit-width 512
```

`--no-system-font` bilinçlidir: sistemde Archivo yoksa metin sessizce başka bir yüze
düşer ve kart farklı bir makinede farklı görünür. Bayrak, yüz bulunamadığında sessiz
kalmak yerine sorunu görünür kılar.

Kartlardaki metin `<text>` öğesidir, yola çevrilmemiştir; bu yüzden düzeltmesi kolaydır
ama başlangıçtaki boşluklar yutulur. Kod satırlarında girinti boşlukla değil `x` konumuyla
verilir.

## Sosyal önizlemeyi bağlama

`social-card.png` GitHub'a kendiliğinden bağlanmaz. Depo ayarları, General bölümü, Social
preview alanından yüklenmesi gerekir.

## Kullanım sınırı

Bu işaret TRCrypto.Net projesine aittir. Proje resmi değildir ve adı geçen hiçbir
platformla bağlantısı yoktur; işaret de bu platformların hiçbirini temsil etmez.
