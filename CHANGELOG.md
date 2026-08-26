# Değişiklik Günlüğü

Bu dosyanın biçimi [Keep a Changelog](https://keepachangelog.com/tr/1.1.0/) temel alır ve
proje [Semantic Versioning](https://semver.org/lang/tr/) kullanır.

## [Yayınlanmadı]

Proje henüz NuGet'e yayınlanmadı. İlk sürüm `0.1.0-preview` olarak planlanıyor.

Güncel durum ve sonraki adımlar: [docs/DURUM.md](docs/DURUM.md)

### Eklendi

- **Proje temeli** — solution yapısı, merkezi paket yönetimi, beş hedef platform
  (`net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `netstandard2.1`), deterministic
  Release build, SourceLink, sembol paketi
- **`TRCrypto.BtcTurk`** — BtcTurk public piyasa verisi:
  - `GetExchangeInfoAsync` — pariteler, varlıklar, sunucu saati
  - `GetServerTimeAsync`
  - `GetTickersAsync` · `GetTickerAsync` · `GetTickersByQuoteAssetAsync`
  - `GetOrderBookAsync`
  - `GetTradesAsync`
- **Kimlik doğrulama** — HMAC-SHA256 imzalama (`X-PCK` / `X-Stamp` / `X-Signature`)
- **Hesap ve emir uçları** — bakiye, işlem geçmişi, açık emirler, emir geçmişi,
  emir sorgulama, emir oluşturma ve iptal
- **Mum verisi** — ayrı bir host üzerinde çalışan grafik API'si
- **WebSocket** — ticker, işlem ve emir defteri akışları; yeniden bağlanma ve
  abonelik geri kurma dahil
- **Borsadan bağımsız yüzey** (`SharedApis`) — REST tarafında sembol, ticker, emir
  defteri, işlem, mum, bakiye ve emir arayüzleri; WebSocket tarafında ticker, işlem
  ve emir defteri arayüzleri; `Discover()` ile yetenek keşfi
- **İstek limitleri** — resmi dokümantasyondan alınan değerlerle uygulanıyor
- **Bağımlılık enjeksiyonu** — `services.AddTRCryptoBtcTurk(...)`
- **Belgeler** — borsa başına API anahtarı rehberi (`docs/credentials/`), resmi kaynaklı
  endpoint envanteri (`docs/vendor/`), teknik spesifikasyon ve doğrulama ekleri (`docs/spec/`)
- **Secret koruması** — `.gitignore` blokları, `gitleaks` yapılandırması, pre-commit
  kancası; kimlik bilgilerinin loglara sızmadığını doğrulayan birim testleri

### Bilinen sınırlamalar

- Kullanıcıya özel socket akışları (emir/bakiye bildirimleri) henüz yok
- Kimlik doğrulama gerektiren uçlar gerçek bir hesaba karşı çalıştırılmadı; imzalama
  sabit test vektörleriyle, uçlar contract testleriyle doğrulandı
- Binance TR, Paribu ve Bitexen adaptörleri planlandı, başlanmadı

### Notlar

Geliştirme sırasında BtcTurk API'sinde, resmi dokümantasyonda yer almayan bazı
davranışlar tespit edildi ve `docs/spec/` ekinde belgelendi. Bunların en önemlisi,
`code` alanının uçlar arasında farklı tiplerde (sayı / metin) dönmesidir.
