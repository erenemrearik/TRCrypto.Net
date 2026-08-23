# BtcTurk — API Anahtarı Alma ve Bağlama Rehberi

> **Kaynak:** [docs.btcturk.com/docs/api-access-permissions](https://docs.btcturk.com/docs/api-access-permissions) ·
> [authentication-v1](https://docs.btcturk.com/docs/authentication/authentication-v1)
> **Erişim tarihi:** 24 Ağustos 2026
>
> Genel güvenlik kuralları için önce [README.md](README.md) dosyasını okuyun.

## 0. Önce: anahtara ihtiyacınız var mı?

**Muhtemelen hayır.** TRCrypto.BtcTurk'ün bu sürümü yalnızca herkese açık piyasa verisi sunar
ve bunun için anahtar gerekmez:

```csharp
var client = new BtcTurkRestClient();   // kimlik bilgisi yok
var info = await client.SpotApi.ExchangeData.GetExchangeInfoAsync();
```

Anahtar yalnızca bakiye görüntüleme, emir verme/iptal ve işlem geçmişi için gerekir.
Bu özellikler henüz eklenmedi.

---

## 1. Anahtar nasıl oluşturulur

1. [btcturk.com](https://www.btcturk.com) hesabınıza giriş yapın
2. **Hesap → API Erişimi** sayfasına gidin
3. Formda:
   - Vereceğiniz **izinleri** seçin (bkz. bölüm 2)
   - İstemcinizin çalışacağı **IP adresini** girin (bkz. bölüm 3)
   - WebSocket erişimi gerekiyorsa ilgili seçeneği işaretleyin
4. Formu gönderin — size bir **public key** ve bir **private key (secret)** verilir

> **Secret yalnızca bir kez gösterilir.** Kaybederseniz yeni anahtar oluşturmanız gerekir.
> Kopyaladıktan sonra doğrudan bölüm 4'teki yönteme kaydedin.

---

## 2. İzinler ve en az yetki eşleşmesi

BtcTurk dört izin tanımlar:

| İzin (panelde) | Kapsam |
|---|---|
| **Toplam Varlık** (Total Funds) | Bakiye uçları |
| **Al-Sat** (Trade) | Emir oluşturma ve iptal |
| **Hesap** (Account) | İşlem geçmişi |
| **WebSocket** | Socket mesajları |

TRCrypto özelliklerine göre gerekli minimum:

| Ne yapacaksınız | Gereken izinler |
|---|---|
| Fiyat/parite/emir defteri okuma | **Hiçbiri — anahtar bile gerekmez** |
| Bakiye görüntüleme | Toplam Varlık |
| Emir geçmişi görüntüleme | Toplam Varlık + Hesap |
| Emir verme / iptal | + Al-Sat |
| Gerçek zamanlı özel akışlar | + WebSocket |

> ### ⚠️ Çekim (Para Çekme) iznini AÇMAYIN
>
> TRCrypto hiçbir özelliği için çekim iznine ihtiyaç duymaz. Çekim izinli bir anahtar
> sızarsa varlıklarınız geri alınamaz şekilde transfer edilebilir.

**Emir testleri için:** ana hesabınızı kullanmayın. Ayrı, düşük bakiyeli bir hesap açın.
Test emirlerini piyasadan çok uzak bir limit fiyatıyla verip hemen iptal edin.

---

## 3. IP allow-list

Anahtar oluştururken IP girmek formun bir parçasıdır. Bu, anahtar sızsa bile başka bir
makineden kullanılmasını engeller — **en etkili tek korumadır.**

| Ortam | Ne yazmalı |
|---|---|
| Yerel geliştirme | Genel IP'niz (`curl ifconfig.me`) |
| CI (GitHub Actions) | Runner IP'leri sabit değildir; kendi runner'ınızı kullanın veya CI'da private test çalıştırmayın |
| Üretim | Sunucunuzun sabit çıkış IP'si |

Dinamik IP'niz varsa IP değiştiğinde anahtarı güncellemeniz gerekir.

---

## 4. Anahtarı uygulamaya bağlama

### Saklama (önerilen: user-secrets)

```bash
cd <projeniz>
dotnet user-secrets init
dotnet user-secrets set "BtcTurk:ApiKey"    "public-key-degeriniz"
dotnet user-secrets set "BtcTurk:ApiSecret" "secret-degeriniz"
```

### Kullanım

```csharp
using TRCrypto.BtcTurk;
using TRCrypto.BtcTurk.Clients;

var client = new BtcTurkRestClient(options =>
{
    options.ApiCredentials = new BtcTurkCredentials(
        configuration["BtcTurk:ApiKey"]!,
        configuration["BtcTurk:ApiSecret"]!);
});
```

### Bağımlılık enjeksiyonu ile

```csharp
builder.Services.AddTRCryptoBtcTurk(options =>
{
    options.ApiCredentials = new BtcTurkCredentials(
        builder.Configuration["BtcTurk:ApiKey"]!,
        builder.Configuration["BtcTurk:ApiSecret"]!);
});
```

---

## 5. Secret formatı — en sık yapılan hata

BtcTurk'ün secret'ı **Base64 kodludur.** İmzalama sırasında ham metin olarak değil,
**çözülmüş baytlar** HMAC anahtarı olarak kullanılır.

TRCrypto bu çözümü sizin için yapar — secret'ı **olduğu gibi**, panelde gördüğünüz haliyle
verin. Kendiniz decode etmeye çalışmayın.

İmza zinciri (referans):

```
1. mesaj    = apiKey + stamp          (stamp: UTC milisaniye)
2. anahtar  = Base64Decode(secret)
3. digest   = HMAC-SHA256(anahtar, mesaj)
4. X-Signature = Base64Encode(digest)
```

2. adım atlanırsa imza **sessizce yanlış** olur — hata mesajı "geçersiz imza" der ve
nedeni belli olmaz.

Gönderilen başlıklar: `X-PCK` (public key), `X-Stamp` (nonce), `X-Signature`.

---

## 6. Sistem saatiniz doğru olmalı

`X-Stamp` UTC milisaniye cinsindendir ve sunucu saatiyle uyumlu olmalıdır. Saatiniz
kaymışsa istekleriniz reddedilir.

Kontrol:

```csharp
var serverTime = await client.SpotApi.ExchangeData.GetServerTimeAsync();
Console.WriteLine($"Fark: {(DateTime.UtcNow - serverTime.Data).TotalSeconds:F1} sn");
```

Birkaç saniyenin üzerinde bir fark varsa Windows'ta
**Ayarlar → Saat ve dil → Tarih ve saat → Şimdi eşitle** ile düzeltin.

---

## 7. Sorun giderme

| Belirti | Neden / çözüm |
|---|---|
| İmza geçersiz | Secret ile public key yer değiştirmiş olabilir; ya da secret kopyalanırken boşluk/satır sonu eklenmiş |
| Yetkisiz, anahtar doğru | Bulunduğunuz IP allow-list'te değil |
| Zaman damgası hatası | Sistem saati kaymış (bölüm 6) |
| İzin hatası | İlgili izin anahtarda işaretli değil; anahtarı yeniden oluşturun |
| Bakiye boş | Anahtar farklı bir hesaba ait |
| HTTP 503 (emir defteri) | Borsa tarafında geçici gecikme; yeniden deneyin |

---

## 8. Anahtarım sızdıysa

1. **Derhal** Hesap → API Erişimi sayfasından anahtarı silin — resmi talimat budur
2. Hesap hareketlerinizi kontrol edin
3. Yeni anahtar oluşturun ve IP allow-list'i doldurun
