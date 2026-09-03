# Binance TR API Anahtarı Alma ve Bağlama Rehberi

> **Kaynak:** [binance.tr/apidocs](https://www.binance.tr/apidocs) · panel akışı Binance TR
> hesap arayüzünden doğrulanmıştır
> **Erişim tarihi:** 27 Ağustos 2026
>
> Genel güvenlik kuralları için önce [README.md](README.md) dosyasını okuyun.

## 0. Önce: anahtara ihtiyacınız var mı?

**Bu sürümde anahtar hiçbir işe yaramaz.** `TRCrypto.BinanceTR` şu an yalnızca kimlik
doğrulama gerektirmeyen uçları sunuyor; imzalama yazıldı ama **canlı doğrulanmadığı için
bilinçli olarak devre dışıydı.** Doğrulanmamış bir imzalama, isteklerin nedeni belirsiz
şekilde reddedilmesine yol açardı.

```csharp
var rest   = new BinanceTRRestClient();     // kimlik bilgisi yok
var socket = new BinanceTRSocketClient();   // kimlik bilgisi yok

var book = await rest.SpotApi.ExchangeData.GetOrderBookAsync("BTC_TRY", limit: 5);
await socket.SpotApi.SubscribeToTickerUpdatesAsync("BTC_TRY", u => Console.WriteLine(u.Data.LastPrice));
```

> [!IMPORTANT]
> **Ticker verisi için anahtar almanız gerekmez.** Borsa REST tarafında ticker'ı anahtarsız
> vermiyor, ama aynı veri **WebSocket üzerinden anahtarsız akıyor**. Yalnızca fiyat takibi
> yapacaksanız anahtar almanıza gerek yok.

Anahtar yalnızca bakiye, emir ve hesap uçları eklendiğinde gerekli olacak.

---

## 1. Anahtar nasıl oluşturulur

1. [binance.tr](https://www.binance.tr) hesabınıza giriş yapın
2. Sağ üstteki hesap simgesinden **API Yönetimi** sayfasına gidin
3. Anahtara bir **isim** verin ve güvenlik doğrulamasını tamamlayın
4. Oluşturma isteği borsanın onayına düşer
5. Onaydan sonra **İzinleri düzenle** ile izinleri ve IP kısıtlamasını ayarlayın

> **Secret yalnızca bir kez gösterilir.** Kopyaladıktan sonra doğrudan bölüm 4'teki
> yönteme kaydedin; kaybederseniz yeni anahtar oluşturmanız gerekir.

> [!NOTE]
> Anahtar oluşturabilmek için hesabınızın kimlik doğrulaması tamamlanmış olmalıdır.

---

## 2. İzinler ve en az yetki eşleşmesi

Resmi dokümantasyon uçları üç güvenlik seviyesine ayırır:

| Seviye | Ne gerekir |
|---|---|
| `NONE` | Hiçbir şey; herkese açık uçlar |
| `API_KEY` | Yalnızca `X-MBX-APIKEY` başlığı |
| `SIGNED` | Anahtar **ve** HMAC-SHA256 imzası |

Panel tarafında verilecek izinler için kural nettir:

| Amaç | Açılacak izin | Kapalı kalacak |
|---|---|---|
| Piyasa verisi okuma | **hiçbiri**, anahtar bile gerekmez | hepsi |
| Bakiye ve hesap okuma | okuma izni | alım satım, **çekim** |
| Emir verme / iptal | alım satım | **çekim** |

> [!WARNING]
> **Çekim (withdrawal) iznini açmayın.** TRCrypto çekim işlemi yapmaz; bu izne hiçbir
> özellik için ihtiyaç duyulmaz. Açıldığında anahtarınız sızarsa varlıklarınız
> doğrudan risk altındadır.

Emir testleri yapacaksanız bunları **ayrı ve düşük bakiyeli** bir hesapta yapın.

---

## 3. IP kısıtlaması

**İzinleri düzenle** ekranındaki **"Erişimi yalnızca güvenilir IP adresleriyle sınırla"**
seçeneğini işaretleyin ve istemcinizin çıkış IP'sini girin.

| Ortam | Ne yazılır |
|---|---|
| Yerel geliştirme | Kendi genel IP'niz (`curl ifconfig.me`) |
| Sunucu / VPS | Sunucunun sabit çıkış IP'si |
| GitHub Actions | Sabit IP yok; CI'da imzalı istek çalıştırmayın |

Ev bağlantınızın IP'si değişebilir; "yetkisiz" hatası alıp izinleri doğru sanıyorsanız
ilk bakılacak yer burasıdır.

---

## 4. Anahtarı uygulamaya bağlama

### Saklama (önerilen: user-secrets)

Değerler repo dizininin **tamamen dışında**, `%APPDATA%\Microsoft\UserSecrets\` altında durur:

```bash
cd tests/TRCrypto.BinanceTR.IntegrationTests
dotnet user-secrets set "BinanceTR:ApiKey"    "..."
dotnet user-secrets set "BinanceTR:ApiSecret" "..."
```

Anahtarı **asla** kaynak koda, `appsettings.json`'a veya bir ortam dosyasına yazmayın.

### Bağımlılık enjeksiyonu ile

```csharp
// Tek çağrı hem REST hem WebSocket istemcisini kaydeder; kimlik bilgisi ikisine de uygulanır.
builder.Services.AddTRCryptoBinanceTR(options =>
{
    options.ApiCredentials = new BinanceTRCredentials(
        builder.Configuration["BinanceTR:ApiKey"]!,
        builder.Configuration["BinanceTR:ApiSecret"]!);
});
```

---

## 5. Secret formatının BtcTurk'ten farkı

| | BtcTurk | Binance TR |
|---|---|---|
| Secret | **Base64**, imzalamadan önce çözülür | **Ham metin**, çözülmez |
| İmza kodlaması | Base64 | **Onaltılık (hex)** |
| Başlık | `X-PCK` · `X-Stamp` · `X-Signature` | `X-MBX-APIKEY` + `signature` parametresi |

İki borsanın imzalama şeması birbirine benzemez. Aynı yardımcı kodu ikisinde de
kullanmaya çalışmak sessizce yanlış imza üretir.

TRCrypto her iki dönüşümü de sizin için yapar. Secret'ı **panelde gördüğünüz haliyle**
verin.

---

## 6. Sistem saatiniz doğru olmalı

İmzalı isteklerde `timestamp` (milisaniye) zorunludur. Borsa isteği yalnızca
`recvWindow` içinde kabul eder: **varsayılan 5000 ms**, en fazla 60000 ms.

> [!WARNING]
> Bu pencere BtcTurk'e göre dardır. Birkaç saniyelik bir saat kayması BtcTurk'te sorun
> çıkarmazken Binance TR'de **tüm imzalı istekleri reddettirir**.

Kontrol:

```csharp
var serverTime = await rest.SpotApi.ExchangeData.GetServerTimeAsync();
Console.WriteLine($"Fark: {(DateTime.UtcNow - serverTime.Data).TotalMilliseconds:N0} ms");
```

Windows'ta düzeltme: **Ayarlar → Saat ve dil → Tarih ve saat → Şimdi eşitle**.
Etki alanına bağlı makinelerde saat etki alanı denetleyicisinden gelir; kayma
sürüyorsa çözüm sistem yöneticisindedir.

---

## 7. Sorun giderme

| Belirti | Neden / çözüm |
|---|---|
| `3701 Invalid API-key, IP, or permissions` | Üç ayrı nedeni tek mesajda toplar: anahtar yanlış, IP listede değil veya izin işaretli değil. **`/api/v3/*` yollarında ise anahtarınız değil, yol yanlıştır**; aşağıdaki nota bakın |
| Zaman damgası / `recvWindow` hatası | Sistem saati kaymış (bölüm 6) |
| İmza geçersiz | Secret'ı Base64 çözmeye çalışmış olabilirsiniz; Binance TR'de çözülmez (bölüm 5) |
| `1106 Incorrect Page number` | Emir defteri kademe sayısı desteklenmiyor; yalnızca 5, 10, 20, 50, 100, 500, 1000 kabul edilir. Mesaj sorunun limit olduğunu söylemez |
| İşlem/mum listesi boş ama `code: 0` | Borsa bu REST uçlarını boş döndürüyor; veri WebSocket üzerinden gelir |
| HTTP 429 / 418 | İstek limiti aşıldı; 418 IP yasağıdır ve süresi tekrarla uzar |

> [!NOTE]
> Global Binance'te herkese açık olan `/api/v3/*` uçları Binance TR'de **anahtar ister**.
> Global Binance için yazılmış örnek kod burada çalışmaz. Kullanılacak yollar
> `/open/v1/...` altındadır.

---

## 8. Anahtarım sızdıysa

1. **Derhal** API Yönetimi sayfasından anahtarı silin
2. Hesap hareketlerinizi ve açık emirlerinizi kontrol edin
3. Yeni anahtar oluşturun, izinleri en aza indirin ve IP kısıtlamasını doldurun

---

## Ayrıca

- Borsanın uç envanteri: [../vendor/binance-tr-capabilities.md](../vendor/binance-tr-capabilities.md)
- WebSocket protokolü: [../vendor/binance-tr-websocket.md](../vendor/binance-tr-websocket.md)
