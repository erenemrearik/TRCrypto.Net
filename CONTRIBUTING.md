# Katkı Rehberi

TRCrypto.Net'e katkıda bulunmak istediğiniz için teşekkürler. Bu proje Türkiye'deki
kripto borsalarını .NET geliştiricileri için erişilebilir kılmayı amaçlıyor.

## Başlamadan önce

```bash
git clone https://github.com/erenemrearik/TRCrypto.Net.git
cd TRCrypto.Net

# Secret koruma kancasini etkinlestirin (bir kez)
git config core.hooksPath .githooks

dotnet build -c Release
dotnet test  -c Release
```

Gerekli: **.NET 10 SDK** (paket `net8.0`'a kadar geriye dönük hedefler).

### İki tür test var

| Tür | Ne yapar | Nerede çalışır |
|---|---|---|
| **Birim** | Ağa çıkmaz; sabit yanıtlarla çözümleme, istek kurulumu ve doğrulama | Her PR'da |
| **Canlı** (`*.IntegrationTests`) | Borsanın gerçek API'sine çıkar | Haftalık zamanlanmış iş |

Canlı testler PR akışında çalıştırılmaz: her PR için borsanın istek limitini yakmamak ve
derlemeyi borsanın erişilebilirliğine bağımlı kılmamak için. Amaçları regresyon yakalamak
değil, **borsanın değiştiğini** fark etmektir.

Kimlik bilgisi isteyen canlı testler, anahtar tanımlı değilse **atlanır.** Anahtarı
olmayan bir katkıcı da her şeyi çalıştırabilir.

```bash
dotnet test -c Release --filter "FullyQualifiedName!~IntegrationTests"   # yalnizca birim
dotnet test -c Release --filter "FullyQualifiedName~IntegrationTests"    # yalnizca canli
```

### Doküman değiştirdiyseniz siteyi yeniden üretin

Dokümanlar `.md` dosyalarında yaşar; [dokümantasyon sitesi](https://erenemrearik.github.io/TRCrypto.Net/) onlardan üretilir
ve `docs/index.html` olarak depoda tutulur. Bir doküman değiştirip siteyi yeniden
üretmezseniz ikisi ayrışır. CI bunu yakalar ve derlemeyi durdurur.

```bash
node tools/site/build.mjs           # siteyi yeniden uret
node tools/site/check.mjs           # markdown isleyicisini dogrula
node tools/audit/docs-vs-code.mjs   # dokumanlar kodla tutarli mi
```

Bağımlılık yoktur; yalnızca Node 18+ gerekir.

Üç şey sizi şaşırtabilir, ikisi de bilinçli:

- **Test eklemek siteyi de değiştirir.** Test sayısı sitenin giriş sayfasında görünür ve
  elle yazılmaz, depodan türetilir. Test ekledikten sonra siteyi yeniden üretin.
- **Yeni bir `.md` dosyası siteye eklenmek zorundadır.** Denetim aracı depodaki tüm
  markdown dosyalarını tarar. Dosyayı `tools/site/build.mjs` içindeki gezinti listesine
  ekleyin ya da siteye girmemesi gerekiyorsa `tools/audit/docs-vs-code.mjs` içindeki
  dışarıda bırakma listesine gerekçesiyle yazın.
- **README'lerdeki sürüm değişiklik günlüğüyle aynı olmalıdır.** Paket README'leri
  NuGet'e paketin içinde gider; eski sürümü gösteren bir README denetimi durdurur.

Projenin şu anki durumu ve sonraki adımlar: [docs/DURUM.md](docs/DURUM.md)

---

## 🔐 En önemli kural: secret'lar

**Gerçek API anahtarı asla commit edilmez.** Bu finansal bir kütüphanedir; sızan bir
anahtar gerçek para kaybettirebilir.

- Kimlik bilgileri `dotnet user-secrets` ile saklanır. Ayrıntı:
  [docs/credentials/README.md](docs/credentials/README.md)
- `.gitignore` yaygın secret dosyalarını bloklar, ama tek savunma o değildir
- Pre-commit kancası `gitleaks` ile hazırlanan değişiklikleri tarar
- Testlerde gerçekçi görünümlü **sahte** değerler kullanılır ve `FAKE` ibaresi taşır

Yanlışlıkla bir anahtar commit ettiyseniz: **önce borsadan anahtarı silin**, sonra bize
haber verin. Geçmişi temizlemek ikincil önceliktir; anahtarı iptal etmek birincildir.

---

## Endpoint eklerken

Bu projenin en katı kuralı şudur:

> ### Endpoint uydurulmaz.
>
> Bir uç yalnızca **resmi dokümantasyondan** doğrulandıysa yazılır. Üçüncü taraf
> wrapper'lar (ccxt, arşivlenmiş kütüphaneler) yalnızca keşif içindir, kontrat kaynağı
> değildir.

Sıra:

1. **Vendor freeze.** Ucu `docs/vendor/<borsa>-capabilities.md` dosyasına kaydedin:
   method, path, parametreler, örnek yanıt, kaynak link, erişim tarihi.
   Doğrulayamıyorsanız "dondurulmamış" olarak işaretleyin ve **yazmayın**.

2. **Fixture.** `tests/.../Fixtures/` altına gerçek bir yanıt koyun.
   **Canlı public API'den alınmış yanıt tercih edilir**; resmi örnekler eksik olabiliyor.
   (Bunun somut örnekleri için `docs/spec/` ekindeki D-7…D-12 bulgularına bakın.)

3. **Test önce.** Testi yazın, **başarısız olduğunu görün**, sonra kodu yazın.

4. **Contract testi.** `tests/.../Endpoints/` altına şu formatta bir dosya ekleyin:
   ```
   GET
   /api/v2/...
   false
   {gercek yanit JSON}
   ```
   `RestRequestValidator` üretilen isteği ve model eşlemesini doğrular.

### Modelleme kuralları

| Kural | Neden |
|---|---|
| Fiyat/miktar için **`decimal`** | `double` hassasiyet kaybettirir |
| Enum'larda **`Unknown` üyesi tanımlanmaz** | Ekosistem konvansiyonu: bilinmeyen değer tanımsız enum değerine düşer, `Enum.IsDefined` ile tespit edilir |
| Sembol adı **ayrıştırılmaz** | Borsalar base/quote'u genelde ayrı alanda veriyor |
| Varlık türü **tahmin edilmez** | Borsa bildiriyorsa o kullanılır |
| Tüm async metotlarda `CancellationToken ct = default` | |
| Kütüphane kodunda `ConfigureAwait(false)` | Analyzer zorluyor (CA2007) |
| Tüm public üyelerde XML dokümantasyonu | |
| Yorumlar **neden**i anlatır, **ne**yi değil | Kod zaten ne yaptığını söylüyor |

### Geçersiz girdi

Ağa çıkmadan reddedilmelidir:

```csharp
if (limit is <= 0 or > MaxTradeLimit)
    throw new ArgumentOutOfRangeException(nameof(limit), limit, "...");
```

### Paylaşılan yüzeye yetenek eklerken

Bir ucu borsadan bağımsız yüzeye de açacaksanız, CryptoExchange.Net 13 ile üç yere
dokunmanız gerekir. Biri unutulursa derleme ya da testler durur, ama nedeni her zaman
açık değildir; bu yüzden burada yazılı.

| Nereye | Ne | Unutulursa |
|---|---|---|
| `*Shared.cs` | V1 arayüzünü (`IOrderBookRestClient` gibi) uygulayan asıl kod | Yetenek yok |
| `*SharedV2.cs` | V2 arayüzünün (`IGetOrderBookRest` gibi) V1'e delege eden karşılığı | `ValidateRequest` derlenmez |
| `RegisterCapabilities()` | Seçenek nesnesini yetenek listesine eklemek | `CapabilityAssert` kırılır |

Üç kural her durumda geçerlidir:

- **V2 kendi seçenek nesnesini oluşturmaz,** V1'inkini döndürür:
  `GetOrderBookOptions IGetOrderBook.GetOrderBookOptions => ((IOrderBookRestClient)this).GetOrderBookOptions;`
  Ayrı bir nesne doğrulama kurallarını ikiye böler. Test aynı nesne olduğunu denetler.
- **Seçenekler kurucuda kaydedilir.** 13'te kaydedilmeyen bir seçenek hiçbir işlem
  türünü desteklemiyor görünür ve her çağrı `TradingMode.Spot is not supported` ile
  reddedilir. Derleme ve yapısal testler bunu göstermez.
- **V1 ve V2 ayrı genel arayüzlerde kalır** (`SharedClient` ve `SharedApi`). Aynı adlı
  üyeler taşıdıkları için birleştirilmeleri kullanıcı kodunu derlenmez hale getirir.

Gerekçesi ve nasıl bulunduğu `docs/spec/` ekinde E.13'te.

---

## Hata yönetimi

API hataları **istisna olarak fırlatılmaz**, sonuç nesnesi olarak döner.

Bazı borsalar iş mantığı hatalarını HTTP 200 içinde döndürür (BtcTurk'te
`"success": false`). Bu durum `MessageHandler` katmanında yakalanıp başarısız sonuca
çevrilmelidir. Aksi halde çağıran taraf sessizce boş veri işler.

Bilinmeyen hata kodları **yutulmaz**; ham kod ve mesaj çağırana taşınır.

---

## Emir işlemleri (dikkat)

Emir verme/iptal gerçek para hareketi yaratır.

- Otomatik yeniden deneme **yapılmaz** (ADR-009), çünkü emir çift işlenebilir
- Örneklerde piyasa emri değil, **limit emri** kullanılır
- Çekim (withdrawal) desteği MVP kapsamı dışındadır ve rehberlerde önerilmez

---

## Pull request

PR'ınızdan önce:

```bash
dotnet build -c Release                              # 0 warning
dotnet test  -c Release                              # hepsi yesil
node tools/audit/docs-vs-code.mjs                    # dokumanlar kodla tutarli
dotnet run --project examples/TRCrypto.Examples.Console -c Release   # canli dogrulama
```

Sonuncusu canlı borsalara bağlanır ve en çok atlanan adımdır. Birim testleri yapıyı
denetler; bir isteğin gerçekten doğrulamadan geçip borsaya ulaştığını yalnızca canlı
çalıştırma gösterir. CryptoExchange.Net 13 göçünde derleme ve testlerin tamamı geçtiği
halde her paylaşılan çağrının reddedildiğini yalnızca bu adım yakaladı.

- Bir PR bir konuya odaklansın; ilgisiz düzeltmeleri ayırın
- Commit mesajı **ne** değil **neden** anlatsın
- Yeni uç eklediyseniz README'deki tabloyu güncelleyin
- Vendor dosyasına erişim tarihini yazın

Yeni bir borsa adaptörü gibi büyük bir katkı planlıyorsanız, önce bir issue açıp
konuşalım. Böylece boşa emek harcanmaz.

---

## Yayın

Yeni bir sürümü NuGet'e çıkarmak bakımcının işidir; adımlar ve sorun giderme
[docs/YAYIN.md](docs/YAYIN.md) içinde. Kısaca: değişiklik günlüğüne sürüm başlığı, CI
yeşil, bir etiket ve GitHub'da onay.

---

## Sorular

Issue açabilir ya da mevcut issue'lara yorum yazabilirsiniz. Türkçe veya İngilizce,
ikisi de olur.
