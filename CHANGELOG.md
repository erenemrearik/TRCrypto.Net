# Değişiklik Günlüğü

Bu dosyanın biçimi [Keep a Changelog](https://keepachangelog.com/tr/1.1.0/) temel alır ve
proje [Semantic Versioning](https://semver.org/lang/tr/) kullanır.

## [Yayınlanmadı]

Proje henüz NuGet'e yayınlanmadı. İlk sürüm `0.1.0-preview` olarak planlanıyor.

Güncel durum ve sonraki adımlar: [docs/DURUM.md](docs/DURUM.md)

### Eklendi

- **Proje temeli.** Solution yapısı, merkezi paket yönetimi, beş hedef platform
  (`net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `netstandard2.1`), deterministic
  Release build, SourceLink, sembol paketi
- **`TRCrypto.BtcTurk`.** BtcTurk public piyasa verisi:
  - `GetExchangeInfoAsync`: pariteler, varlıklar, sunucu saati
  - `GetServerTimeAsync`
  - `GetTickersAsync` · `GetTickerAsync` · `GetTickersByQuoteAssetAsync`
  - `GetOrderBookAsync`
  - `GetTradesAsync`
- **Kimlik doğrulama.** HMAC-SHA256 imzalama (`X-PCK` / `X-Stamp` / `X-Signature`)
- **Hesap ve emir uçları.** Bakiye, işlem geçmişi, açık emirler, emir geçmişi,
  emir sorgulama, emir oluşturma ve iptal
- **Mum verisi.** Ayrı bir sunucu üzerinde çalışan grafik API'si
- **WebSocket.** Ticker, tüm pariteler, işlem ve emir defteri akışları; yeniden bağlanma ve
  abonelik geri kurma dahil
- **Borsadan bağımsız yüzey** (`SharedApis`). REST tarafında sembol, ticker, emir
  defteri, işlem, mum, bakiye ve emir arayüzleri; WebSocket tarafında ticker, işlem
  ve emir defteri arayüzleri; `Discover()` ile yetenek keşfi
- **İstek limitleri.** Resmi dokümantasyondan alınan değerlerle uygulanıyor
- **Bağımlılık enjeksiyonu:** `services.AddTRCryptoBtcTurk(...)`
- **`TRCrypto.BinanceTR`.** Binance TR piyasa verisi ve hesap uçları:
  - `GetServerTimeAsync` · `GetSymbolsAsync`
  - `GetOrderBookAsync` · `GetAggregatedTradesAsync`
  - **Hesap ve emir uçları:** hesap bilgisi, emir oluşturma ve iptali, emir listesi,
    emir ayrıntısı, işlem geçmişi
  - **Kimlik doğrulama:** HMAC-SHA256 imzalama (`X-MBX-APIKEY` ve `signature`)
  - **WebSocket.** Ticker, işlem, toplu işlem, emir defteri (tam ve kademeli) ve
    mum akışları
  - **Borsadan bağımsız yüzey.** REST tarafında sembol, emir defteri, işlem, bakiye ve
    emir arayüzleri; WebSocket tarafında ticker, işlem, emir defteri ve mum arayüzleri
  - `services.AddTRCryptoBinanceTR(...)`
- **Bağımlılık enjeksiyonu.** Tek çağrı hem REST hem WebSocket istemcisini kaydeder;
  socket istemcisi tekil olarak paylaşılır
- **Belgeler.** Borsa başına API anahtarı rehberi (`docs/credentials/`), resmi kaynaklı
  endpoint envanteri (`docs/vendor/`), teknik spesifikasyon ve doğrulama ekleri (`docs/spec/`)
- **Secret koruması.** `.gitignore` blokları, `gitleaks` yapılandırması, pre-commit
  kancası; kimlik bilgilerinin loglara sızmadığını doğrulayan birim testleri

### Bilinen sınırlamalar

- **BtcTurk:** kullanıcıya özel socket akışları (emir/bakiye bildirimleri) henüz yok.
  Socket girişi canlı doğrulandı, ancak mesaj gövdeleri hesapta hareket olmadan
  gelmiyor ve hiçbir yerde belgelenmemiş
- **BtcTurk:** okuma uçları ve imzalama gerçek bir hesaba karşı doğrulandı; emir
  oluşturma ve iptal, gerçek emir vermeyi gerektirdiği için bilinçli olarak ertelendi
- **Binance TR:** private uçlar canlı bir hesaba karşı çalıştırılmadı. Şema resmi
  dokümantasyondan alındı, imzalama yayımlanmış test vektörüyle doğrulandı; gerçek
  hesap doğrulaması anahtar geldiğinde yapılacak
- **Binance TR:** REST ticker yok, çünkü borsa bu veriyi anahtarsız sunmuyor. Ticker
  yalnızca WebSocket üzerinden alınabilir
- Paribu ve Bitexen adaptörleri planlandı, başlanmadı

### Notlar

Geliştirme sırasında her iki borsanın API'sinde de, resmi dokümantasyonda yer almayan
davranışlar tespit edildi; tümü `docs/spec/` ekinde (D-1…D-44) belgelendi. Öne çıkanlar:

- **BtcTurk:** `code` alanı uçlar arasında farklı tiplerde (sayı / metin) dönüyor
- **Binance TR:** `market/trades` ve `market/klines` uçları başarı koduyla **boş liste**
  döndürüyor. Hata dönmedikleri için sessizce "işlem yok" olarak okunabilirler
- **Binance TR:** tek borsa içinde üç ayrı sembol biçimi kullanılıyor (`BTC_TRY`
  REST'te, `btctry` abonelikte, `BTCTRY` akış gövdesinde)
- İki borsa zıt serileştirme ayarı gerektiriyor: BtcTurk'te alan eşleşmesi harf
  büyüklüğüne duyarsız, Binance TR'de duyarlı olmalı
