# API Kimlik Bilgileri — Genel Rehber

Bu klasör, TRCrypto ile bir Türk kripto borsasına bağlanmak için gereken API anahtarlarının
**nasıl alınacağını, nasıl saklanacağını ve nasıl kullanılacağını** anlatır.

İki okuyucu kitlesi için yazılmıştır:

- **Kütüphaneyi kullanan geliştirici** — kendi hesabını kendi uygulamasına bağlayacak kişi
- **TRCrypto'ya katkı veren geliştirici** — private endpoint testlerini çalıştıracak kişi

| Borsa | Rehber | Durum |
|---|---|---|
| BtcTurk | [btcturk.md](btcturk.md) | ✅ Hazır |
| Binance TR | `binance-tr.md` | ⏳ Adapter ile birlikte |
| Paribu | `paribu.md` | ⏳ Adapter ile birlikte |
| Bitexen | `bitexen.md` | ⏳ Faz 2 |

---

## Önce şunu bilin: çoğu şey için anahtar GEREKMEZ

Piyasa verisi (fiyat, emir defteri, işlemler, pariteler) **herkese açıktır**. Bunlar için
API anahtarı oluşturmanıza gerek yoktur:

```csharp
var client = new BtcTurkRestClient();          // kimlik bilgisi yok
var ticker = await client.SpotApi.ExchangeData.GetTickerAsync("BTCTRY");
```

Anahtar yalnızca **hesabınıza özel** işlemler için gerekir: bakiye görüntüleme, emir verme,
emir iptali, işlem geçmişi.

---

## Altın kural: en az yetki (least privilege)

Bir API anahtarı, verdiğiniz izinlerin tamamını yapabilen bir vekildir. Anahtar sızarsa,
verdiğiniz her izin saldırganın eline geçer.

> ### ⚠️ Çekim (Withdrawal / Para Çekme) iznini AÇMAYIN
>
> TRCrypto hiçbir özelliği için çekim iznine ihtiyaç duymaz. Çekim izni olan bir anahtar
> sızarsa **varlıklarınız geri alınamaz biçimde transfer edilebilir**. Sızan bir okuma
> anahtarı can sıkıcıdır; sızan bir çekim anahtarı paranızı kaybettirir.

Yapmanız gereken:

- Yalnızca ihtiyacınız olan izinleri işaretleyin
- **IP allow-list'i mutlaka doldurun** — anahtar başka bir IP'den kullanılamasın
- Emir denemeleri için ana hesabınızı değil, **ayrı ve düşük bakiyeli bir hesap** kullanın
- Anahtarları düzenli aralıklarla yenileyin
- Farklı ortamlar (geliştirme / üretim) için **ayrı anahtarlar** kullanın

---

## Anahtarlarınızı nerede saklamalısınız

### ❌ Asla

- Kaynak koda gömmek (`new BtcTurkCredentials("abc123", "xyz")`)
- `appsettings.json` içine yazıp commit etmek
- Slack/WhatsApp/e-posta ile göndermek
- Ekran görüntüsü veya log çıktısı paylaşırken maskelememek

> Bir secret git geçmişine bir kez girerse, dosyayı silmek yetmez — geçmişte kalır.
> Tek doğru çözüm **anahtarı borsada iptal edip yenisini oluşturmaktır**.

### ✅ Yerel geliştirmede: `dotnet user-secrets`

Değerler proje klasörünün **tamamen dışında**, kullanıcı profilinizde saklanır. Yanlışlıkla
commit etmeniz fiziksel olarak mümkün değildir.

```bash
cd tests/TRCrypto.BtcTurk.IntegrationTests
dotnet user-secrets init
dotnet user-secrets set "BtcTurk:ApiKey"    "buraya-public-key"
dotnet user-secrets set "BtcTurk:ApiSecret" "buraya-base64-secret"
```

Okuma:

```csharp
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var credentials = new BtcTurkCredentials(
    config["BtcTurk:ApiKey"]!,
    config["BtcTurk:ApiSecret"]!);
```

### ✅ Alternatif: ortam değişkenleri / `.env`

[`.env.example`](../../.env.example) dosyasını `.env` adıyla kopyalayıp doldurun.
`.env` `.gitignore` tarafından bloklanmıştır; `.env.example` ise şablondur ve gerçek
değer içermez.

### ✅ CI ortamında: GitHub Actions Secrets

Repository → Settings → Secrets and variables → Actions. Değerler ortam değişkeni olarak
inject edilir; test kodu `IConfiguration` üzerinden okur, böylece yerel ve CI'da **aynı
kod yolu** çalışır.

### ✅ Üretimde

Azure Key Vault, AWS Secrets Manager, HashiCorp Vault veya platformunuzun secret store'u.

---

## Uygulamaya bağlama (DI)

```csharp
// Tek çağrı hem REST hem WebSocket istemcisini kaydeder; kimlik bilgisi ikisine de uygulanır.
builder.Services.AddTRCryptoBtcTurk(options =>
{
    options.ApiCredentials = new BtcTurkCredentials(
        builder.Configuration["BtcTurk:ApiKey"]!,
        builder.Configuration["BtcTurk:ApiSecret"]!);
});

// Kullanım — client yeniden kullanılabilir, her istek için yenisini OLUŞTURMAYIN
public sealed class PortfolioService(IBtcTurkRestClient client)
{
    public async Task<decimal?> GetTryBalanceAsync(CancellationToken ct = default)
    {
        var result = await client.SpotApi.Account.GetBalancesAsync(ct);
        if (!result.Success)
            return null;
        return result.Data.FirstOrDefault(b => b.Asset == "TRY")?.Free;
    }
}
```

---

## TRCrypto secret'inizi nasıl koruyor

- `BtcTurkCredentials.ToString()` ham secret yerine **maskeli fingerprint** döndürür
- Secret hiçbir log kaydına, exception mesajına veya telemetry alanına yazılmaz
- İstek gövdesi loglandığında `X-Signature` ve secret alanları redaction'dan geçer
- Bunlar birim testleriyle doğrulanır — log çıktısı yakalanır ve ham secret'ın hiçbir
  yerde geçmediği assert edilir

---

## Anahtarım sızdıysa ne yapmalıyım?

1. **Derhal** borsanın panelinden anahtarı silin (dakikalar önemlidir)
2. Hesap hareketlerinizi kontrol edin
3. Yeni anahtar oluşturun, IP allow-list'i doldurun
4. Sızıntı git geçmişindeyse: anahtar zaten iptal edildiği için geçmişi temizlemek
   ikincil önceliktir — ama repo herkese açıksa `git filter-repo` ile temizleyin

---

## Sorun giderme

| Belirti | Olası neden |
|---|---|
| İmza geçersiz / yetkisiz | Secret Base64 ise decode edilmemiş olabilir; anahtar/secret yer değiştirmiş olabilir |
| İstek reddedildi, anahtar doğru | IP allow-list'te bulunduğunuz IP yok |
| Zaman damgası hatası | Sistem saatiniz kaymış — NTP ile senkronize edin |
| İzin hatası | Anahtarda ilgili izin işaretli değil |
| Bakiye boş dönüyor | Anahtar farklı bir hesaba ait olabilir |
