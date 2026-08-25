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

Projenin şu anki durumu ve sonraki adımlar: [docs/DURUM.md](docs/DURUM.md)

---

## 🔐 En önemli kural: secret'lar

**Gerçek API anahtarı asla commit edilmez.** Bu finansal bir kütüphanedir; sızan bir
anahtar gerçek para kaybettirebilir.

- Kimlik bilgileri `dotnet user-secrets` ile saklanır — ayrıntı:
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

1. **Vendor freeze** — ucu `docs/vendor/<borsa>-capabilities.md` dosyasına kaydedin:
   method, path, parametreler, örnek yanıt, kaynak link, erişim tarihi.
   Doğrulayamıyorsanız "dondurulmamış" olarak işaretleyin ve **yazmayın**.

2. **Fixture** — `tests/.../Fixtures/` altına gerçek bir yanıt koyun.
   **Canlı public API'den alınmış yanıt tercih edilir**; resmi örnekler eksik olabiliyor.
   (Bunun somut örnekleri için `docs/spec/` ekindeki D-7…D-12 bulgularına bakın.)

3. **Test önce** — testi yazın, **başarısız olduğunu görün**, sonra kodu yazın.

4. **Contract testi** — `tests/.../Endpoints/` altına şu formatta bir dosya ekleyin:
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

---

## Hata yönetimi

API hataları **istisna olarak fırlatılmaz**, sonuç nesnesi olarak döner.

Bazı borsalar iş mantığı hatalarını HTTP 200 içinde döndürür (BtcTurk'te
`"success": false`). Bu durum `MessageHandler` katmanında yakalanıp başarısız sonuca
çevrilmelidir — aksi halde çağıran taraf sessizce boş veri işler.

Bilinmeyen hata kodları **yutulmaz**; ham kod ve mesaj çağırana taşınır.

---

## Emir işlemleri (dikkat)

Emir verme/iptal gerçek para hareketi yaratır.

- Otomatik yeniden deneme **yapılmaz** (ADR-009) — emir çift işlenebilir
- Örneklerde piyasa emri değil, **limit emri** kullanılır
- Çekim (withdrawal) desteği MVP kapsamı dışındadır ve rehberlerde önerilmez

---

## Pull request

PR'ınızdan önce:

```bash
dotnet build -c Release   # 0 warning
dotnet test  -c Release   # hepsi yesil
```

- Bir PR bir konuya odaklansın; ilgisiz düzeltmeleri ayırın
- Commit mesajı **ne** değil **neden** anlatsın
- Yeni uç eklediyseniz README'deki tabloyu güncelleyin
- Vendor dosyasına erişim tarihini yazın

Yeni bir borsa adaptörü gibi büyük bir katkı planlıyorsanız, önce bir issue açıp
konuşalım — böylece boşa emek harcanmaz.

---

## Sorular

Issue açabilir ya da mevcut issue'lara yorum yazabilirsiniz. Türkçe veya İngilizce,
ikisi de olur.
