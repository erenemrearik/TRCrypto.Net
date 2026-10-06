# Yayın Rehberi

Bu belge yeni bir sürümü NuGet'e çıkarmanın adımlarını anlatır. Yayın hattı tek seferlik
kuruldu; her sürümde yapılacak iş, bir etiket atmak ve GitHub'da onay vermektir.

Komutlar `cmd.exe` içinde çalışır.

---

## Bir kez kuruldu, tekrar gerekmez

Yayın **Trusted Publishing** ile yapılır. Depoda saklanan bir API anahtarı yoktur: GitHub
kısa ömürlü bir kimlik belgesi üretir, NuGet onu kayıtlı politikayla doğrular ve bir saat
geçerli, tek kullanımlık bir anahtar döndürür.

İki yerde yapılandırma vardır. İkisi de yerinde durduğu sürece dokunmanız gerekmez.

| Yer | Ayar | Değer |
|---|---|---|
| nuget.org → Trusted Publishing | Repository Owner | `erenemrearik` |
| | Repository | `TRCrypto.Net` |
| | Workflow File | `release.yml` |
| | Environment | `nuget` |
| | Glob Pattern | `TRCrypto.*` |
| GitHub → Settings → Environments | Ortam adı | `nuget` |
| | Required reviewers | siz |

> [!WARNING]
> Ortam adı iki tarafta da tam olarak `nuget` olmalıdır. Biri değişirse NuGet anahtar
> vermez ve yayın adımı hata verir. Hata mesajı politikanın eşleşmediğini söyler ama
> hangi alanın eşleşmediğini söylemez; ilk bakılacak yer bu tablodur.

---

## Her sürümde

### 1. Sürüm numarasına karar verin

| Değişiklik | Sürüm | Örnek |
|---|---|---|
| Hata düzeltmesi, genel API aynı | yama | `0.2.0` → `0.2.1` |
| Yeni özellik, mevcut kod kırılmıyor | minör | `0.2.0` → `0.3.0` |
| Mevcut kod kırılıyor ya da CryptoExchange.Net ana sürümü değişti | minör (1.0 öncesi) | `0.2.0` → `0.3.0` |

Ön sürümlerde sona `-preview.N` eklenir. Etiketinde kısa çizgi olan her sürüm NuGet'te ve
GitHub'da otomatik olarak ön sürüm işaretlenir.

> [!IMPORTANT]
> NuGet'te yayınlanan bir sürüm **silinemez**, yalnızca listeden kaldırılabilir. Aynı
> numara bir daha kullanılamaz. Emin değilseniz `-preview.N` ile çıkarın.

### 2. Değişiklik günlüğünü güncelleyin

`CHANGELOG.md` içindeki ilk başlık yayınlanacak sürüm olur:

```markdown
## [0.3.0-preview.1] - 14 Kasım 2026
```

Bu başlık önemlidir: sitenin giriş sayfası yayınlanan sürümü buradan okur. Başlık
`[Yayınlanmadı]` kaldıkça site "henüz yayınlanmadı" der.

### 3. Sürüm numarasını belgelere yazın

Kurulum notlarında sürüm geçer ve paket README'leri NuGet'e paketin içinde gider:

- `README.md` ve `README.en.md`
- `src/TRCrypto.BtcTurk/README.md`, `src/TRCrypto.BinanceTR/README.md`,
  `src/TRCrypto.CoinTR/README.md`
- `Directory.Build.props` içindeki `VersionPrefix` (etiket zaten sürümü belirler; bu
  yalnızca yerel derlemelerin doğru numarayı göstermesi içindir)

Unutursanız sorun değil: denetim aracı README'lerdeki sürümün değişiklik günlüğüyle
aynı olmasını zorunlu tutar ve CI durur.

### 4. Doğrulayın

```cmd
dotnet build -c Release
dotnet test -c Release --filter "FullyQualifiedName!~IntegrationTests"
node tools/site/build.mjs
node tools/site/check.mjs
node tools/audit/docs-vs-code.mjs
dotnet run --project examples/TRCrypto.Examples.Console -c Release
```

Sonuncusu canlı borsalara bağlanır ve en çok atlanan adımdır. Atlamayın: 0.2.0
hazırlanırken derleme ve testlerin tamamı geçtiği halde her paylaşılan çağrının
reddedildiğini yalnızca bu adım gösterdi.

### 5. Commit'leyip gönderin, CI'ın yeşil olmasını bekleyin

```cmd
git add -A
git commit -m "Prepare 0.3.0-preview.1"
git push origin main
```

CI üç iş çalıştırır: derleme ve test, dokümantasyon sitesi, secret taraması. Üçü yeşil
olmadan etikete geçmeyin. Durum: <https://github.com/erenemrearik/TRCrypto.Net/actions>

### 6. Etiketi atın

```cmd
git tag -a v0.3.0-preview.1 -m "TRCrypto.Net 0.3.0-preview.1"
git push origin v0.3.0-preview.1
```

Etiketin başında `v` olmalıdır; yayın akışı yalnızca `v*.*.*` biçimindeki etiketlerde
başlar.

### 7. GitHub'da onay verin

Etiket gönderilince **Release** akışı başlar: derler, birim testlerini çalıştırır, üç
paketi ve sembol paketlerini üretir. Sonra **NuGet'e yayinla** işi sizin onayınızda durur.

1. <https://github.com/erenemrearik/TRCrypto.Net/actions> adresinde **Release** çalışmasını
   açın
2. Sarı **Review deployments** düğmesine basın
3. `nuget` kutusunu işaretleyip **Approve and deploy** deyin

Onaylamadan hiçbir şey nuget.org'a gitmez. Onaydan önce **Paket icerigini dogrula**
adımına bakabilirsiniz; üç paketin içindeki beş platform klasörünü ve README'yi listeler.

### 8. Yayını doğrulayın

Paketler önce doğrulamadan geçer (imza ve zararlı yazılım taraması). İlk 10 ile 20 dakika
paket sayfası açılmayabilir ve `dotnet add package` sürümü bulamayabilir; bu normaldir.

Yayın tamamlandığında depoya bakmayan ayrı bir klasörde deneyin:

```cmd
mkdir %TEMP%\trcrypto-dogrulama
cd %TEMP%\trcrypto-dogrulama
dotnet new console
dotnet add package TRCrypto.BtcTurk --prerelease
dotnet add package TRCrypto.BinanceTR --prerelease
dotnet add package TRCrypto.CoinTR --prerelease
```

Paketler: <https://www.nuget.org/profiles/arikerenemre>

### 9. Durum belgelerini güncelleyin

`docs/DURUM.md` ve `CLAUDE.md` içinde sürümün yayında olduğunu yazın, siteyi yeniden
üretip commit'leyin.

---

## Sorun giderme

| Belirti | Neden ve çözüm |
|---|---|
| CI kod değişmeden kırmızıya döndü, `NU1902` | Bir bağımlılık için yeni güvenlik kaydı açıldı. Önce bağımlılığın gerekli olup olmadığına, sonra yükseltmeye bakılır. Ayrıntı `CLAUDE.md` içinde |
| **NuGet oturumu (OIDC)** adımı hata verdi | NuGet politikası eşleşmedi. Yukarıdaki tablodaki dört alanı kontrol edin, en sık neden ortam adı |
| Yayın adımı yeşil ama yeni sürüm yok | Aynı numara daha önce yayınlanmış. Akış `--skip-duplicate` kullanır, bu yüzden hata vermeden atlar. Yeni bir numara seçin |
| Onay düğmesi hiç çıkmadı | GitHub'daki `nuget` ortamı silinmiş ya da onaylayıcı listesi boş |
| Paket sayfası 404 | Doğrulama sürüyor; 20 dakika bekleyin |
| Yanlış sürüm yayınlandı | Silinemez. nuget.org'da paketin **Manage** sayfasından listeden kaldırın ve düzeltilmiş yeni bir sürüm çıkarın |

---

## Akışın kendisi

Yayın akışı `.github/workflows/release.yml` dosyasıdır. Değiştirirseniz dikkat edilecek
iki şey var: dosya adı NuGet politikasında yazılıdır, ve yayın işinin `environment: nuget`
satırı onay kapısının kendisidir. İkisi de sessizce bozulabilir.
