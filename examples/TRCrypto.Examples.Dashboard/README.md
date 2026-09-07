# Piyasa Panosu

Üç borsanın Türk lirası paritelerini yan yana gösteren canlı bir ekran.

```bash
dotnet run --project examples/TRCrypto.Examples.Dashboard
```

Ardından <http://localhost:5180> adresini açın. Kimlik bilgisi gerekmez; yalnızca herkese
açık veri kullanılır.

## Neden var

**Veri tarayıcıdan borsalara doğrudan gitmiyor.** Önce bu süreçte TRCrypto.Net üzerinden
akıyor, sonra WebSocket ile sayfaya itiliyor. Bu ayrım önemlidir: tarayıcıdan doğrudan
borsaya bağlanan bir sayfa güzel görünür ama kütüphaneyi hiç çalıştırmaz, dolayısıyla
hiçbir şey doğrulamaz.

Ekranın iki işi var.

**Tezi görünür kılmak.** Her fiyatın altında o borsanın native sembol adı yazıyor:
`BTCTRY` ve `BTC_TRY`. Abone olan kodda bu adların hiçbiri geçmiyor; üç borsa da tek bir
`ITickerSocketClient` ile ve tek bir `SharedSymbol` ile dinleniyor.

**Birim testlerinin göremediğini göstermek.** Uzun süre çalışan, çok abonelikli bir ekran
sızıntıyı, abonelik çakışmasını ve yeniden bağlanma davranışını ortaya çıkarır. Bu projede
iki ciddi hata tam olarak böyle bulundu: Binance TR'de paylaşılan durum yüzünden yalnızca
son abonelik mesaj alıyordu, BtcTurk'te tüm ticker akışı abonelik onayı alıp hiç veri
göndermiyordu. İkisi de "abonelik başarılı" diyordu.

> [!NOTE]
> Bu bir test değil, gözlem aracıdır. CI'da kırılmaz ve kimse bakmazsa bir şey söylemez.
> Regresyonlara karşı asıl koruma birim testleri ile haftalık canlı testlerdir.

## Ekranda ne var

| Bölüm | Ne gösterir |
|---|---|
| Tahta | Varlık başına bir satır, borsa başına fiyat ve 24 saatlik değişim |
| Fark sütunu | O an fiyat bildiren borsalar arasındaki en yüksek ile en düşük fiyat arası |
| Native ad | Her borsanın aynı `SharedSymbol` için ürettiği sembol |
| Bağlantı sağlığı | Abonelik sayısı, gelen mesaj, son mesajın yaşı, kopma sayısı |

Fiyat değiştiğinde hücre kısa süre yeşil ya da kırmızı yanar. On beş saniyedir mesaj
gelmeyen bir borsa "gecikmeli" olarak işaretlenir.

## Yapı

| Dosya | Sorumluluk |
|---|---|
| `MarketBoard.cs` | Borsadan bağımsız yüzeyle abone olur, durumu tutar |
| `Program.cs` | Statik sayfayı sunar, anlık görüntüyü WebSocket ile iter |
| `wwwroot/index.html` | Ekranın kendisi; harici bağımlılığı yoktur |

`MarketBoard` içinde borsaya göre dallanan tek bir satır yoktur. Yeni bir borsa eklemek,
listeye bir `ITickerSocketClient` daha eklemekten ibarettir.

Sayfa her 250 milisaniyede bir toplu anlık görüntü alır. Her güncellemeyi tek tek itmek,
saniyede yüzlerce mesaj gelen akışlarda tarayıcıyı boğardı.
