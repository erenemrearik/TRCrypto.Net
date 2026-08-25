# Güvenlik Politikası

TRCrypto.Net, gerçek para hareketi yaratabilen borsa API'lerine erişim sağlar.
Güvenliği ciddiye alıyoruz.

## Güvenlik açığı bildirimi

**Güvenlik açıklarını herkese açık issue olarak bildirmeyin.**

Bunun yerine GitHub'ın özel bildirim kanalını kullanın:
[Security → Report a vulnerability](https://github.com/erenemrearik/TRCrypto.Net/security/advisories/new)

Bildiriminizde şunlar yardımcı olur:

- Etkilenen paket ve sürüm
- Açığın türü ve olası etkisi
- Yeniden üretme adımları
- Varsa bir düzeltme önerisi

Bildiriminize **72 saat içinde** dönüş yapmayı hedefliyoruz. Düzeltme yayınlanana kadar
açığı gizli tutmanızı rica ederiz.

## Özellikle ilgilendiğimiz konular

| Konu | Neden kritik |
|---|---|
| Kimlik bilgisinin loglara, istisnalara veya telemetriye sızması | Sızan anahtar varlık kaybettirebilir |
| İmzalama hataları | Yanlış imza, isteklerin reddedilmesine ya da yanlış hesaba işlem yapılmasına yol açabilir |
| Emir işlemlerinde yeniden deneme / yinelenme | Aynı emrin iki kez gönderilmesi doğrudan finansal zarardır |
| Zaman damgası / nonce üretimi | Tekrar saldırılarına (replay) karşı koruma |
| Bağımlılık zinciri | Kötü niyetli veya ele geçirilmiş paket |
| TLS doğrulamasının atlanabildiği bir yol | Ortadaki adam saldırısı |

## Kapsam dışı

- Borsaların kendi API'lerindeki açıklar — doğrudan ilgili borsaya bildirin
- Kütüphaneyi kullanan uygulamanın kendi yapılandırma hataları
- Anahtarını yanlışlıkla paylaşan kullanıcı senaryoları
  (yine de bkz. [docs/credentials/README.md](docs/credentials/README.md))

## Kullanıcılar için güvenlik notları

Bu kütüphane hiçbir özelliği için **çekim (withdrawal) iznine ihtiyaç duymaz.**
API anahtarınızda bu izni açmayın.

- En az yetki ilkesi ve IP allow-list: [docs/credentials/README.md](docs/credentials/README.md)
- Anahtarlar `dotnet user-secrets` veya bir secret store'da saklanmalıdır
- Kütüphane, kimlik bilgilerinin ham değerini loglara yazmaz; bu davranış birim
  testleriyle doğrulanır

## Desteklenen sürümler

Proje `0.x` aşamasındadır. Güvenlik düzeltmeleri yalnızca en son sürüme uygulanır.

| Sürüm | Destek |
|---|---|
| 0.x (en son) | ✅ |
| Daha eski 0.x | ❌ |
