# CoinTR API Anahtarı Alma ve Bağlama Rehberi

> **Kaynak:** [cointr-ex.github.io/openapis](https://cointr-ex.github.io/openapis/) ve
> [cointr.com/api-doc/common/intro](https://www.cointr.com/api-doc/common/intro)
> **Erişim tarihi:** 7 Eylül 2026
>
> Genel güvenlik kuralları için önce [README.md](README.md) dosyasını okuyun.

## 0. Önce: anahtara ihtiyacınız var mı?

**Bu sürümde anahtar hiçbir işe yaramaz.** `TRCrypto.CoinTR` şu an yalnızca kimlik
doğrulama gerektirmeyen uçları ve akışları sunuyor. İmzalama sağlayıcısı yazıldı, ancak
private uçlar canlı bir hesapla denenmediği için **bilinçli olarak yayımlanmadı.**
Doğrulanmamış bir imzalama, isteklerin nedeni belirsiz şekilde reddedilmesine yol açar.

Fiyat, emir defteri, işlem ve mum verisinin tamamı anahtarsız çalışır:

```csharp
var rest   = new CoinTRRestClient();     // kimlik bilgisi yok
var socket = new CoinTRSocketClient();   // kimlik bilgisi yok

var ticker = await rest.SpotApi.ExchangeData.GetTickersAsync("BTCTRY");
await socket.SpotApi.SubscribeToTickerUpdatesAsync(
    "BTCTRY", u => Console.WriteLine(u.Data.LastPrice));
```

Anahtar yalnızca bakiye, emir ve hesap uçları eklendiğinde gerekli olacak.

---

## 1. Anahtar nasıl oluşturulur

1. [cointr.com](https://www.cointr.com) hesabınıza giriş yapın
2. Hesap menüsünden **API Yönetimi** sayfasına gidin
3. Anahtara bir isim verin, izinleri seçin ve güvenlik doğrulamasını tamamlayın
4. Bir **parola** (passphrase) belirleyin
5. Oluşturulan anahtar, secret ve parolayı kaydedin

> [!IMPORTANT]
> **Parola sonradan görüntülenemez.** Diğer üç borsadan farklı olarak burada kimlik
> bilgisi üç parçalıdır ve parola her istekte gönderilir. Kaybederseniz anahtarı yeniden
> oluşturmanız gerekir.

> **Secret de yalnızca bir kez gösterilir.** Kopyaladıktan sonra doğrudan bölüm 4'teki
> yönteme kaydedin.

---

## 2. İzinler ve en az yetki eşleşmesi

| Amaç | Açılacak izin | Kapalı kalacak |
|---|---|---|
| Piyasa verisi okuma | **hiçbiri**, anahtar bile gerekmez | hepsi |
| Bakiye ve hesap okuma | okuma | alım satım, **çekim** |
| Emir verme ve iptal | alım satım | **çekim** |

> [!WARNING]
> **Çekim (withdrawal) iznini açmayın.** TRCrypto çekim işlemi yapmaz; bu izne hiçbir
> özellik için ihtiyaç duyulmaz. Açıldığında anahtarınız sızarsa varlıklarınız doğrudan
> risk altındadır.

Emir testleri yapacaksanız bunları **ayrı ve düşük bakiyeli** bir hesapta yapın.

---

## 3. IP kısıtlaması

Anahtar oluşturma ekranında IP listesi doldurulabilir. Doldurun.

| Ortam | Ne yazılır |
|---|---|
| Yerel geliştirme | Kendi genel IP'niz (`curl ifconfig.me`) |
| Sunucu veya VPS | Sunucunun sabit çıkış IP'si |
| GitHub Actions | Sabit IP yok; CI'da imzalı istek çalıştırmayın |

Ev bağlantınızın IP'si değişebilir; beklenmedik bir yetki hatasında ilk bakılacak yer
burasıdır.

---

## 4. Anahtarı uygulamaya bağlama

### Saklama (önerilen: user-secrets)

Değerler repo dizininin **tamamen dışında**, `%APPDATA%\Microsoft\UserSecrets\` altında
durur:

```bash
cd tests/TRCrypto.CoinTR.IntegrationTests
dotnet user-secrets set "CoinTR:ApiKey"     "..."
dotnet user-secrets set "CoinTR:ApiSecret"  "..."
dotnet user-secrets set "CoinTR:Passphrase" "..."
```

Anahtarı **asla** kaynak koda, `appsettings.json` dosyasına veya bir ortam dosyasına
yazmayın.

### Bağımlılık enjeksiyonu ile

```csharp
// Tek çağrı hem REST hem WebSocket istemcisini kaydeder; kimlik bilgisi ikisine de uygulanır.
builder.Services.AddTRCryptoCoinTR(options =>
{
    options.ApiCredentials = new CoinTRCredentials(
        builder.Configuration["CoinTR:ApiKey"]!,
        builder.Configuration["CoinTR:ApiSecret"]!,
        builder.Configuration["CoinTR:Passphrase"]!);
});
```

---

## 5. İmzalama şemasının diğer borsalardan farkı

| | BtcTurk | Binance TR | CoinTR |
|---|---|---|---|
| Kimlik bilgisi | anahtar + secret | anahtar + secret | anahtar + secret + **parola** |
| İmzalanan değer | anahtar + zaman damgası | sorgu dizesi | zaman damgası + metot + **yol** + sorgu + gövde |
| Secret | Base64, çözülür | ham metin | **ham metin**, çözülmez |
| İmza kodlaması | Base64 | onaltılık | **Base64** |
| Başlıklar | `X-PCK` · `X-Stamp` · `X-Signature` | `X-MBX-APIKEY` | `ACCESS-KEY` · `ACCESS-PASSPHRASE` · `ACCESS-SIGN` · `ACCESS-TIMESTAMP` |

Dördü de HMAC-SHA256 kullanır ve hiçbirinde imzalanan değer aynı değildir. **Yolu imzaya
katan tek borsa CoinTR'dir**; yol atlandığında imza sessizce geçersiz olur.

> [!WARNING]
> Resmi dokümantasyondaki imzalama tarifi çalışmıyor. Doküman iki aşamalı ve zaman
> dilimine bağlı bir şema anlatıyor; o şema isteği geçirmiyor. Kütüphanedeki şema çalışan
> bir entegrasyondan doğrulandı. Ayrıntı:
> [../vendor/cointr-capabilities.md](../vendor/cointr-capabilities.md)

TRCrypto bütün dönüşümleri sizin için yapar. Secret'ı **panelde gördüğünüz haliyle**
verin.

---

## 6. Sistem saatiniz doğru olmalı

İmzalı isteklerde zaman damgası (milisaniye) zorunludur ve imzanın ilk parçasıdır.

```csharp
var serverTime = await rest.SpotApi.ExchangeData.GetServerTimeAsync();
Console.WriteLine($"Fark: {(DateTime.UtcNow - serverTime.Data).TotalMilliseconds:N0} ms");
```

Windows'ta düzeltme: **Ayarlar → Saat ve dil → Tarih ve saat → Şimdi eşitle**.

---

## 7. Sorun giderme

| Belirti | Neden ve çözüm |
|---|---|
| Yanıt `code` alanı `"00000"` değil | İşlem başarısız. Kod bir **metindir**; sayı olarak karşılaştırmayın |
| İmza reddedildi | En sık üç neden: parola gönderilmemiş, yol imzaya katılmamış, ya da secret anahtarla aynı çiftten değil |
| Parola hatası | Parola sonradan görüntülenemez; hatırlamıyorsanız anahtarı yeniden oluşturun |
| Abonelik onaylandı ama veri gelmiyor | Emir defteri kanalında kademe sayısı kanal adının parçasıdır; yalnızca 1, 5 ve 15 tanımlıdır |
| Bağlantı düşüyor | Sunucu düz metin `pong` çerçevesi gönderir; JSON değildir ve ayrıştırılmaya çalışılırsa hata üretir. Kütüphane bunu eler |
| Değişim yüzdesi yüz kat küçük | `change24h` kesirdir. `ChangePercentage` alanını kullanın |

---

## 8. Anahtarım sızdıysa

1. **Derhal** API Yönetimi sayfasından anahtarı silin
2. Hesap hareketlerinizi ve açık emirlerinizi kontrol edin
3. Yeni anahtar oluşturun, izinleri en aza indirin ve IP kısıtlamasını doldurun

---

## Ayrıca

- Borsanın uç envanteri: [../vendor/cointr-capabilities.md](../vendor/cointr-capabilities.md)
- Paket kullanımı: [TRCrypto.CoinTR README](https://github.com/erenemrearik/TRCrypto.Net/blob/main/src/TRCrypto.CoinTR/README.md)
